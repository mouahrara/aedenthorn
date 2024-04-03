using System;
using System.Collections.Generic;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using Object = StardewValley.Object;

namespace BulkAnimalPurchase
{
	/// <summary>The mod entry point.</summary>
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static ModConfig Config;

		internal static ModEntry context;
		private static ClickableTextureComponent minusButton;
		private static ClickableTextureComponent plusButton;
		private static int animalsToBuy;

		/// <summary>The mod entry point, called after the mod is first loaded.</summary>
		/// <param name="helper">Provides simplified APIs for writing mods.</param>
		public override void Entry(IModHelper helper)
		{
			Config = Helper.ReadConfig<ModConfig>();

			context = this;

			SMonitor = Monitor;
			SHelper = helper;
			helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

			// Load Harmony patches
			try
			{
				Harmony harmony = new(ModManifest.UniqueID);

				harmony.Patch(
					original: AccessTools.Constructor(typeof(PurchaseAnimalsMenu), new Type[] { typeof(List<Object>), typeof(GameLocation) }),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_Patch), nameof(PurchaseAnimalsMenu_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Game1), nameof(Game1.drawDialogueBox), new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(bool),typeof(bool), typeof(string), typeof(bool), typeof(bool), typeof(int), typeof(int), typeof(int) }),
					prefix: new HarmonyMethod(typeof(Game1_drawDialogueBox_Patch), nameof(Game1_drawDialogueBox_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.draw), new Type[] { typeof(SpriteBatch) }),
					transpiler: new HarmonyMethod(typeof(PurchaseAnimalsMenu_draw_Patch), nameof(PurchaseAnimalsMenu_draw_Patch.Transpiler))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.performHoverAction)),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_performHoverAction_Patch), nameof(PurchaseAnimalsMenu_performHoverAction_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.setUpForReturnAfterPurchasingAnimal)),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_setUpForReturnAfterPurchasingAnimal_Patch), nameof(PurchaseAnimalsMenu_setUpForReturnAfterPurchasingAnimal_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick)),
					prefix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_receiveLeftClick_Patch), nameof(PurchaseAnimalsMenu_receiveLeftClick_Patch.Prefix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(PurchaseAnimalsMenu), nameof(PurchaseAnimalsMenu.receiveLeftClick)),
					postfix: new HarmonyMethod(typeof(PurchaseAnimalsMenu_receiveLeftClick_Patch), nameof(PurchaseAnimalsMenu_receiveLeftClick_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(Object), nameof(Object.salePrice)),
					postfix: new HarmonyMethod(typeof(Item_salePrice_Patch), nameof(Item_salePrice_Patch.Postfix))
				);
				harmony.Patch(
					original: AccessTools.Method(typeof(SpriteText), nameof(SpriteText.drawStringWithScrollBackground)),
					prefix: new HarmonyMethod(typeof(SpriteText_drawStringWithScrollBackground_Patch), nameof(SpriteText_drawStringWithScrollBackground_Patch.Prefix))
				);
			}
			catch (Exception e)
			{
				Monitor.Log($"Issue with Harmony patching: {e}", LogLevel.Error);
				return;
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

		public static string AddToString(string str)
		{
			if (!Config.EnableMod)
				return str;
			return str + " " + string.Format(SHelper.Translation.Get("x-left-to-add"), animalsToBuy);
		}
	}
}
