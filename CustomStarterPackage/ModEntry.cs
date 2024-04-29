using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Objects;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace CustomStarterPackage
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;

		public const string dictPath = "aedenthorn.CustomStarterPackage/dictionary";
		public const string chestKey = "aedenthorn.CustomStarterPackage/chest";
		internal static Dictionary<string, StarterItemData> dataDict = new();

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Chest), nameof(Chest.dumpContents)),
					postfix: new HarmonyMethod(typeof(Chest_dumpContents_Patch), nameof(Chest_dumpContents_Patch.Postfix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			dataDict = Game1.content.Load<Dictionary<string, StarterItemData>>(dictPath);
			SMonitor.Log($"Loaded {dataDict.Count} items from content patcher");
			foreach (var o in Game1.player.currentLocation.objects.Pairs)
			{
				if (o.Value is Chest && (o.Value as Chest).giftbox.Value && (o.Value as Chest).Items.Count == 1 && (o.Value as Chest).Items[0].ParentSheetIndex == 472 && (o.Value as Chest).Items[0].Stack == 15)
				{
					SMonitor.Log($"Found starter chest at {o.Key}; replacing");
					Inventory items = new();

					foreach (var d in dataDict)
					{
						if (d.Value.ChancePercent < Game1.random.Next(100))
							continue;
						SMonitor.Log($"Adding {d.Key}");

						Item obj;
						string itemId;

						switch (d.Value.Type)
						{
							case "Object":
								int amount = (d.Value.MinAmount < d.Value.MaxAmount) ? Game1.random.Next(d.Value.MinAmount, d.Value.MaxAmount + 1) : d.Value.MinAmount;
								int quality = (d.Value.MinQuality < d.Value.MaxQuality) ? Game1.random.Next(d.Value.MinQuality, d.Value.MaxQuality + 1) : d.Value.MinQuality;

								if ((itemId = GetItemIdForNewDataModel(d.Value.Type, Game1.objectData, d.Value.NameOrIndex)) is null)
									continue;
								obj = new Object(itemId, amount, quality: quality);
								break;
							case "BigCraftable":
							case "Chest":
								if ((itemId = GetItemIdForNewDataModel(d.Value.Type, Game1.bigCraftableData, d.Value.NameOrIndex)) is null)
									continue;
								obj = new Object(Vector2.Zero, itemId, false);
								break;
							case "Hat":
								if ((itemId = GetItemIdForOldDataModel(d.Value.Type, SHelper.GameContent.Load<Dictionary<string, string>>("Data/hats"), d.Value.NameOrIndex)) is null)
									continue;
								obj = new Hat(itemId);
								break;
							case "Boots":
								if ((itemId = GetItemIdForOldDataModel(d.Value.Type, SHelper.GameContent.Load<Dictionary<string, string>>("Data/Boots"), d.Value.NameOrIndex)) is null)
									continue;
								obj = new Boots(itemId);
								break;
							case "Ring":
								if ((itemId = GetItemIdForNewDataModel(d.Value.Type, Game1.objectData, d.Value.NameOrIndex)) is null)
									continue;
								obj = new Ring(itemId);
								break;
							case "Clothing":
								if ((itemId = GetItemIdForNewDataModel(d.Value.Type, Game1.shirtData, d.Value.NameOrIndex, false)) is null && (itemId = GetItemIdForNewDataModel(d.Value.Type, Game1.pantsData, d.Value.NameOrIndex)) is null)
									continue;
								obj = new Clothing(itemId);
								break;
							case "Furniture":
								if ((itemId = GetItemIdForOldDataModel(d.Value.Type, SHelper.GameContent.Load<Dictionary<string, string>>("Data/Furniture"), d.Value.NameOrIndex)) is null)
									continue;
								obj = Furniture.GetFurnitureInstance(itemId, Vector2.Zero);
								break;
							case "Tool":
							case "Axe":
							case "FishingRod":
							case "Hoe":
							case "Pan":
							case "Pickaxe":
							case "Shears":
							case "WateringCan":
								if ((itemId = GetItemIdForNewDataModel(d.Value.Type, Game1.toolData, d.Value.NameOrIndex)) is null)
									continue;
								obj = ItemRegistry.Create("(T)" + itemId, 1, 0, true);
								if (obj is WateringCan)
								{
									(obj as WateringCan).WaterLeft = 0;
								}
								break;
							case "MeleeWeapon":
								if ((itemId = GetItemIdForNewDataModel(d.Value.Type, Game1.weaponData, d.Value.NameOrIndex)) is null)
									continue;
								obj = new MeleeWeapon(itemId);
								break;
							default:
								SMonitor.Log($"Object type {d.Value.Type} not recognized", LogLevel.Warn);
								continue;
						}
						if (obj != null)
						{
							items.Add(obj);
						}
						else
						{
							SMonitor.Log($"Object {d.Key} not recognized", LogLevel.Warn);
						}
					}
					if (items.Count > 0)
					{
						(o.Value as Chest).Items.Clear();
						(o.Value as Chest).Items.AddRange(items);
						(o.Value as Chest).dropContents.Value = true;
						o.Value.modData[chestKey] = "true";
						SMonitor.Log($"Added {items.Count} items to starter package", LogLevel.Info);
						return;
					}
					SMonitor.Log($"No items added to the starter package (The default starter package will be used)", LogLevel.Info);
					return;
				}
			}
		}

		private static string GetItemIdForOldDataModel(string type, Dictionary<string, string> data, string nameOrId, bool log = true)
		{
			if (!data.TryGetValue(nameOrId, out _))
			{
				try
				{
					return data.First(o => o.Value.StartsWith(nameOrId + '/')).Key;
				}
				catch
				{
					if (log)
					{
						SMonitor.Log($"{type} {nameOrId} not found", LogLevel.Warn);
					}
					return null;
				}
			}
			return nameOrId;
		}

		private static string GetItemIdForNewDataModel<T>(string type, IDictionary<string, T> data, string nameOrId, bool log = true)
		{
			if (!data.TryGetValue(nameOrId, out _))
			{
				try
				{
					return data.First(o => typeof(T).GetField("Name").GetValue(o.Value).Equals(nameOrId)).Key;
				}
				catch
				{
					if (log)
					{
						SMonitor.Log($"{type} {nameOrId} not found", LogLevel.Warn);
					}
					return null;
				}
			}
			return nameOrId;
		}

		private void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo(dictPath))
			{
				e.LoadFrom(() => new Dictionary<string, StarterItemData>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
			}
		}

		private void GameLoop_SaveLoaded(object sender, StardewModdingAPI.Events.SaveLoadedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			dataDict = Game1.content.Load<Dictionary<string, StarterItemData>>(dictPath);
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
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
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
		}
	}
}
