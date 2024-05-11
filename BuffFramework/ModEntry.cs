using System;
using System.Collections.Generic;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buffs;

namespace BuffFramework
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;
		internal const string dictKey = "aedenthorn.BuffFramework/dictionary";
		internal static Dictionary<string, Dictionary<string, object>> buffDict = new();

		internal static List<BuffFrameworkAPI> APIInstances = new();

		internal const string healthRegenKey = "aedenthorn.BuffFramework.healthRegen";
		internal const string staminaRegenKey = "aedenthorn.BuffFramework.staminaRegen";
		internal const string soundKey = "aedenthorn.BuffFramework.sound";
		internal static PerScreen<Dictionary<string, string>> healthRegenerationBuffs = new(() => new());
		internal static PerScreen<Dictionary<string, string>> staminaRegenerationBuffs = new(() => new());
		internal static PerScreen<Dictionary<string, string>> glowRateBuffs = new(() => new());
		internal static Dictionary<string, (string, ICue)> soundBuffs = new();

		internal static Dictionary<string, string> HealthRegenerationBuffs
		{
			get => healthRegenerationBuffs.Value;
			set => healthRegenerationBuffs.Value = value;
		}

		internal static Dictionary<string, string> StaminaRegenerationBuffs
		{
			get => staminaRegenerationBuffs.Value;
			set => staminaRegenerationBuffs.Value = value;
		}

		internal static Dictionary<string, string> GlowRateBuffs
		{
			get => glowRateBuffs.Value;
			set => glowRateBuffs.Value = value;
		}

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.Player.Warped += Player_Warped;
			Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
			Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
			Helper.Events.GameLoop.ReturnedToTitle += GameLoop_ReturnedToTitle;
			Helper.Events.GameLoop.OneSecondUpdateTicked += GameLoop_OneSecondUpdateTicked;
			Helper.Events.Content.AssetRequested += Content_AssetRequested;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Method(typeof(Buff), nameof(Buff.OnAdded)),
					postfix: new HarmonyMethod(typeof(Buff_OnAdded_Patch), nameof(Buff_OnAdded_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Buff), nameof(Buff.OnRemoved)),
					postfix: new HarmonyMethod(typeof(Buff_OnRemoved_Patch), nameof(Buff_OnRemoved_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), nameof(Farmer.doneEating)),
					prefix: new HarmonyMethod(typeof(Farmer_doneEating_Patch), nameof(Farmer_doneEating_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Farmer), "farmerInit"),
					postfix: new HarmonyMethod(typeof(Farmer_farmerInit_Patch), nameof(Farmer_farmerInit_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(BuffManager), nameof(BuffManager.GetValues)),
					transpiler: new HarmonyMethod(typeof(BuffManager_GetValues_Patch), nameof(BuffManager_GetValues_Patch.Transpiler))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
			}
		}

		public override object GetApi(IModInfo mod)
		{
			BuffFrameworkAPI instance = new();

			APIInstances.Add(instance);
			return instance;
		}

		public void GameLoop_OneSecondUpdateTicked(object sender, StardewModdingAPI.Events.OneSecondUpdateTickedEventArgs e)
		{
			if(!Config.ModEnabled || !Game1.shouldTimePass())
				return;

			foreach (string healthRegen in HealthRegenerationBuffs.Values)
			{
				Game1.player.health = Math.Clamp(Game1.player.health + GetInt(healthRegen), 0, Game1.player.maxHealth);
			}
			foreach (string staminaRegen in StaminaRegenerationBuffs.Values)
			{
				Game1.player.Stamina = Math.Clamp(Game1.player.Stamina + GetInt(staminaRegen), 0, Game1.player.MaxStamina);
			}
		}

		public void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
		}

		public void Player_Warped(object sender, StardewModdingAPI.Events.WarpedEventArgs e)
		{
			HandleEventAndFestival();
			Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
		}

		public void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
		{
			Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
		}

		public void GameLoop_UpdateTicking(object sender, StardewModdingAPI.Events.UpdateTickingEventArgs e)
		{
			UpdateBuffs();
			Helper.Events.GameLoop.UpdateTicking -= GameLoop_UpdateTicking;
		}

		public void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
		{
			ClearAll();
		}

		public void GameLoop_ReturnedToTitle(object sender, StardewModdingAPI.Events.ReturnedToTitleEventArgs e)
		{
			ClearAll();
		}

		public void Content_AssetRequested(object sender, StardewModdingAPI.Events.AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo(dictKey))
			{
				e.LoadFrom(() => new Dictionary<string, Dictionary<string, object>>(), StardewModdingAPI.Events.AssetLoadPriority.Exclusive);
			}
		}

		public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (configMenu is not null)
			{
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
}
