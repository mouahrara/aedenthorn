using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.GameData;
using StardewValley.Tools;

namespace HereFishy
{
	public partial class ModEntry : Mod
	{
		internal static IMonitor SMonitor;
		internal static IModHelper SHelper;
		internal static IManifest SModManifest;
		internal static ModConfig Config;
		internal static ModEntry context;

		private static string fishyMalePath;
		private static string fishyFemalePath;
		private static string weePath;

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
			Helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
			Helper.Events.Display.RenderedStep += Display_RenderedStep;
			Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
			Helper.Events.Content.AssetRequested += OnAssetRequested;

			InitAudioPaths();
		}

		private static void InitAudioPaths()
		{
			fishyMalePath = Path.Combine(SHelper.DirectoryPath, "fishy_male.wav");
			fishyFemalePath = Path.Combine(SHelper.DirectoryPath, "fishy_female.wav");
			weePath = Path.Combine(SHelper.DirectoryPath, "wee.wav");

			if (!File.Exists(fishyMalePath))
			{
				fishyMalePath = Path.Combine(SHelper.DirectoryPath, "assets", "fishy_male.wav");
			}
			if (!File.Exists(fishyFemalePath))
			{
				fishyFemalePath = Path.Combine(SHelper.DirectoryPath, "assets", "fishy_female.wav");
			}
			if (!File.Exists(weePath))
			{
				weePath = Path.Combine(SHelper.DirectoryPath, "assets", "wee.wav");
			}
			if (!File.Exists(fishyMalePath))
			{
				fishyMalePath = null;
				SMonitor.Log("The fishy_male.wav file is missing.", LogLevel.Error);
			}
			if (!File.Exists(fishyFemalePath))
			{
				fishyFemalePath = null;
				SMonitor.Log("The fishy_female.wav file is missing.", LogLevel.Error);
			}
			if (!File.Exists(weePath))
			{
				weePath = null;
				SMonitor.Log("The wee.wav file is missing.", LogLevel.Error);
			}
		}

		private static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
		{
			if (e.NameWithoutLocale.IsEquivalentTo("Data/AudioChanges"))
			{
				e.Edit(asset =>
				{
					IDictionary<string, AudioCueData> data = asset.AsDictionary<string, AudioCueData>().Data;

					if (fishyMalePath is not null)
					{
						data.Add($"{SModManifest.UniqueID}_fishy_male", new AudioCueData
						{
							Id = $"{SModManifest.UniqueID}_fishy_male",
							Category = "Sound",
							FilePaths = new List<string>
							{
								fishyMalePath
							},
							StreamedVorbis = false,
							Looped = false,
							UseReverb = false
						});
					}
					if (fishyFemalePath is not null)
					{
						data.Add($"{SModManifest.UniqueID}_fishy_female", new AudioCueData
						{
							Id = $"{SModManifest.UniqueID}_fishy_female",
							Category = "Sound",
							FilePaths = new List<string>
							{
								fishyFemalePath
							},
							StreamedVorbis = false,
							Looped = false,
							UseReverb = false
						});
					}
					if (weePath is not null)
					{
						data.Add($"{SModManifest.UniqueID}_wee", new AudioCueData
						{
							Id = $"{SModManifest.UniqueID}_wee",
							Category = "Sound",
							FilePaths = new List<string>
							{
								weePath
							},
							StreamedVorbis = false,
							Looped = false,
							UseReverb = false
						});
					}
				});
			}
		}

		private void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

			if (SparklingText is not null && SparklingText.update(Game1.currentGameTime))
			{
				SparklingText = null;
			}
		}

		private void Display_RenderedStep(object sender, RenderedStepEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady || e.Step != StardewValley.Mods.RenderSteps.World_AlwaysFront)
				return;

			if (SparklingText is not null && LastUser is not null)
			{
				SparklingText.draw(e.SpriteBatch, Game1.GlobalToLocal(Game1.viewport, LastUser.Position + new Vector2(0f, LastUser.yJumpOffset * 2f) + new Vector2(32f - SparklingText.textWidth / 2f, -160f)));
			}
		}

		private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
		{
			if (!Config.EnableMod || !Context.IsWorldReady)
				return;

			if (e.Button == SButton.MouseRight)
			{
				if (!HereFishying)
				{
					DelayedAction.functionAfterDelay(() =>
					{
						if (Context.CanPlayerMove && (Game1.player.CurrentTool is FishingRod))
						{
							try
							{
								if (Game1.player.currentLocation.waterTiles is not null && Game1.player.currentLocation.waterTiles[(int)Game1.currentCursorTile.X, (int)Game1.currentCursorTile.Y])
								{
									if (Game1.player.Stamina > 0f || Config.StaminaCost <= 0f)
									{
										HereFishy(Game1.player, Game1.currentCursorTile);
									}
									else
									{
										Game1.player.doEmote(36);
										Game1.staminaShakeTimer = 1000;
									}
								}
							}
							catch
							{
								SMonitor.Log($"Error getting water tile");
							}
						}
					}, 0);
				}
				else
				{
					if (CanPerfect)
					{
						WasPerfect = true;
						SparklingText = new SparklingText(Game1.dialogueFont, Game1.content.LoadString("Strings\\UI:BobberBar_Perfect"), Color.Yellow, Color.White, false, 0.1, 1500);
						Game1.playSound("jingle1");
					}
				}
			}
		}

		private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
		{
			// Get Generic Mod Config Menu's API
			IGenericModConfigMenuApi gmcm = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");

			if (gmcm is not null)
			{
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
					name: () => SHelper.Translation.Get("GMCM.PlaySound.Name"),
					getValue: () => Config.PlaySound,
					setValue: value => Config.PlaySound = value
				);
				gmcm.AddTextOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.VoiceGender.Name"),
					allowedValues: new string[] { "Auto", "Male", "Female" },
					formatAllowedValue: (string value) => SHelper.Translation.Get("GMCM.VoiceGender." + value),
					getValue: () => Config.VoiceGender,
					setValue: value => Config.VoiceGender = value
				);
				gmcm.AddNumberOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.StaminaCost.Name"),
					getValue: () => Config.StaminaCost,
					setValue: value => Config.StaminaCost = value
				);
				gmcm.AddBoolOption(
					mod: ModManifest,
					name: () => SHelper.Translation.Get("GMCM.AllowMovement.Name"),
					getValue: () => Config.AllowMovement,
					setValue: value => Config.AllowMovement = value
				);
			}
		}
	}
}
