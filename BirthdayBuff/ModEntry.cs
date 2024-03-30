using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace BirthdayBuff
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;
		internal const string BuffFrameworkKey = "aedenthorn.BuffFramework/dictionary";
		internal static object HappyBirthdayAPI;
		internal static IBuffFrameworkAPI BuffFrameworkAPI;
		internal static bool cachedResult = false;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
			Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
			Helper.Events.GameLoop.DayEnding += GameLoop_DayEnding;
		}

		private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			cachedResult = IsBirthdayDay();
		}

		private void GameLoop_DayEnding(object sender, StardewModdingAPI.Events.DayEndingEventArgs e)
		{
			cachedResult = false;
		}

		private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
		{
			if (Game1.ticks <= 1)
				return;

			HappyBirthdayAPI = Helper.ModRegistry.GetApi("Omegasis.HappyBirthday");
			BuffFrameworkAPI = Helper.ModRegistry.GetApi<IBuffFrameworkAPI>("aedenthorn.BuffFramework");

			BuffFrameworkAPI.Add($"{ModManifest.UniqueID}/HappyBirthday", new Dictionary<string, object>()
				{
					{ "buffId", $"{ModManifest.UniqueID}/HappyBirthday" },
					{ "description", SHelper.Translation.Get("birthday-buff-description") },
					{ "source", ModManifest.UniqueID },
					{ "displaySource", SHelper.Translation.Get("birthday-buff-displaySource") },
					{ "texturePath", "Maps/springobjects" },
					{ "textureX", "80" },
					{ "textureY", "144" },
					{ "textureWidth", "16" },
					{ "textureHeight", "16" },
					{ "farming", Config.Farming },
					{ "mining", Config.Mining },
					{ "foraging", Config.Foraging },
					{ "fishing", Config.Fishing },
					{ "attack", Config.Attack },
					{ "defense", Config.Defense },
					{ "speed", Config.Speed },
					{ "magneticRadius", Config.MagneticRadius },
					{ "luck", Config.Luck },
					{ "maxStamina", Config.MaxStamina },
					{ "sound", Config.Sound },
					{ "glow", Config.GlowColor },
					{ "glowRate", Config.GlowRate }
				}, () => {
					return cachedResult;
				});
			Helper.Events.GameLoop.UpdateTicked -= GameLoop_UpdateTicked;
		}

		private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
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
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.480").Trim(),
					getValue: () => Config.Farming,
					setValue: value => Config.Farming = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.486").Trim(),
					getValue: () => Config.Mining,
					setValue: value => Config.Mining = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.492").Trim(),
					getValue: () => Config.Foraging,
					setValue: value => Config.Foraging = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.483").Trim(),
					getValue: () => Config.Fishing,
					setValue: value => Config.Fishing = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.504").Trim(),
					getValue: () => Config.Attack,
					setValue: value => Config.Attack = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.501").Trim(),
					getValue: () => Config.Defense,
					setValue: value => Config.Defense = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.507").Trim(),
					getValue: () => Config.Speed,
					setValue: value => Config.Speed = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.498").Trim(),
					getValue: () => Config.MagneticRadius,
					setValue: value => Config.MagneticRadius = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.489").Trim(),
					getValue: () => Config.Luck,
					setValue: value => Config.Luck = value
				);
				configMenu.AddNumberOption(
					mod: ModManifest,
					name: () => Game1.content.LoadString("Strings\\StringsFromCSFiles:Buff.cs.495").Trim(),
					getValue: () => Config.MaxStamina,
					setValue: value => Config.MaxStamina = value
				);
				configMenu.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.Sound.Name"),
					getValue: () => Config.Sound,
					setValue: value => Config.Sound = value
				);
			}
		}
	}
}
