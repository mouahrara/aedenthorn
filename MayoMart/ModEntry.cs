using System;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace MayoMart
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;
			SMonitor = Monitor;
			SHelper = helper;
			SModManifest = ModManifest;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Dialogue), "parseDialogueString"),
					postfix: new HarmonyMethod(typeof(Dialogue_parseDialogueString_Patch), nameof(Dialogue_parseDialogueString_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void Content_AssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (Config.ReplaceTexts)
			{
				if (e.NameWithoutLocale.StartsWith("Characters/Dialogue/") || e.NameWithoutLocale.StartsWith("Data/Events/") || (e.NameWithoutLocale.StartsWith("Strings/") && !e.NameWithoutLocale.IsEquivalentTo("Strings/credits")) || e.NameWithoutLocale.IsEquivalentTo("Data/ExtraDialogue") || e.NameWithoutLocale.IsEquivalentTo("Data/Festivals/winter25") || e.NameWithoutLocale.IsEquivalentTo("Data/mail"))
				{
					e.Edit(ReplaceJojaWithMayo);
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Data/hats"))
				{
					e.Edit((IAssetData data) => ReplaceJojaWithMayoStringDataFormat(data, new int[] { 1, 5 }));
				}
			}
			if (Config.ReplaceTextures)
			{
				if (e.NameWithoutLocale.IsEquivalentTo("Maps/spring_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/spring_town.fr-FR") => "assets/Maps/spring_town.fr-FR.png",
						_ => "assets/Maps/spring_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/summer_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/summer_town.fr-FR") => "assets/Maps/summer_town.fr-FR.png",
						_ => "assets/Maps/summer_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/fall_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/fall_town.fr-FR") => "assets/Maps/fall_town.fr-FR.png",
						_ => "assets/Maps/fall_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/winter_town"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/winter_town.fr-FR") => "assets/Maps/winter_town.fr-FR.png",
						_ => "assets/Maps/winter_town.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(0, 836, 512, 156), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/townInterior"))
				{
					Texture2D texture = Helper.ModContent.Load<Texture2D>(e.Name switch
					{
						IAssetName name when name.IsEquivalentTo("Maps/townInterior.fr-FR") => "assets/Maps/townInterior.fr-FR.png",
						_ => "assets/Maps/townInterior.png"
					});

					e.Edit((IAssetData data) => data.AsImage().PatchImage(texture, null, new Rectangle(96, 928, 320, 128), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Maps/walls_and_floors"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Maps/walls_and_floors.png"), null, new Rectangle(80, 48, 16, 48), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/Craftables"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/Craftables.png"), null, new Rectangle(80, 448, 16, 32), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/furniture.png"), null, new Rectangle(144, 803, 48, 23), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furniture"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/joja_furniture.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furnitureFront"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/TileSheets/joja_furnitureFront.png"), null, null, PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/hats"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Characters/Farmer/hats.png"), null, new Rectangle(0, 640, 20, 80), PatchMode.Replace));
				}
				else if (e.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts"))
				{
					e.Edit((IAssetData data) => data.AsImage().PatchImage(Helper.ModContent.Load<Texture2D>("assets/Characters/Farmer/shirts.png"), null, new Rectangle(72, 192, 8, 32), PatchMode.Replace));
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
				mod: ModManifest,
				reset: () => Config = new ModConfig(),
				save: () => {
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.StartsWith("Characters/Dialogue/"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.StartsWith("Data/Events/"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.StartsWith("Strings/") && !asset.NameWithoutLocale.IsEquivalentTo("Strings/credits"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/ExtraDialogue"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/Festivals/winter25"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/mail"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Data/hats"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/spring_town"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/summer_town"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/fall_town"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/winter_town"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/townInterior"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Maps/walls_and_floors"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/Craftables"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/furniture"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furniture"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("TileSheets/joja_furnitureFront"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/hats"));
					SHelper.GameContent.InvalidateCache(asset => asset.NameWithoutLocale.IsEquivalentTo("Characters/Farmer/shirts"));
					Helper.WriteConfig(Config);
				}
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.ModEnabled,
				setValue: value => Config.ModEnabled = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ReplaceTexts.Name"),
				getValue: () => Config.ReplaceTexts,
				setValue: value => Config.ReplaceTexts = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => Helper.Translation.Get("GMCM.ReplaceTextures.Name"),
				getValue: () => Config.ReplaceTextures,
				setValue: value => Config.ReplaceTextures = value
			);
		}
	}
}
