using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Tools;

namespace CustomToolbarAndroid
{
	public partial class ModEntry
	{
		private static readonly Type TutorialManagerType = Type.GetType("StardewValley.Menus.TutorialManager, StardewValley");
		private static readonly Type VirtualJoypadType = Type.GetType("StardewValley.Mobile.VirtualJoypad, StardewValley");
		private static readonly Type TapToMoveType = Type.GetType("StardewValley.Mobile.TapToMove, StardewValley");
		private static readonly object TutorialType_USE_HOE = Enum.Parse(Type.GetType("tutorialType, StardewValley"), "USE_HOE");
		private static readonly FieldInfo VerticalToolbarField = typeof(Options).GetField("verticalToolbar", BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo AlignTopField = typeof(Toolbar).GetField("alignTop", BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo TransparencyField = typeof(Toolbar).GetField("transparency", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo MaxItemSlotSizeField = typeof(Game1).GetField("maxItemSlotSize", BindingFlags.Public | BindingFlags.Static);
		private static readonly FieldInfo VirtualJoypadField = typeof(Game1).GetField("virtualJoypad", BindingFlags.Public | BindingFlags.Static);
		private static readonly FieldInfo ShowTheTutorialsField = TutorialManagerType.GetField("showTheTutorials", BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo XPositionOnScreenField = typeof(IClickableMenu).GetField("xPositionOnScreen", BindingFlags.Public | BindingFlags.Instance);
		private static readonly FieldInfo StartTapPositionXField = typeof(Toolbar).GetField("_startTapPositionX", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo StartIndexField = typeof(Toolbar).GetField("_startIndex", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo DrawStartIndexField = typeof(Toolbar).GetField("_drawStartIndex", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo ShowTooltipField = typeof(Toolbar).GetField("_showTooltip", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo IgnoreReleaseField = typeof(Toolbar).GetField("_ignoreRelease", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo ToolbarPressedField = typeof(Toolbar).GetField("toolbarPressed", BindingFlags.Public | BindingFlags.Static);
		private static readonly FieldInfo HoverTicksAtStartField = typeof(Toolbar).GetField("hoverTicksAtStart", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo ButtonsField = typeof(Toolbar).GetField("buttons", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo NextToolIndexField = typeof(Toolbar).GetField("_nextToolIndex", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo MostRecentlyChosenMeleeWeaponField = TapToMoveType.GetField("mostRecentlyChosenMeleeWeapon", BindingFlags.Public | BindingFlags.Static);
		private static readonly FieldInfo DidInitiateItemStowField = typeof(Game1).GetField("_didInitiateItemStow", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo HoverTitleField = typeof(Toolbar).GetField("hoverTitle", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo HoverItemField = typeof(Toolbar).GetField("hoverItem", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo LastHoverItemField = typeof(Toolbar).GetField("lastHoverItem", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo TooltipPositionField = typeof(Toolbar).GetField("_tooltipPosition", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo YOffsetField = typeof(Toolbar).GetField("yOffset", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly FieldInfo ToolbarPaddingXField = typeof(Game1).GetField("toolbarPaddingX", BindingFlags.Public | BindingFlags.Static);
		private static readonly MethodInfo ItemSlotSizeGetter = typeof(Toolbar).GetProperty("itemSlotSize", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo MaxVisibleItemsGetter = typeof(Toolbar).GetProperty("maxVisibleItems", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo ViewportGetter = typeof(Toolbar).GetProperty("viewport", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo ScreenHeightGetter = typeof(Toolbar).GetProperty("screenHeight", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo InstanceGetter = TutorialManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetGetMethod();
		private static readonly MethodInfo MaxScrollIndexGetter = typeof(Toolbar).GetProperty("maxScrollIndex", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true);
		private static readonly MethodInfo JoystickHeldGetter = VirtualJoypadType.GetProperty("joystickHeld", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo JoystickWasJustHeldGetter = VirtualJoypadType.GetProperty("joystickWasJustHeld", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo TapToMoveGetter = typeof(GameLocation).GetProperty("tapToMove", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo TapHoldActiveGetter = TapToMoveType.GetProperty("TapHoldActive", BindingFlags.Public | BindingFlags.Instance).GetGetMethod();
		private static readonly MethodInfo IsTutorialCompleteMethod = TutorialManagerType.GetMethod("isTutorialComplete", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo TestToScrollToolbarMethod = typeof(Toolbar).GetMethod("testToScrollToolbar", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo UpdateScrollIndexMethod = typeof(Toolbar).GetMethod("updateScrollIndex", BindingFlags.NonPublic | BindingFlags.Instance);
		private static readonly MethodInfo ClearAutoSelectToolMethod = TapToMoveType.GetMethod("ClearAutoSelectTool", BindingFlags.Public | BindingFlags.Instance);
		private static readonly MethodInfo TestForHoeSelectedMethod = TutorialManagerType.GetMethod("TestForHoeSelected", BindingFlags.Public | BindingFlags.Instance);

		private static bool oldAlignTop = false;
		private static float oldTransparency = 1f;

		private static Rectangle GetToolbarBoundingBox(Toolbar __instance)
		{
			int itemSlotSize = (int)ItemSlotSizeGetter.Invoke(__instance, null);
			int maxItemSlotSize = (int)MaxItemSlotSizeField.GetValue(null);
			int maxVisibleItems = (int)MaxVisibleItemsGetter.Invoke(__instance, null);
			int screenHeight = (int)ScreenHeightGetter.Invoke(__instance, null);
			int x = Config.OffsetX;
			int y = (oldAlignTop ? maxItemSlotSize + 24 + Config.OffsetY : screenHeight - Config.OffsetY) - itemSlotSize - 24;
			int width = itemSlotSize * maxVisibleItems + 24;
			int height = itemSlotSize + 24;

			return new Rectangle(x, y, width, height);
		}

		private static bool Toolbar_testToScrollToolbar_prefix(Toolbar __instance, int x, int y)
		{
			if (!Config.EnableMod)
				return true;

			object instance = InstanceGetter.Invoke(null, null);
			bool showTheTutorials = (bool)ShowTheTutorialsField.GetValue(instance);
			bool isTutorialComplete = (bool)IsTutorialCompleteMethod.Invoke(instance, new object[] { TutorialType_USE_HOE });
			bool verticalToolbar = (bool)VerticalToolbarField.GetValue(Game1.options);

			if ((showTheTutorials && !isTutorialComplete) || verticalToolbar)
				return true;

			int xPositionOnScreen = (int)XPositionOnScreenField.GetValue(__instance);
			int _startTapPositionX = (int)StartTapPositionXField.GetValue(__instance);

			if (GetToolbarBoundingBox(__instance).Contains(x, y))
			{
				if (_startTapPositionX == -1)
				{
					StartTapPositionXField.SetValue(__instance, x - xPositionOnScreen);
				}
				else
				{
					UpdateDrawStartIndex(__instance, x);
				}
			}
			return false;
		}

		private static void UpdateDrawStartIndex(Toolbar __instance, int x)
		{
			int itemSlotSize = (int)ItemSlotSizeGetter.Invoke(__instance, null);
			int _startTapPositionX = (int)StartTapPositionXField.GetValue(__instance);
			int maxScrollIndex = (int)MaxScrollIndexGetter.Invoke(__instance, null);
			int _startIndex = (int)StartIndexField.GetValue(__instance);
			int previousIndex = (int)DrawStartIndexField.GetValue(__instance);
			int delta = _startTapPositionX - x;
			int offsetSlots = (int)Math.Round((double)delta / itemSlotSize);

			DrawStartIndexField.SetValue(__instance, Math.Max(0, Math.Min(_startIndex + offsetSlots, maxScrollIndex)));
			if ((int)DrawStartIndexField.GetValue(__instance) != previousIndex)
				ShowTooltipField.SetValue(__instance, false);
		}

		private static bool Toolbar_receiveLeftClick_prefix(Toolbar __instance, int x, int y)
		{
			if (!Config.EnableMod)
				return true;

			object virtualJoypad = VirtualJoypadField.GetValue(null);
			bool joystickHeld = (bool)JoystickHeldGetter.Invoke(virtualJoypad, null);
			bool verticalToolbar = (bool)VerticalToolbarField.GetValue(Game1.options);

			if (joystickHeld || Game1.currentLocation is MermaidHouse || verticalToolbar)
				return true;

			int itemSlotSize = (int)ItemSlotSizeGetter.Invoke(__instance, null);
			int maxVisibleItems = (int)MaxVisibleItemsGetter.Invoke(__instance, null);
			int toolbarPaddingX = (int)ToolbarPaddingXField.GetValue(null);

			if ((x > toolbarPaddingX + maxVisibleItems * itemSlotSize) && GetToolbarBoundingBox(__instance).Contains(x, y))
			{
				bool toolbarPressed = (bool)ToolbarPressedField.GetValue(null);

				TestToScrollToolbarMethod.Invoke(__instance, new object[] { x, y });
				if (!toolbarPressed)
				{
					HoverTicksAtStartField.SetValue(__instance, DateTime.Now.Ticks);
				}
				if (!Game1.player.UsingTool)
				{
					List<ClickableComponent> buttons = (List<ClickableComponent>)ButtonsField.GetValue(__instance);

					foreach (ClickableComponent button in buttons)
					{
						if (button.containsPoint(x, y))
						{
							ToolbarPressedField.SetValue(null, true);
							break;
						}
					}
				}
				return false;
			}
			return true;
		}

		public static bool Toolbar_releaseLeftClick_prefix(Toolbar __instance, int x, int y)
		{
			if (!Config.EnableMod)
				return true;

			bool _ignoreRelease = (bool)IgnoreReleaseField.GetValue(__instance);
			object tapToMove = TapToMoveGetter.Invoke(Game1.currentLocation, null);
			bool tapHoldActive = (bool)TapHoldActiveGetter.Invoke(tapToMove, null);
			bool verticalToolbar = (bool)VerticalToolbarField.GetValue(Game1.options);

			if (_ignoreRelease || Game1.currentLocation is MermaidHouse || Game1.currentLocation.currentEvent is not null || Game1.player.isEating || !Game1.displayFarmer || tapHoldActive || verticalToolbar)
				return true;

			int itemSlotSize = (int)ItemSlotSizeGetter.Invoke(__instance, null);
			int maxVisibleItems = (int)MaxVisibleItemsGetter.Invoke(__instance, null);
			int toolbarPaddingX = (int)ToolbarPaddingXField.GetValue(null);

			UpdateScrollIndexMethod.Invoke(__instance, new object[] { x, y });
			if ((x > maxVisibleItems * itemSlotSize + toolbarPaddingX) && GetToolbarBoundingBox(__instance).Contains(x, y))
			{
				List<ClickableComponent> buttons = (List<ClickableComponent>)ButtonsField.GetValue(__instance);
				int _drawStartIndex = (int)DrawStartIndexField.GetValue(__instance);

				HoverTicksAtStartField.SetValue(__instance, DateTime.Now.Ticks);
				if (Game1.player.UsingTool)
				{
					foreach (ClickableComponent button in buttons)
					{
						if (button.containsPoint(x, y))
						{
							int num = Convert.ToInt32(button.name) + _drawStartIndex;

							if (Game1.player.CurrentToolIndex == num)
							{
								NextToolIndexField.SetValue(__instance, -1);
							}
							else if (num >= _drawStartIndex && num < _drawStartIndex + maxVisibleItems)
							{
								NextToolIndexField.SetValue(__instance, num);
							}
							break;
						}
					}
				}
				else
				{
					foreach (ClickableComponent button2 in buttons)
					{
						if (button2.containsPoint(x, y))
						{
							int num2 = Convert.ToInt32(button2.name) + _drawStartIndex;

							if (Game1.player.CurrentToolIndex == num2)
							{
								if (Game1.player.netItemStowed.Value)
								{
									Game1.player.netItemStowed.Value = false;
									break;
								}
								DidInitiateItemStowField.SetValue(Program.gamePtr, true);
								Game1.playSound("stoneStep");
								Game1.player.netItemStowed.Set(newValue: true);
								Game1.player.UpdateItemStow();
							}
							else if (num2 >= _drawStartIndex && num2 < _drawStartIndex + maxVisibleItems)
							{
								object instance = InstanceGetter.Invoke(null, null);

								Game1.player.CurrentToolIndex = num2;
								ClearAutoSelectToolMethod.Invoke(tapToMove, null);
								if (Game1.player.CurrentTool != null && Game1.player.CurrentTool is MeleeWeapon weapon)
								{
									MostRecentlyChosenMeleeWeaponField.SetValue(tapToMove, weapon);
								}
								TestForHoeSelectedMethod.Invoke(instance, null);
								if (Game1.player.ActiveObject != null)
								{
									Game1.player.showCarrying();
									Game1.playSound("pickUpItem");
								}
								else
								{
									Game1.player.showNotCarrying();
									Game1.playSound("stoneStep");
								}
							}
						}
					}
				}
				return false;
			}
			return true;
		}

		private static bool Toolbar_performHoverAction_prefix(Toolbar __instance, int x, int y)
		{
			if (!Config.EnableMod)
				return true;

			object virtualJoypad = VirtualJoypadField.GetValue(null);
			bool joystickHeld = (bool)JoystickHeldGetter.Invoke(virtualJoypad, null);
			bool joystickWasJustHeld = (bool)JoystickWasJustHeldGetter.Invoke(virtualJoypad, null);
			bool verticalToolbar = (bool)VerticalToolbarField.GetValue(Game1.options);
			object tapToMove = TapToMoveGetter.Invoke(Game1.currentLocation, null);
			bool tapHoldActive = (bool)TapHoldActiveGetter.Invoke(tapToMove, null);

			if (joystickHeld || joystickWasJustHeld || !Game1.displayHUD || tapHoldActive || verticalToolbar)
				return true;

			int itemSlotSize = (int)ItemSlotSizeGetter.Invoke(__instance, null);
			int maxVisibleItems = (int)MaxVisibleItemsGetter.Invoke(__instance, null);

			if ((x > maxVisibleItems * itemSlotSize) && GetToolbarBoundingBox(__instance).Contains(x, y))
			{
				List<ClickableComponent> buttons = (List<ClickableComponent>)ButtonsField.GetValue(__instance);
				int _drawStartIndex = (int)DrawStartIndexField.GetValue(__instance);

				TestToScrollToolbarMethod.Invoke(__instance, new object[] { x, y });
				foreach (ClickableComponent button in buttons)
				{
					if (button.containsPoint(x, y))
					{
						int num = Convert.ToInt32(button.name);
						if (num + _drawStartIndex < Game1.player.Items.Count && Game1.player.Items[num + _drawStartIndex] != null)
						{
							HoverTitleField.SetValue(__instance, Game1.player.Items[num + _drawStartIndex].DisplayName);
							HoverItemField.SetValue(__instance, Game1.player.Items[num + _drawStartIndex]);

							Item hoverItem = (Item)HoverItemField.GetValue(__instance);
							Item lastHoverItem = (Item)LastHoverItemField.GetValue(__instance);
							int toolbarPaddingX = (int)ToolbarPaddingXField.GetValue(null);
							int yOffset = (int)YOffsetField.GetValue(__instance);

							if (hoverItem != lastHoverItem)
							{
								HoverTicksAtStartField.SetValue(__instance, DateTime.Now.Ticks);
							}
							LastHoverItemField.SetValue(__instance, hoverItem);
							if (verticalToolbar)
							{
								button.scale = Math.Min(button.scale + 0.05f, 1.1f);
								TooltipPositionField.SetValue(__instance, new Vector2(toolbarPaddingX + itemSlotSize + 64, yOffset + num * itemSlotSize));
							}
							else
							{
								TooltipPositionField.SetValue(__instance, new Vector2(12 + (num + 1) * itemSlotSize, 0f));
							}
						}
					}
					else if (!verticalToolbar)
					{
						button.scale = Math.Max(button.scale - 0.025f, 1f);
					}
				}
				return false;
			}
			return true;
		}

		private static void Toolbar_draw_prefix(Toolbar __instance)
		{
			oldTransparency = (float)TransparencyField.GetValue(__instance);
		}

		private static IEnumerable<CodeInstruction> Toolbar_draw_transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
		{
			try
			{
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 2; i < list.Count; i++)
				{
					if (list[i - 2].opcode.Equals(OpCodes.Ldc_R4) && list[i - 2].operand.Equals(1f) && list[i - 1].opcode.Equals(OpCodes.Stfld) && list[i - 1].operand.Equals(TransparencyField))
					{
						list.InsertRange(i, new CodeInstruction[]
						{
							new(OpCodes.Ldarg_0) { labels = list[i].labels },
							new(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(SetAlignTopAndTransparency), BindingFlags.NonPublic | BindingFlags.Static))
						});
					}
					if ((list[i - 2].opcode.Equals(OpCodes.Ldc_I4_S) && list[i - 2].operand.Equals((sbyte)24) && list[i - 1].opcode.Equals(OpCodes.Add) && list[i].opcode.Equals(OpCodes.Ldc_I4_S) && list[i].operand.Equals((sbyte)12)) || list[i - 2].opcode.Equals(OpCodes.Ldarg_0) && list[i - 1].opcode.Equals(OpCodes.Call) && list[i - 1].operand.Equals(ScreenHeightGetter) && list[i].opcode.Equals(OpCodes.Ldc_I4_S) && list[i].operand.Equals((sbyte)12))
					{
						list[i] = new(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(GetOffsetY), BindingFlags.NonPublic | BindingFlags.Static));
					}
				}
				return list;
			}
			catch (Exception e)
			{
				SMonitor.Log($"There was an issue modifying the instructions for {typeof(Toolbar)}.{original.Name}: {e}", LogLevel.Error);
				return instructions;
			}
		}

		private static void SetAlignTopAndTransparency(Toolbar __instance)
		{
			if (!Config.EnableMod)
				return;

			Rectangle playerBoundingBox = Game1.player.GetBoundingBox();
			Rectangle tb = GetToolbarBoundingBox(__instance);
			float scale = Game1.options.uiScale / Game1.options.zoomLevel;
			Rectangle toolbarBoundingBox = new((int)(tb.X * scale), (int)(tb.Y * scale), (int)(tb.Width * scale), (int)(tb.Height * scale));

			if (Game1.options.pinToolbarToggle)
			{
				AlignTopField.SetValue(__instance, Config.PinnedPosition == "top");
			}
			oldAlignTop = (bool)AlignTopField.GetValue(__instance);
			playerBoundingBox.Y -= 128 - playerBoundingBox.Height;
			playerBoundingBox.Height = 128;
			TransparencyField.SetValue(__instance, Math.Min(1f, oldTransparency + 0.075f));
			if (toolbarBoundingBox.Intersects(Game1.GlobalToLocal(Game1.viewport, playerBoundingBox)))
			{
				TransparencyField.SetValue(__instance, Math.Max(Config.OpacityPercentage, (float)TransparencyField.GetValue(__instance) - 0.15f));
			}
		}

		private static int GetOffsetY()
		{
			if (!Config.EnableMod)
				return 12;

			return Config.OffsetY;
		}

		private static void Toolbar_maxVisibleItemsGetter_postfix(ref int __result)
		{
			if (!Config.EnableMod || (bool)VerticalToolbarField.GetValue(Game1.options))
				return;

			__result = Math.Min(__result, Config.MaxVisibleItems);
		}

		private static IEnumerable<CodeInstruction> Toolbar_transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
		{
			try
			{
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode.Equals(OpCodes.Ldsfld) && list[i].operand.Equals(ToolbarPaddingXField))
					{
						list[i] = new(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(GetOffsetX), BindingFlags.NonPublic | BindingFlags.Static));
					}
				}
				return list;
			}
			catch (Exception e)
			{
				SMonitor.Log($"There was an issue modifying the instructions for {typeof(Toolbar)}.{original.Name}: {e}", LogLevel.Error);
				return instructions;
			}
		}

		private static int GetOffsetX()
		{
			if (!Config.EnableMod || (bool)VerticalToolbarField.GetValue(Game1.options))
				return (int)ToolbarPaddingXField.GetValue(null);

			return Config.OffsetX;
		}
	}
}
