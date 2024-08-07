﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley;

namespace FloatingGardenPots
{
	public partial class ModEntry
	{
		private static Vector2 GetPotOffset(GameLocation location, Vector2 tileLocation)
		{
			if(!offsetDict.TryGetValue(location, out var dict))
			{
				dict = new Dictionary<Vector2, Vector2>();
				offsetDict[location] = dict;
			}
			if(!dict.TryGetValue(tileLocation, out var offset))
			{
				offset = Vector2.Zero;
				if (CheckLocation(location, tileLocation.X - 1f, tileLocation.Y))
				{
					offset += new Vector2(32f, 0f);
				}
				if (CheckLocation(location, tileLocation.X + 1f, tileLocation.Y))
				{
					offset += new Vector2(-32f, 0f);
				}
				if (offset.X != 0f && CheckLocation(location, tileLocation.X + (float)Math.Sign(offset.X), tileLocation.Y + 1f))
				{
					offset += new Vector2(0f, -42f);
				}
				if (CheckLocation(location, tileLocation.X, tileLocation.Y - 1f))
				{
					offset += new Vector2(0f, 32f);
				}
				if (CheckLocation(location, tileLocation.X, tileLocation.Y + 1f))
				{
					offset += new Vector2(0f, -42f);
				}
				dict[tileLocation] = offset;
			}
			return offset;
		}

		public static bool CheckLocation(GameLocation location, float tile_x, float tile_y)
		{
			return location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Water", "Back") == null || location.doesTileHaveProperty((int)tile_x, (int)tile_y, "Passable", "Buildings") != null;
		}
	}
}
