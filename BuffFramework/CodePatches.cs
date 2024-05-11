using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Microsoft.Xna.Framework.Audio;
using StardewValley;

namespace BuffFramework
{
	public partial class ModEntry
	{
		public class Buff_OnAdded_Patch
		{
			public static void Postfix(Buff __instance)
			{
				if(!Config.ModEnabled)
					return;

				if (soundBuffs.ContainsKey(__instance.id))
				{
					ICue cue = null;

					if (Game1.soundBank.Exists(soundBuffs[__instance.id].Item1))
					{
						cue = Game1.soundBank.GetCue(soundBuffs[__instance.id].Item1);
						cue.Play();
					};
					soundBuffs[__instance.id] = (soundBuffs[__instance.id].Item1, cue);
				}
			}
		}

		public class Buff_OnRemoved_Patch
		{
			public static void Postfix(Buff __instance)
			{
				if(!Config.ModEnabled)
					return;

				if (soundBuffs.ContainsKey(__instance.id))
				{
					if (Game1.soundBank.Exists(soundBuffs[__instance.id].Item1))
					{
						ICue cue = soundBuffs[__instance.id].Item2;

						if (cue.IsPlaying)
						{
							cue.Stop(AudioStopOptions.Immediate);
						}
					};
				}
				HealthRegenerationBuffs.Remove(__instance.id);
				StaminaRegenerationBuffs.Remove(__instance.id);
				GlowRateBuffs.Remove(__instance.id);
				soundBuffs.Remove(__instance.id);
			}
		}

		public class Farmer_doneEating_Patch
		{
			public static void Prefix(Farmer __instance)
			{
				ApplyBuffsOnEat(__instance);
			}
		}

		public class Farmer_farmerInit_Patch
		{
			public static void Postfix(Farmer __instance)
			{
				__instance.hat.fieldChangeEvent += Hat_fieldChangeEvent;
				__instance.shirtItem.fieldChangeEvent += ShirtItem_fieldChangeEvent;
				__instance.pantsItem.fieldChangeEvent += PantsItem_fieldChangeEvent;
				__instance.boots.fieldChangeEvent += Boots_fieldChangeEvent;
				__instance.leftRing.fieldChangeEvent += LeftRing_fieldChangeEvent;
				__instance.rightRing.fieldChangeEvent += RightRing_fieldChangeEvent;
			}

			public static void Hat_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Hat> field, StardewValley.Objects.Hat oldValue, StardewValley.Objects.Hat newValue)
			{
				ApplyBuffsOnEquip();
			}

			public static void ShirtItem_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Clothing> field, StardewValley.Objects.Clothing oldValue, StardewValley.Objects.Clothing newValue)
			{
				ApplyBuffsOnEquip();
			}

			public static void PantsItem_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Clothing> field, StardewValley.Objects.Clothing oldValue, StardewValley.Objects.Clothing newValue)
			{
				ApplyBuffsOnEquip();
			}

			public static void Boots_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Boots> field, StardewValley.Objects.Boots oldValue, StardewValley.Objects.Boots newValue)
			{
				ApplyBuffsOnEquip();
			}

			public static void LeftRing_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Ring> field, StardewValley.Objects.Ring oldValue, StardewValley.Objects.Ring newValue)
			{
				ApplyBuffsOnEquip();
			}

			public static void RightRing_fieldChangeEvent(Netcode.NetRef<StardewValley.Objects.Ring> field, StardewValley.Objects.Ring oldValue, StardewValley.Objects.Ring newValue)
			{
				ApplyBuffsOnEquip();
			}
		}

		public class BuffManager_GetValues_Patch
		{
			public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
			{
				List<CodeInstruction> list = instructions.ToList();

				for (int i = 0; i < list.Count; i++)
				{
					if (list[i].opcode.Equals(OpCodes.Ldc_R4) && list[i].operand is not null && list[i].operand.Equals(0.05f))
					{
						CodeInstruction[] replacementInstructions = new CodeInstruction[]
						{
							new(OpCodes.Call, typeof(BuffManager_GetValues_Patch).GetMethod(nameof(GetGlowRate), BindingFlags.Public | BindingFlags.Static))
						};
						list.InsertRange(i, replacementInstructions);
						i += replacementInstructions.Length;
						list.RemoveAt(i);
						break;
					}
				}
				return list;
			}

			public static float GetGlowRate()
			{
				if (GlowRateBuffs.Count == 0)
				{
					return Buff.glowRate;
				}
				else
				{
					return GlowRateBuffs.Values.Select(value => GetFloat(value)).Average();
				}
			}
		}
	}
}
