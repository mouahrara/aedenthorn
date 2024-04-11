using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Objects;

namespace OverworldChests
{
	public partial class ModEntry : Mod
	{
		private const string modKey = "aedenthorn.OverworldChests";
		private const string modCoinKey = "aedenthorn.OverworldChests/Coin";
		private static ModConfig Config;
		private static IMonitor SMonitor;
		private static IModHelper SHelper;
		private static IAdvancedLootFrameworkApi advancedLootFrameworkApi = null;
		private static List<object> treasuresList = new();
		private static Random myRand;
		private static int daysSinceLastSpawn;
		private static readonly Color[] tintColors = new Color[]
		{
			Color.DarkGray,
			Color.Brown,
			Color.Silver,
			Color.Gold,
			Color.Purple,
		};

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			SMonitor = Monitor;
			SHelper = Helper;

			myRand = new Random();

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Chest), nameof(Chest.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(Chest_draw_Patch), nameof(Chest_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Chest), nameof(Chest.ShowMenu), Array.Empty<Type>()),
					postfix: new HarmonyMethod(typeof(Chest_showMenu_Patch), nameof(Chest_showMenu_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			RegisterConsoleCommands();

			advancedLootFrameworkApi = Helper.ModRegistry.GetApi<IAdvancedLootFrameworkApi>("aedenthorn.AdvancedLootFramework");
			if (advancedLootFrameworkApi != null)
			{
				Monitor.Log($"Loaded AdvancedLootFramework API", LogLevel.Debug);
				UpdateTreasuresList();
				Monitor.Log($"Got {treasuresList.Count} possible treasures");
			}

			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => Helper.WriteConfig(Config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.EnableMod,
				setValue: value => {
					if (Config.EnableMod != value)
					{
						if (value == false)
						{
							RemoveChests();
						}
						else
						{
							SpawnChests();
						}
					}
					Config.EnableMod = value;
				}
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.AllowIndoorSpawns.Name"),
				getValue: () => Config.AllowIndoorSpawns,
				setValue: value => Config.AllowIndoorSpawns = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RespawnInterval.Name"),
				getValue: () => Config.RespawnInterval,
				setValue: value => {
					Config.RespawnInterval = Math.Max(1, value);
				}
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RoundNumberOfChestsUp.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.RoundNumberOfChestsUp.Tooltip"),
				getValue: () => Config.RoundNumberOfChestsUp,
				setValue: value => Config.RoundNumberOfChestsUp = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ChestDensity.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ChestDensity.Tooltip"),
				getValue: () => Config.ChestDensity,
				setValue: value => Config.ChestDensity = value,
				min: 0f,
				max: 1f,
				interval: 0.001f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.RarityChance.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.RarityChance.Tooltip"),
				getValue: () => Config.RarityChance,
				setValue: value => Config.RarityChance = value,
				min: 0f,
				max: 1f,
				interval: 0.01f
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxItems.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxItems.Tooltip"),
				getValue: () => Config.MaxItems,
				setValue: value => Config.MaxItems = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ItemsBaseMaxValue.Tooltip"),
				getValue: () => Config.ItemsBaseMaxValue,
				setValue: value => Config.ItemsBaseMaxValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.Mult.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.Mult.Tooltip"),
				getValue: () => Config.Mult,
				setValue: value => Config.Mult = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MinItemValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MinItemValue.Tooltip"),
				getValue: () => Config.MinItemValue,
				setValue: value => Config.MinItemValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MaxItemValue.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.MaxItemValue.Tooltip"),
				getValue: () => Config.MaxItemValue,
				setValue: value => Config.MaxItemValue = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMin.Tooltip"),
				getValue: () => Config.CoinBaseMin,
				setValue: value => Config.CoinBaseMin = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.CoinBaseMax.Tooltip"),
				getValue: () => Config.CoinBaseMax,
				setValue: value => Config.CoinBaseMax = value
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.IncreaseRate.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.IncreaseRate.Tooltip"),
				getValue: () => Config.IncreaseRate,
				setValue: value => Config.IncreaseRate = value
			);
			configMenu.AddSectionTitle(
				mod: ModManifest,
				text: () => SHelper.Translation.Get("GMCM.ItemListChances.Text"),
				tooltip: () => SHelper.Translation.Get("GMCM.ItemListChances.Tooltip")
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesHat.Name"),
				getValue: () => Config.ItemListChances["Hat"],
				setValue: value => {
					Config.ItemListChances["Hat"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesShirt.Name"),
				getValue: () => Config.ItemListChances["Shirt"],
				setValue: value => {
					Config.ItemListChances["Shirt"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesPants.Name"),
				getValue: () => Config.ItemListChances["Pants"],
				setValue: value => {
					Config.ItemListChances["Pants"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBoots.Name"),
				getValue: () => Config.ItemListChances["Boots"],
				setValue: value => {
					Config.ItemListChances["Boots"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesMeleeWeapon.Name"),
				getValue: () => Config.ItemListChances["MeleeWeapon"],
				setValue: value => {
					Config.ItemListChances["MeleeWeapon"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesRing.Name"),
				getValue: () => Config.ItemListChances["Ring"],
				setValue: value => {
					Config.ItemListChances["Ring"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesRelic.Name"),
				getValue: () => Config.ItemListChances["Relic"],
				setValue: value => {
					Config.ItemListChances["Relic"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesMineral.Name"),
				getValue: () => Config.ItemListChances["Mineral"],
				setValue: value => {
					Config.ItemListChances["Mineral"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesCooking.Name"),
				getValue: () => Config.ItemListChances["Cooking"],
				setValue: value => {
					Config.ItemListChances["Cooking"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesFish.Name"),
				getValue: () => Config.ItemListChances["Fish"],
				setValue: value => {
					Config.ItemListChances["Fish"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesSeed.Name"),
				getValue: () => Config.ItemListChances["Seed"],
				setValue: value => {
					Config.ItemListChances["Seed"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBasicObject.Name"),
				getValue: () => Config.ItemListChances["BasicObject"],
				setValue: value => {
					Config.ItemListChances["BasicObject"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
			configMenu.AddNumberOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ItemListChancesBigCraftable.Name"),
				getValue: () => Config.ItemListChances["BigCraftable"],
				setValue: value => {
					Config.ItemListChances["BigCraftable"] = value;
					UpdateTreasuresList();
				},
				min: 0,
				max: 100
			);
		}

		private static void UpdateTreasuresList()
		{
			treasuresList = advancedLootFrameworkApi.LoadPossibleTreasures(Config.ItemListChances.Where(p => p.Value > 0).ToDictionary(s => s.Key, s => s.Value).Keys.ToArray(), Config.MinItemValue, Config.MaxItemValue);
		}

		private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
		{
			RespawnChests();
			daysSinceLastSpawn = 0;
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			if (daysSinceLastSpawn >= Config.RespawnInterval)
			{
				RespawnChests();
				daysSinceLastSpawn = 0;
			}
			daysSinceLastSpawn++;
		}

		private void RespawnChests()
		{
			Monitor.Log($"Respawning chests", LogLevel.Debug);
			RemoveChests();
			SpawnChests();
		}

		private void RemoveChests()
		{
			foreach (GameLocation location in Game1.locations)
			{
				if (location is FarmHouse || (!Config.AllowIndoorSpawns && !location.IsOutdoors) || !IsLocationAllowed(location))
					continue;

				Monitor.Log($"Respawning chests in {location.Name}");
				IList<Vector2> overlayObjectsToRemovePosition = location.overlayObjects
					.Where(o => o.Value is Chest && o.Value.modData.ContainsKey(modKey))
					.Select(o => o.Key)
					.ToList();
				List<Vector2> objectsToRemovePosition = new();
				int objectsRemovedCount = overlayObjectsToRemovePosition.Count;

				foreach (Vector2 position in overlayObjectsToRemovePosition)
				{
					location.overlayObjects.Remove(position);
				}
				foreach (var dictionary in location.objects)
				{
					foreach (var kvp in dictionary)
					{
						if (kvp.Value is Chest && kvp.Value.modData.ContainsKey(modKey))
						{
							objectsToRemovePosition.Add(kvp.Value.TileLocation);
						}
					}
				}
				objectsRemovedCount += objectsToRemovePosition.Count;
				foreach (Vector2 tilelocation in objectsToRemovePosition)
				{
					location.objects.Remove(tilelocation);
				}
				Monitor.Log($"Removed {objectsRemovedCount} chests");
			}
		}

		private void SpawnChests()
		{
			foreach (GameLocation location in Game1.locations)
			{
				if (location is FarmHouse || (!Config.AllowIndoorSpawns && !location.IsOutdoors) || !IsLocationAllowed(location))
					continue;

				int width = location.map.Layers[0].LayerWidth;
				int height = location.map.Layers[0].LayerHeight;

				bool IsTileFree(Vector2 position)
				{
					if (location.isWaterTile((int)position.X, (int)position.Y))
						return false;
					if (!location.CanItemBePlacedHere(position))
						return false;
					if (location.isCropAtTile((int)position.X, (int)position.Y))
						return false;
					return true;
				}

				bool IsTileIndexFree(int i)
				{
					return IsTileFree(new Vector2(i % width, i / width));
				}

				int freeTiles = Enumerable.Range(0, width * height).Count(IsTileIndexFree);
				Monitor.Log($"Got {freeTiles} free tiles");
				int maxChests = Math.Min(freeTiles, (int)Math.Floor(freeTiles * Config.ChestDensity) + (Config.RoundNumberOfChestsUp ? 1 : 0));
				Monitor.Log($"Max chests: {maxChests}");

				while (maxChests > 0)
				{
					Vector2 freeTile = location.getRandomTile();

					if (!IsTileFree(freeTile))
						continue;

					double fraction = Math.Pow(myRand.NextDouble(), 1 / Config.RarityChance);
					int level = (int)Math.Ceiling(fraction * Config.Mult);
					Chest chest = advancedLootFrameworkApi.MakeChest(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, level, Config.IncreaseRate, Config.ItemsBaseMaxValue, freeTile);

					chest.playerChoiceColor.Value = MakeTint(fraction);
					chest.modData.Add(modKey, "T");
					chest.modData.Add(modCoinKey, advancedLootFrameworkApi.GetChestCoins(level, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax).ToString());
					location.overlayObjects[freeTile] = chest;
					maxChests--;
				}
			}
		}

		private static bool IsLocationAllowed(GameLocation location)
		{
			if(Config.OnlyAllowLocations.Length > 0)
				return Config.OnlyAllowLocations.Split(',').Contains(location.Name);
			return !Config.DisallowLocations.Split(',').Contains(location.Name);
		}

		private static Color MakeTint(double fraction)
		{
			return tintColors[(int)Math.Floor(fraction * tintColors.Length)];
		}
	}
}
