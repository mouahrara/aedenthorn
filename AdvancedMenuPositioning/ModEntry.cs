using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace AdvancedMenuPositioning
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static Point lastMousePosition;
		private static IClickableMenu currentlyDragging;
		private static int RightClickLimiter;
		private static readonly List<ClickableComponent> adjustedComponents = new();
		private static readonly List<IClickableMenu> adjustedMenus = new();
		private static readonly List<IClickableMenu> detachedMenus = new();

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking_MoveMenus;
			helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking_RightClick;
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			helper.Events.Input.MouseWheelScrolled += Input_MouseWheelScrolled;
			helper.Events.Display.RenderingActiveMenu += Display_RenderingActiveMenu;
			helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
		}

		private void GameLoop_UpdateTicking_MoveMenus(object sender, UpdateTickingEventArgs e)
		{
			if (!Context.IsWorldReady || !Config.EnableMod)
				return;

			Point mousePosition = Game1.getMousePosition();
			Point delta = new((int)Utility.ModifyCoordinateForUIScale(mousePosition.X - lastMousePosition.X), (int)Utility.ModifyCoordinateForUIScale(mousePosition.Y - lastMousePosition.Y));
			bool moveKeybindPressed = IsKeybindPressed(Config.MoveKeys.Keybinds[0].Buttons);

			if (moveKeybindPressed)
			{
				if (!TryMoveActiveMenu(delta))
				{
					if (!TryMoveMenuFromList(Game1.onScreenMenus, delta))
					{
						TryMoveMenuFromList(detachedMenus, delta);
					}
				}
			}
			else
			{
				currentlyDragging = null;
			}
			lastMousePosition = mousePosition;
			foreach (IClickableMenu menu in detachedMenus)
			{
				if (menu.isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY())))
				{
					menu.performHoverAction((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()));
				}
			}
		}

		private void GameLoop_UpdateTicking_RightClick(object sender, UpdateTickingEventArgs e)
		{
			if (!Context.IsWorldReady || !Config.EnableMod)
				return;

			if (detachedMenus.Count > 0)
			{
				bool isRightClickHeld = Helper.Input.IsDown(SButton.MouseRight) || Helper.Input.IsSuppressed(SButton.MouseRight);
				bool isMouseOutsideActiveMenu = Game1.activeClickableMenu is null || !Game1.activeClickableMenu.isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()));

				if (isRightClickHeld)
				{
					if (isMouseOutsideActiveMenu)
					{
						if (RightClickLimiter < 30)
						{
							RightClickLimiter++;
						}
						else
						{
							RightClickLimiter -= 3;
							HandleClickOnDetachedMenu((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()), SButton.MouseRight);
						}
					}
				}
				else
				{
					RightClickLimiter = 0;
				}
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Context.IsWorldReady || !Config.EnableMod)
				return;

			bool isMouseOutsideActiveMenu = Game1.activeClickableMenu is null || !Game1.activeClickableMenu.isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()));

			if (!isMouseOutsideActiveMenu)
			{
				bool detachKeybindPressed = IsKeybindPressed(Config.DetachKeys.Keybinds[0].Buttons);

				if (detachKeybindPressed)
				{
					detachedMenus.Add(Game1.activeClickableMenu);
					Game1.activeClickableMenu = null;
					Helper.Input.Suppress(e.Button);
					Game1.playSound("bigDeSelect");
				}
			}
			else
			{
				if (detachedMenus.Count > 0)
				{
					bool moveKeybindPressed = IsKeybindPressed(Config.MoveKeys.Keybinds[0].Buttons);

					if (!moveKeybindPressed)
					{
						bool closeKeybindPressed = IsKeybindPressed(Config.CloseKeys.Keybinds[0].Buttons);
						bool detachKeybindPressed = IsKeybindPressed(Config.DetachKeys.Keybinds[0].Buttons);

						if (closeKeybindPressed)
						{
							for (int i = detachedMenus.Count - 1; i >= 0; i--)
							{
								if (detachedMenus[i].isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY())))
								{
									detachedMenus.RemoveAt(i);
									Helper.Input.Suppress(e.Button);
									Game1.playSound("bigDeSelect");
									break;
								}
							}
						}
						else if (detachKeybindPressed)
						{
							if (Game1.activeClickableMenu is null)
							{
								for (int i = detachedMenus.Count - 1; i >= 0; i--)
								{
									if (detachedMenus[i].isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY())))
									{
										Game1.activeClickableMenu = detachedMenus[i];
										detachedMenus.RemoveAt(i);
										Helper.Input.Suppress(e.Button);
										Game1.playSound("bigSelect");
										break;
									}
								}
							}
						}
						else if (e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
						{
							HandleClickOnDetachedMenu((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()), e.Button);
						}
					}
				}
			}
		}

		private void Input_MouseWheelScrolled(object sender, MouseWheelScrolledEventArgs e)
		{
			if (!Context.IsWorldReady)
				return;

			bool isMouseOutsideActiveMenu = Game1.activeClickableMenu is null || !Game1.activeClickableMenu.isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY()));

			if (isMouseOutsideActiveMenu)
			{
				for (int i = detachedMenus.Count - 1; i >= 0; i--)
				{
					if (detachedMenus[i].isWithinBounds((int)Utility.ModifyCoordinateForUIScale(Game1.getMouseX()), (int)Utility.ModifyCoordinateForUIScale(Game1.getMouseY())))
					{
						detachedMenus[i].receiveScrollWheelAction(e.Delta);
						SHelper.Input.SuppressScrollWheel();
						break;
					}
				}
			}
		}

		private void Display_RenderingActiveMenu(object sender, RenderingActiveMenuEventArgs e)
		{
			if (!Context.IsWorldReady)
				return;

			if (detachedMenus.Count > 0)
			{
				bool showMenuBackground = Game1.options.showMenuBackground;
				bool showClearBackgrounds = Game1.options.showClearBackgrounds;

				Game1.options.showMenuBackground = false;
				Game1.options.showClearBackgrounds = true;
				foreach (IClickableMenu menu in detachedMenus)
				{
					menu.GetType().GetField("drawBG", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)?.SetValue(menu, false);
					menu.draw(e.SpriteBatch);
				}
				Game1.options.showMenuBackground = showMenuBackground;
				Game1.options.showClearBackgrounds = showClearBackgrounds;
			}
		}

		private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
		{
			adjustedComponents.Clear();
			adjustedMenus.Clear();
			detachedMenus.Clear();
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => Helper.WriteConfig(Config)
				);

				// Main section
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.EnableMod,
					setValue: value => Config.EnableMod = value
				);
				gmcm.AddKeybindList(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MoveKeys.Name"),
					getValue: () => Config.MoveKeys,
					setValue: value => Config.MoveKeys = value
				);
				gmcm.AddKeybindList(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.DetachKeys.Name"),
					getValue: () => Config.DetachKeys,
					setValue: value => Config.DetachKeys = value
				);
				gmcm.AddKeybindList(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.CloseKeys.Name"),
					getValue: () => Config.CloseKeys,
					setValue: value => Config.CloseKeys = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.StrictKeybindings.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.StrictKeybindings_Tooltip"),
					getValue: () => Config.StrictKeybindings,
					setValue: value => Config.StrictKeybindings = value
				);
			}
		}
	}
}
