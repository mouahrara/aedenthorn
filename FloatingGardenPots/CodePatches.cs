using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using xTile.Dimensions;
using StardewValley;
using StardewValley.Objects;
using StardewValley.ItemTypeDefinitions;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Object = StardewValley.Object;

namespace FloatingGardenPots
{
	public partial class ModEntry
	{
		public class Object_canBePlacedHere_Patch
		{
			public static void Postfix(Object __instance, GameLocation l, Vector2 tile, ref bool __result)
			{
				if (!Config.ModEnabled)
					return;

				if (__instance.QualifiedItemId == "(BC)62")
				{
					if (CrabPot.IsValidCrabPotLocationTile(l, (int)tile.X, (int)tile.Y))
					{
						__result = true;
					}
				}
			}
		}

		public class GameLocation_checkAction_Patch
		{
			public static bool Prefix(GameLocation __instance, Location tileLocation, Farmer who, ref bool __result)
			{
				if (!Config.ModEnabled)
					return true;
				if (who.ActiveObject?.bigCraftable.Value != true || who.ActiveObject.ParentSheetIndex != 62)
					return true;
				if(!CrabPot.IsValidCrabPotLocationTile(__instance, tileLocation.X, tileLocation.Y))
					return true;

				Vector2 p = new(tileLocation.X, tileLocation.Y);
				var pot = new IndoorPot(p);

				__result = true;
				pot.hoeDirt.Value.state.Value = 1;
				pot.showNextIndex.Value = true;
				pot.modData[modKey] = "true";
				__instance.objects.Add(p, pot);
				who.reduceActiveItemByOne();
				__instance.playSound("dropItemInWater");
				SMonitor.Log($"Placed garden pot at {p}");
				return false;
			}
		}

		public class IndoorPot_DayUpdate_Patch
		{
			public static void Postfix(IndoorPot __instance)
			{
				if (!Config.ModEnabled || !__instance.Location.isWaterTile((int)__instance.TileLocation.X, (int)__instance.TileLocation.Y))
					return;

				__instance.hoeDirt.Value.state.Value = 1;
				__instance.showNextIndex.Value = true;
			}
		}

		public class IndoorPot_draw_Patch
		{
			public static bool Prefix(IndoorPot __instance, SpriteBatch spriteBatch, int x, int y, float alpha = 1f)
			{
				if (!Config.ModEnabled || !__instance.modData.ContainsKey(modKey))
					return true;

				var yBob = (float)(Math.Sin(Game1.currentGameTime.TotalGameTime.TotalMilliseconds / 500.0 + x * 64) * 8.0 + 8.0);
				Vector2 offset = GetPotOffset(Game1.currentLocation, new Vector2(x, y));

				offset.Y += yBob;
				if (yBob <= 0.001f)
				{
					Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(0, 0, 64, 64), 150f, 8, 0, offset + new Vector2(x * 64 + 4, y * 64 + 32), false, Game1.random.NextDouble() < 0.5, 0.001f, 0.01f, Color.White, 0.75f, 0.003f, 0f, 0f, false));
				}

				Vector2 scaleFactor = __instance.getScale() * 4f;
				Vector2 position = Game1.GlobalToLocal(Game1.viewport, new Vector2(x * 64, y * 64 - 64)) + offset;
				Rectangle destinationRectangle = new((int)(position.X - scaleFactor.X / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(position.Y - scaleFactor.Y / 2f) + ((__instance.shakeTimer > 0) ? Game1.random.Next(-1, 2) : 0), (int)(64f + scaleFactor.X), (int)(128f + scaleFactor.Y / 2f));

				ParsedItemData dataOrErrorItem = ItemRegistry.GetDataOrErrorItem(__instance.QualifiedItemId);
				spriteBatch.Draw(dataOrErrorItem.GetTexture(), destinationRectangle, dataOrErrorItem.GetSourceRect(__instance.showNextIndex.Value ? 1 : 0), Color.White * alpha, 0f, Vector2.Zero, SpriteEffects.None, Math.Max(0f, ((y + 1) * 64 - 24) / 10000f) + x * 1E-05f);
				if (__instance.hoeDirt.Value.HasFertilizer())
				{
					Rectangle fertilizerSourceRect = __instance.hoeDirt.Value.GetFertilizerSourceRect();
					fertilizerSourceRect.Width = 13;
					fertilizerSourceRect.Height = 13;
					spriteBatch.Draw(Game1.mouseCursors, Game1.GlobalToLocal(Game1.viewport, new Vector2(__instance.TileLocation.X * 64f + 4f, __instance.TileLocation.Y * 64f - 12f)), fertilizerSourceRect, Color.White, 0f, Vector2.Zero, 4f, SpriteEffects.None, (__instance.TileLocation.Y + 0.65f) * 64f / 10000f + x * 1E-05f);
				}
				__instance.hoeDirt.Value.crop?.drawWithOffset(spriteBatch, __instance.TileLocation + offset / 64f, (__instance.hoeDirt.Value.isWatered() && __instance.hoeDirt.Value.crop.currentPhase.Value == 0 && !__instance.hoeDirt.Value.crop.raisedSeeds.Value) ? (new Color(180, 100, 200) * 1f) : Color.White, __instance.hoeDirt.Value.getShakeRotation(), new Vector2(32f, 8f));
				__instance.heldObject.Value?.draw(spriteBatch, x * 64 + (int)offset.X, y * 64 - 48 + (int)offset.Y, (__instance.TileLocation.Y + 0.66f) * 64f / 10000f + x * 1E-05f, 1f);
				if (__instance.bush.Value is not null)
				{
					__instance.bush.Value.Tile += offset / 64f;
					__instance.bush.Value.draw(spriteBatch, -24f);
					__instance.bush.Value.Tile -= offset / 64f;
				}
				return false;
			}
		}
	}
}
