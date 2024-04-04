using System;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.ItemTypeDefinitions;
using StardewValley.Objects;

namespace ConnectedGardenPots
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static Texture2D gardenPotspriteSheet;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					prefix: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(IndoorPot), nameof(IndoorPot.draw), new Type[] { typeof(SpriteBatch), typeof(int), typeof(int), typeof(float) }),
					postfix: new HarmonyMethod(typeof(IndoorPot_draw_Patch), nameof(IndoorPot_draw_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ParsedItemData), nameof(ParsedItemData.GetTexture)),
					prefix: new HarmonyMethod(typeof(ParsedItemData_GetTexture_Patch), nameof(ParsedItemData_GetTexture_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(ParsedItemData), nameof(ParsedItemData.GetSourceRect), new Type[] { typeof(int), typeof(int) }),
					prefix: new HarmonyMethod(typeof(ParsedItemData_GetSourceRect_Patch), nameof(ParsedItemData_GetSourceRect_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e)
		{
			try
			{
				gardenPotspriteSheet = Game1.content.Load<Texture2D>("aedenthorn.ConnectedGardenPots/sprite_sheet");
			}
			catch
			{
				gardenPotspriteSheet = Helper.ModContent.Load<Texture2D>("assets/sprite_sheet.png");
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
				save: () => Helper.WriteConfig(Config)
			);

			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
				getValue: () => Config.EnableMod,
				setValue: value => Config.EnableMod = value
			);
		}
	}
}
