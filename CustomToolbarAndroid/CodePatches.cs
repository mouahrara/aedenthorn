using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CustomToolbarAndroid
{
	public partial class ModEntry
	{
		private static float oldTransparency = 1f;

		private static void Toolbar_draw_prefix(Toolbar __instance)
		{
			oldTransparency = (float)typeof(Toolbar).GetField("transparency", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
		}

		private static IEnumerable<CodeInstruction> Toolbar_draw_transpiler(IEnumerable<CodeInstruction> instructions, MethodBase original)
		{
			try
			{
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 2; i < list.Count; i++)
				{
					if (list[i - 2].opcode.Equals(OpCodes.Ldc_R4) && list[i - 2].operand.Equals(1f) && list[i - 1].opcode.Equals(OpCodes.Stfld) && list[i - 1].operand.Equals(typeof(Toolbar).GetField("transparency", BindingFlags.NonPublic | BindingFlags.Instance)))
					{
						list.InsertRange(i, new CodeInstruction[]
						{
							new(OpCodes.Ldarg_0) { labels = list[i].labels },
							new(OpCodes.Ldarg_0),
							new(OpCodes.Ldflda, typeof(Toolbar).GetField("alignTop", BindingFlags.Public | BindingFlags.Instance)),
							new(OpCodes.Ldarg_0),
							new(OpCodes.Ldflda, typeof(Toolbar).GetField("transparency", BindingFlags.NonPublic | BindingFlags.Instance)),
							new(OpCodes.Ldarg_0),
							new(OpCodes.Call, typeof(Toolbar).GetProperty("screenHeight", BindingFlags.Public | BindingFlags.Instance).GetGetMethod()),
							new(OpCodes.Call, typeof(ModEntry).GetMethod(nameof(SetAlignTopAndTransparency), BindingFlags.NonPublic | BindingFlags.Static))
						});
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

		private static void SetAlignTopAndTransparency(Toolbar __instance, ref bool alignTop, ref float transparency, int screenHeight)
		{
			if (!Config.EnableMod)
				return;

			if (Game1.options.pinToolbarToggle)
			{
				alignTop = Config.PinnedPosition == "top";

				Rectangle playerBoundingBox = Game1.player.GetBoundingBox();
				int maxItemSlotSize = (int)typeof(Game1).GetField("maxItemSlotSize", BindingFlags.Public | BindingFlags.Static).GetValue(null);
				int yPositionOnScreen = alignTop ? maxItemSlotSize + 24 + 12 : screenHeight - 12;
				int toolbarHeight = (int)typeof(Toolbar).GetField("toolbarHeight", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

				transparency = Math.Min(1f, oldTransparency + 0.075f);
				if (alignTop ? Game1.GlobalToLocal(globalPosition: new Vector2(playerBoundingBox.Center.X, playerBoundingBox.Bottom - 128), viewport: Game1.viewport).Y <= Utility.ModifyCoordinateFromUIScale(yPositionOnScreen) : Game1.GlobalToLocal(globalPosition: new Vector2(playerBoundingBox.Center.X, playerBoundingBox.Bottom), viewport: Game1.viewport).Y >= Utility.ModifyCoordinateFromUIScale(yPositionOnScreen - toolbarHeight - 12))
				{
					transparency = Math.Max(Config.OpacityPercentage, transparency - 0.15f);
				}
			}
		}

		private static void Toolbar_maxVisibleItemsGetter_postfix(ref int __result)
		{
			if (!Config.EnableMod)
				return;

			if (!(bool)typeof(Options).GetField("verticalToolbar").GetValue(Game1.options))
			{
				__result = Math.Min(__result, Config.MaxVisibleItems);
			}
		}
	}
}
