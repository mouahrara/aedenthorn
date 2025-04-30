using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CustomToolbar
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

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
			helper.Events.Input.ButtonPressed += Input_ButtonPressed;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Toolbar), nameof(Toolbar.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Toolbar_draw_prefix)))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Toolbar), nameof(Toolbar.isWithinBounds)),
					prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Toolbar_isWithinBounds_prefix)))
				);
				harmony.Patch(
					original: AccessTools.Constructor(typeof(IClickableMenu), new Type[] { typeof(int), typeof( int), typeof(int), typeof(int), typeof(bool) }),
					postfix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Toolbar_postfix)))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
		{
			if (e.Button == Config.RotateKey)
			{
				foreach (IClickableMenu menu in Game1.onScreenMenus)
				{
					if (menu is Toolbar)
					{
						if (menu.isWithinBounds(Game1.getMouseX(), Game1.getMouseY()))
						{
							Monitor.Log($"Switching orientation to {(Config.Vertical ? "horizontal" : "vertical")}");
							Config.Vertical = !Config.Vertical;
							Helper.WriteConfig(Config);
							Game1.playSound("dwop");
						}
					}
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ShowWithActiveMenu.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ShowWithActiveMenu.Tooltip"),
					getValue: () => Config.ShowWithActiveMenu,
					setValue: value => Config.ShowWithActiveMenu = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Vertical.Name"),
					getValue: () => Config.Vertical,
					setValue: value => Config.Vertical = value
				);
				gmcm.AddKeybind(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.RotateKey.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.RotateKey.Tooltip"),
					getValue: () => Config.RotateKey,
					setValue: value => Config.RotateKey = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.PinToolbar.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.PinToolbar.Tooltip"),
					getValue: () => Game1.options.pinToolbarToggle,
					setValue: value => Game1.options.pinToolbarToggle = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.SetPosition.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.SetPosition.Tooltip"),
					getValue: () => Config.SetPosition,
					setValue: value => Config.SetPosition = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MarginX.Name"),
					getValue: () => Config.MarginX,
					setValue: value => Config.MarginX = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MarginY.Name"),
					getValue: () => Config.MarginY,
					setValue: value => Config.MarginY = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.OffsetX.Name"),
					getValue: () => Config.OffsetX,
					setValue: value => Config.OffsetX = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.OffsetY.Name"),
					getValue: () => Config.OffsetY,
					setValue: value => Config.OffsetY = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.PinnedPosition.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.PinnedPosition.Tooltip"),
					allowedValues: new string[] { "top", "bottom", "left", "right" },
					formatAllowedValue: (string value) => SHelper.Translation.Get("GMCM.PinnedPosition." + value),
					getValue: () => Config.PinnedPosition,
					setValue: value => Config.PinnedPosition = value
				);
			}
		}
	}
}
