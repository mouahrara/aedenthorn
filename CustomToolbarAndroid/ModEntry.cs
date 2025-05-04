using System;
using System.Reflection;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;

namespace CustomToolbarAndroid
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

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Toolbar), nameof(Toolbar.draw), new Type[] { typeof(SpriteBatch) }),
					prefix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Toolbar_draw_prefix))),
					transpiler: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Toolbar_draw_transpiler)))
				);
				harmony.Patch(
					original: AccessTools.PropertyGetter(typeof(Toolbar), "maxVisibleItems"),
					postfix: new HarmonyMethod(AccessTools.Method(typeof(ModEntry), nameof(Toolbar_maxVisibleItemsGetter_postfix)))
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
			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
				FieldInfo verticalToolbar = typeof(Options).GetField("verticalToolbar");
				FieldInfo toolbarSlotSize = typeof(Options).GetField("toolbarSlotSize");

				// Register mod
				gmcm.Register(
					mod: ModManifest,
					reset: () => Config = new ModConfig(),
					save: () => Helper.WriteConfig(Config)
				);

				// Main section
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ModEnabled.Name"),
					getValue: () => Config.EnableMod,
					setValue: value => Config.EnableMod = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.VerticalToolbar.Name"),
					getValue: () => (bool)verticalToolbar.GetValue(Game1.options),
					setValue: value => verticalToolbar.SetValue(Game1.options, value)
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.ToolbarSlotSize.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.ToolbarSlotSize.Tooltip"),
					getValue: () => (int)toolbarSlotSize.GetValue(Game1.options),
					setValue: value => toolbarSlotSize.SetValue(Game1.options, value),
					min: 32,
					max: 200
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.PinToolbar.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.PinToolbar.Tooltip"),
					getValue: () => Game1.options.pinToolbarToggle,
					setValue: value => Game1.options.pinToolbarToggle = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.PinnedPosition.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.PinnedPosition.Tooltip"),
					allowedValues: new string[] { "top", "bottom" },
					formatAllowedValue: (string value) => SHelper.Translation.Get("GMCM.PinnedPosition." + value),
					getValue: () => Config.PinnedPosition,
					setValue: value => Config.PinnedPosition = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.OpacityPercentage.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.OpacityPercentage.Tooltip"),
					getValue: () => Config.OpacityPercentage * 100f,
					setValue: value => Config.OpacityPercentage = value / 100f,
					min: 0f,
					max: 100f,
					interval: 1f
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.MaxVisibleItems.Name"),
					tooltip: () => SHelper.Translation.Get("GMCM.MaxVisibleItems.Tooltip"),
					getValue: () => Config.MaxVisibleItems,
					setValue: value => Config.MaxVisibleItems = value,
					min: 1,
					max: 36
				);
			}
		}
	}
}
