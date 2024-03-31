using System.IO;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace AllChestsMenu
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			if (!Config.ModEnabled)
				return;

			context = this;

			SMonitor = Monitor;
			SHelper = helper;

			Helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
		}

		public void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
		{
			if (!Config.ModEnabled)
				return;

			if (Game1.activeClickableMenu is AllChestsMenu)
			{
				if (Game1.options.snappyMenus && Game1.options.gamepadControls && e.Button == Config.SwitchButton)
				{
					Game1.playSound("shwip");
					if (!(Game1.activeClickableMenu as AllChestsMenu).focusBottom)
						(Game1.activeClickableMenu as AllChestsMenu).lastTopSnappedCC = Game1.activeClickableMenu.currentlySnappedComponent;
					(Game1.activeClickableMenu as AllChestsMenu).focusBottom = !(Game1.activeClickableMenu as AllChestsMenu).focusBottom;
					Game1.activeClickableMenu.currentlySnappedComponent = null;
					Game1.activeClickableMenu.snapToDefaultClickableComponent();
				}
				if (((Game1.activeClickableMenu as AllChestsMenu).locationText.Selected || (Game1.activeClickableMenu as AllChestsMenu).renameBox.Selected) && e.Button.ToString().Length == 1)
				{
					SHelper.Input.Suppress(e.Button);
				}
			}
			if (e.Button == Config.MenuKey && (Config.ModKey == SButton.None || !Config.ModToOpen || Helper.Input.IsDown(Config.ModKey)))
			{
				OpenMenu();
			}
		}

		public void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
		{
			var phoneAPI = Helper.ModRegistry.GetApi<IMobilePhoneApi>("aedenthorn.MobilePhone");

			phoneAPI?.AddApp("aedenthorn.AllChestsMenu", "AllChestsMenu", OpenMenu, Helper.ModContent.Load<Texture2D>(Path.Combine("assets", "icon.png")));

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
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.LimitToCurrentLocation.Name"),
				getValue: () => Config.LimitToCurrentLocation,
				setValue: value => Config.LimitToCurrentLocation = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ShippingBin.Name"),
				getValue: () => Config.IncludeShippingBin,
				setValue: value => Config.IncludeShippingBin = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.UnrestrictedShippingBin.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.UnrestrictedShippingBin.Tooltip"),
				getValue: () => Config.UnrestrictedShippingBin,
				setValue: value => Config.UnrestrictedShippingBin = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.MenuKey.Name"),
				getValue: () => Config.MenuKey,
				setValue: value => Config.MenuKey = value
			);
			configMenu.AddBoolOption(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModToOpen.Name"),
				getValue: () => Config.ModToOpen,
				setValue: value => Config.ModToOpen = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModKey.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ModKey.Tooltip"),
				getValue: () => Config.ModKey,
				setValue: value => Config.ModKey = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.ModKey2.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.ModKey2.Tooltip"),
				getValue: () => Config.ModKey2,
				setValue: value => Config.ModKey2 = value
			);
			configMenu.AddKeybind(
				mod: ModManifest,
				name: () => SHelper.Translation.Get("GMCM.SwitchButton.Name"),
				tooltip: () => SHelper.Translation.Get("GMCM.SwitchButton.Tooltip"),
				getValue: () => Config.SwitchButton,
				setValue: value => Config.SwitchButton = value
			);
		}
	}
}
