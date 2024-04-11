using StardewValley;
using StardewValley.TerrainFeatures;

namespace CropsSurviveSeasonChange
{
	public partial class ModEntry
	{
		private static bool ShouldDestroyCrop(bool outdoors, HoeDirt hoeDirt, GameLocation environment)
		{
			return ShouldKillCrop(outdoors, hoeDirt.crop, environment);
		}

		private static bool ShouldKillCrop(bool outdoors, Crop crop, GameLocation environment)
		{
			if (!Config.ModEnabled || crop.forageCrop.Value || crop.dead.Value || (!Config.IncludeRegrowables && crop.GetData().RegrowDays != -1) || (environment.GetSeason() == Season.Winter && !Config.IncludeWinter))
			{
				return outdoors;
			}
			return false;
		}
	}
}
