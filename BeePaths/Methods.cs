using Microsoft.Xna.Framework;
using StardewValley;

namespace BeePaths
{
	public partial class ModEntry
	{
		private static BeeData GetBee(Vector2 startTile, Vector2 endTile, bool random = true)
		{
			var start = startTile * 64 + new Vector2(Game1.random.Next(64), Game1.random.Next(64) - 32);
			var end = endTile * 64 + new Vector2(Game1.random.Next(64), Game1.random.Next(64) - 32);
			var pos = random ? Vector2.Lerp(start, end, (float)Game1.random.NextDouble()) : start;

			return new BeeData()
			{
				startPos = start,
				endPos = end,
				pos = pos,
				startTile = startTile,
				endTile = endTile
			};
		}
	}
}
