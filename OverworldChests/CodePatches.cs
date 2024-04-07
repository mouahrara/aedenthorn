using StardewValley;
using StardewValley.Objects;

namespace OverworldChests
{
	public partial class ModEntry
	{
		public class Chest_draw_Patch
		{
			public static bool Prefix(Chest __instance)
			{
				if (!Config.EnableMod)
					return true;
				if (!__instance.modData.ContainsKey(modKey))
					return true;
				if (!__instance.Location.objects.ContainsKey(__instance.TileLocation) || (__instance.Items.Count > 0 && __instance.Items[0] != null))
					return true;

				SMonitor.Log($"removing chest at {__instance.TileLocation}");
				__instance.Location.objects.Remove(__instance.TileLocation);
				return false;
			}
		}

		public class Chest_showMenu_Patch
		{
			public static void Postfix(Chest __instance)
			{
				if (!Config.EnableMod)
					return;
				if (!__instance.modData.ContainsKey(modKey))
					return;
				if (!__instance.modData.ContainsKey(modCoinKey))
					return;
				Game1.player.Money += int.Parse(__instance.modData[modCoinKey]);
				__instance.modData.Remove(modCoinKey);
				return;
			}
		}
	}
}
