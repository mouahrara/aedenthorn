using System;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using Object = StardewValley.Object;

namespace BeePaths
{
	public partial class ModEntry
	{
		public class FindCloseFlower_Patch
		{
			public static bool Prefix(GameLocation location, Vector2 startTileLocation, ref int range, Func<Crop, bool> additional_check, ref Crop __result)
			{
				if (!Config.ModEnabled || !Config.FixFlowerFind)
					return true;

				range = Config.BeeRange;
				float closestDistance = float.MaxValue;

				foreach(var kvp in location.terrainFeatures.Pairs)
				{
					if(kvp.Value is not HoeDirt || (kvp.Value as HoeDirt).crop is null || new Object((kvp.Value as HoeDirt).crop.indexOfHarvest.Value, 1, false, -1, 0).Category != -80 || (kvp.Value as HoeDirt).crop.currentPhase.Value < (kvp.Value as HoeDirt).crop.phaseDays.Count - 1 || (kvp.Value as HoeDirt).crop.dead.Value || (additional_check != null && !additional_check((kvp.Value as HoeDirt).crop)))
						continue;

					var distance = Vector2.Distance(startTileLocation, kvp.Key);
					if (distance <= range && distance < closestDistance)
					{
						closestDistance = distance;
						__result = (kvp.Value as HoeDirt).crop;
						__result.tilePosition = kvp.Key;
					}
				}
				return false;
			}
		}
	}
}
