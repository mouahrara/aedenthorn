using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace HereFishy
{
	public partial class ModEntry
	{
		private static readonly PerScreen<Farmer> lastUser = new(() => null);
		private static readonly PerScreen<bool> canPerfect = new(() => false);
		private static readonly PerScreen<bool> wasPerfect = new(() => false);
		private static readonly PerScreen<SparklingText> sparklingText = new(() => null);
		private static readonly PerScreen<bool> hereFishying = new(() => false);

		internal static Farmer LastUser
		{
			get => lastUser.Value;
			set => lastUser.Value = value;
		}

		internal static bool CanPerfect
		{
			get => canPerfect.Value;
			set => canPerfect.Value = value;
		}

		internal static bool WasPerfect
		{
			get => wasPerfect.Value;
			set => wasPerfect.Value = value;
		}

		internal static SparklingText SparklingText
		{
			get => sparklingText.Value;
			set => sparklingText.Value = value;
		}

		internal static bool HereFishying
		{
			get => hereFishying.Value;
			set => hereFishying.Value = value;
		}

		private static async void HereFishy(Farmer who, Vector2 bobberTile)
		{
			Item fish = GetFish(who, bobberTile, out int fishSize, out int fishQuality, out int fishDifficulty, out bool isBossFish, out bool caughtDoubleFish);

			await FarmerAnimation(who, bobberTile);
			PullFishFromWater(who, bobberTile * Game1.tileSize, fish, fishSize, fishQuality, fishDifficulty, WasPerfect, isBossFish, caughtDoubleFish ? 2 : 1);
		}

		private static Item GetFish(Farmer who, Vector2 bobberTile, out int fishSize, out int fishQuality, out int fishDifficulty, out bool isBossFish, out bool caughtDoubleFish)
		{
			Item fish = who.currentLocation.getFish(0, null, FishingRod.distanceToLand((int)bobberTile.X, (int)bobberTile.Y, who.currentLocation), who, 0d, bobberTile);
			Dictionary<string, string> dictionary = DataLoader.Fish(Game1.content);

			if (fish is null || ItemRegistry.GetDataOrErrorItem(fish.QualifiedItemId).IsErrorItem)
			{
				fish = ItemRegistry.Create("(O)" + Game1.random.Next(167, 173));
			}

			bool nonFish = fish is Furniture || fish.HasContextTag("fish_nonfish") || !(Utility.IsNormalObjectAtParentSheetIndex(fish, fish.ItemId) && dictionary.ContainsKey(fish.ItemId) && int.TryParse(dictionary[fish.ItemId].Split('/')[1], out _));

			fishSize = 0;
			fishQuality = 0;
			fishDifficulty = 0;
			if (dictionary.TryGetValue(fish.ItemId, out string value) && !nonFish)
			{
				string[] array = value.Split('/');

				fishDifficulty = Convert.ToInt32(array[1]);
				int minFishSize = Convert.ToInt32(array[3]);
				int maxFishSize = Convert.ToInt32(array[4]);
				float fFishSize = getFFishSize(who, fish);

				static float getFFishSize(Farmer who, Item fish)
				{
					int minimumSizeContribution = 1 + who.FishingLevel / 2;
					float fFishSize = 1f;

					fFishSize *= Game1.random.Next(minimumSizeContribution, Math.Max(6, minimumSizeContribution)) / 5f;
					if (fish is Object @object && @object.scale.X == 1f)
					{
						fFishSize *= 1.2f;
					}
					fFishSize *= 1f + Game1.random.Next(-10, 11) / 100f;
					fFishSize = Math.Max(0f, Math.Min(1f, fFishSize));
					return fFishSize;
				}

				fishSize = (int)(minFishSize + (maxFishSize - minFishSize) * fFishSize);
				fishSize++;
				fishQuality = (fFishSize < 0.33) ? 0 : ((fFishSize < 0.66) ? 1 : 2);
				if (Game1.player.CurrentTool is FishingRod && Game1.player.CurrentTool.UpgradeLevel == 1)
				{
					fishQuality = 0;
					fishSize = minFishSize;
				}
			}
			fish.TryGetTempData("IsBossFish", out isBossFish);
			caughtDoubleFish = !isBossFish && Game1.random.NextDouble() < 0.1 + Game1.player.DailyLuck / 2.0;
			return fish;
		}

		private static async Task FarmerAnimation(Farmer who, Vector2 bobberTile)
		{
			List<FarmerSprite.AnimationFrame> animationFrames = new()
			{
				new FarmerSprite.AnimationFrame(94, 100, false, who.FacingDirection == 3, null, false).AddFrameAction(f => f.jitterStrength = 2f)
			};
			float stamina = who.Stamina;

			LastUser = who;
			HereFishying = true;
			if (Config.PlaySound && fishyMalePath is not null && fishyFemalePath is not null)
			{
				if ((Config.VoiceGender == "Auto" && who.Gender != Gender.Female) || Config.VoiceGender == "Male")
				{
					who.currentLocation.playSound($"{SModManifest.UniqueID}_fishy_male", who.Tile + new Vector2(0, -1));
				}
				else
				{
					who.currentLocation.playSound($"{SModManifest.UniqueID}_fishy_female", who.Tile + new Vector2(0, -1));
				}
			}
			who.completelyStopAnimatingOrDoingAction();
			who.CanMove = Config.AllowMovement;
			who.forceTimePass = true;
			who.jitterStrength = 2f;
			who.FarmerSprite.setCurrentAnimation(animationFrames.ToArray());
			who.FarmerSprite.PauseForSingleAnimation = true;
			who.FarmerSprite.loop = true;
			who.FarmerSprite.loopThisAnimation = true;
			who.Sprite.currentFrame = 94;
			await Task.Delay(1793);
			CanPerfect = true;
			WasPerfect = false;
			who.synchronizedJump(8f);
			who.Stamina = Math.Max(0, who.Stamina - Config.StaminaCost);
			who.checkForExhaustion(stamina);
			await Task.Delay(100);
			CanPerfect = false;
			await Task.Delay(900);
			who.stopJittering();
			who.completelyStopAnimatingOrDoingAction();
			who.forceCanMove();
			who.CanMove = Config.AllowMovement;
			who.forceTimePass = true;
			HereFishying = false;
			await Task.Delay(Game1.random.Next(500, 1000));
			if (Config.PlaySound && weePath is not null)
			{
				who.currentLocation.playSound($"{SModManifest.UniqueID}_wee", bobberTile);
			}
		}

		public static void PullFishFromWater(Farmer who, Vector2 bobberPosition, Item fish, int fishSize, int fishQuality, int fishDifficulty, bool wasPerfect, bool isBossFish, int numberOfFishCaught)
		{
			ItemMetadata fishMetadata = ItemRegistry.GetMetadata(fish.ItemId);
			bool isObjectTypeFish = fishMetadata.TypeIdentifier == "(O)";

			if (fishQuality >= 2 && wasPerfect)
			{
				fishQuality = 4;
			}
			else if (fishQuality >= 1 && wasPerfect)
			{
				fishQuality = 2;
			}
			if (!Game1.isFestival() && who.IsLocalPlayer && isObjectTypeFish)
			{
				int experience = Math.Max(1, (fishQuality + 1) * 3 + fishDifficulty / 3);

				if (wasPerfect)
				{
					experience += (int)(experience * 1.4f);
				}
				if (isBossFish)
				{
					experience *= 5;
				}
				who.gainExperience(1, experience);
			}
			if (fishQuality < 0)
			{
				fishQuality = 0;
			}

			string textureName;
			Rectangle sourceRect;
			TemporaryAnimatedSpriteList temporarySprites = new();

			if (isObjectTypeFish)
			{
				ParsedItemData parsedOrErrorData = fishMetadata.GetParsedOrErrorData();

				textureName = parsedOrErrorData.TextureName;
				sourceRect = parsedOrErrorData.GetSourceRect();
			}
			else
			{
				textureName = "LooseSprites\\Cursors";
				sourceRect = new Rectangle(228, 408, 16, 16);
			}
			for (int i = 0; i < numberOfFishCaught; i++)
			{
				const float gravity = 0.002f;
				float distance = bobberPosition.Y - (who.StandingPixel.Y - Game1.tileSize);
				float height = Math.Abs(distance + 256f + 32f);
				float velocity = (float)Math.Sqrt(2f * gravity * height);
				float time = (float)(Math.Sqrt(2f * (height - distance) / gravity) + (velocity / gravity));
				float xVelocity = time != 0f ? (who.Position.X - bobberPosition.X) / time : 0f;

				temporarySprites.Add(new TemporaryAnimatedSprite(textureName, sourceRect, time, 1, 0, bobberPosition, false, false, bobberPosition.Y / 10000f, 0f, Color.White, 4f, 0f, 0f, 0f, false)
				{
					motion = new Vector2(xVelocity, -velocity),
					acceleration = new Vector2(0f, gravity),
					timeBasedMotion = true,
					endFunction = i < numberOfFishCaught - 1 ? delegate
					{
						AddOneFishToInventory(who, fish, fishQuality);
					} : delegate
					{
						AddOneFishToInventory(who, fish, fishQuality);
						PlayerCaughtFishEndFunction(who, fish, fishSize, isBossFish, numberOfFishCaught);
					},
					endSound = i == 0 ? "tinyWhip" : "fishSlap",
					Parent = who.currentLocation,
					delayBeforeAnimationStart = i * 100
				});
			}
			Game1.Multiplayer.broadcastSprites(who.currentLocation, temporarySprites);
		}

		public static void AddOneFishToInventory(Farmer who, Item fish, int fishQuality)
		{
			Object fishObject = new(fish.ItemId, 1, false, -1, fishQuality);

			if (!who.addItemToInventoryBool(fishObject))
			{
				Game1.createItemDebris(fishObject, who.getStandingPosition(), 0, who.currentLocation);
			}
		}

		public static void PlayerCaughtFishEndFunction(Farmer who, Item fish, int fishSize, bool isBossFish, int numberOfFishCaught)
		{
			bool recordSize = false;
			bool isUncaughtFish = fish.QualifiedItemId.StartsWith("(O)") && !who.fishCaught.ContainsKey(fish.QualifiedItemId) && !fish.QualifiedItemId.Equals("(O)388") && !fish.QualifiedItemId.Equals("(O)390");

			who.Halt();
			who.armOffset = Vector2.Zero;
			who.canReleaseTool = false;
			if (!Game1.isFestival())
			{
				recordSize = who.caughtFish(fish.QualifiedItemId, fishSize, false, numberOfFishCaught);
			}
			else
			{
				Game1.currentLocation.currentEvent.caughtFish(fish.QualifiedItemId, fishSize, who);
			}
			if (isBossFish)
			{
				Game1.showGlobalMessage(Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14068"));     
				Game1.Multiplayer.globalChatInfoMessage("CaughtLegendaryFish", who.Name, TokenStringBuilder.ItemName(fish.QualifiedItemId));
			}
			else if (recordSize)
			{
				SparklingText = new(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:FishingRod.cs.14069"), Color.LimeGreen, Color.Azure);
				if (!isUncaughtFish)
				{
					who.playNearbySoundLocal("newRecord");
				}
			}
			else
			{
				who.playNearbySoundLocal("fishSlap");
			}
			if (isUncaughtFish && who.fishCaught.ContainsKey(fish.QualifiedItemId))
			{
				SparklingText = new(Game1.dialogueFont, Game1.content.LoadString("Strings\\1_6_Strings:FirstCatch"), new Color(200, 255, 220), Color.White);
				who.playNearbySoundLocal("discoverMineral");
			}
			DoneFishing(who);
		}

		public static void DoneFishing(Farmer who)
		{
			who.UsingTool = false;
			who.CanMove = true;
			who.completelyStopAnimatingOrDoingAction();
		}
	}
}
