using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Objects;
using StardewValley.TokenizableStrings;
using Object = StardewValley.Object;

namespace BuffFramework
{
	public partial class ModEntry
	{
		internal static List<ICue> pausedSounds = new();

		public static void HandleEventAndFestival()
		{
			if (!Config.ModEnabled)
				return;

			Game1.currentLocation.checkForEvents();
			if (Game1.eventUp || Game1.isFestival())
			{
				if (Game1.eventUp && Game1.CurrentEvent is not null)
				{
					Game1.CurrentEvent.onEventFinished += HandleEventAndFestival;
				}
				foreach ((string, ICue) sound in soundBuffs.Values)
				{
					if (sound.Item2.IsPlaying)
					{
						sound.Item2.Pause();
						pausedSounds.Add(sound.Item2);
					}
				}
			}
			else
			{
				foreach (ICue sound in pausedSounds)
				{
					sound.Resume();
				}
				pausedSounds.Clear();
			}
		}

		public static void ApplyBuffsOnEat(Farmer who)
		{
			if (!Config.ModEnabled || !who.IsLocalPlayer)
				return;

			foreach(var kvp in buffDict)
			{
				var key = kvp.Key;
				var value = kvp.Value;
				string id = GetBuffId(key, value);

				if (id is null)
					continue;

				string consume = null;

				foreach (var p in value)
				{
					switch (p.Key.ToLower())
					{
						case "consume":
							consume = GetString(p.Value);
							break;
					}
					if (consume is not null)
						break;
				}

				if (consume is null || !Game1.player.isEating || Game1.player.itemToEat is not Object || ((Game1.player.itemToEat as Object).QualifiedItemId != consume && (Game1.player.itemToEat as Object).ItemId != consume && (Game1.player.itemToEat as Object).Category != GetInt(consume) && (Game1.player.itemToEat as Object).Name != consume))
					continue;

				int duration = new Buff(id).millisecondsDuration;

				CreateOrUpdateBuff(who, id, value, duration > 0 ? duration : Buff.ENDLESS);
			}
		}

		public static void ApplyBuffsOnEquip()
		{
			if (!Config.ModEnabled)
				return;

			Dictionary<string, (Dictionary<string, object>, string)> buffsToAdd = new();
			Dictionary<string, (string, List<string>)> buffsToRemove = new();

			foreach(var kvp in buffDict)
			{
				var key = kvp.Key;
				var value = kvp.Value;
				string id = GetBuffId(key, value);
				bool isValid = true;

				if (id is null)
					continue;

				string hat = null;
				string shirt = null;
				string pants = null;
				string boots = null;
				string ring = null;

				foreach (var p in value)
				{
					switch (p.Key.ToLower())
					{
						case "hat":
							hat = GetString(p.Value);
							break;
						case "shirt":
							shirt = GetString(p.Value);
							break;
						case "pants":
							pants = GetString(p.Value);
							break;
						case "boots":
							boots = GetString(p.Value);
							break;
						case "ring":
							ring = GetString(p.Value);
							break;
					}
					if (hat is not null && shirt is not null && pants is not null && boots is not null && ring is not null)
						break;
				}

				if (hat is null && shirt is null && pants is null && boots is null && ring is null)
					continue;

				static bool isValidRing(Ring ring, string name)
				{
					if (ring.Name == name)
					{
						return true;
					}
					else if (ring is CombinedRing)
					{
						foreach(Ring r in (ring as CombinedRing).combinedRings)
						{
							if(r.Name == name)
							{
								return true;
							}
						}
					}
					return false;
				}

				if (hat is not null && (Game1.player.hat.Value is null || Game1.player.hat.Value.Name != hat))
					isValid = false;
				if (isValid && shirt is not null && (Game1.player.shirtItem.Value is null || Game1.player.shirtItem.Value.Name != shirt))
					isValid = false;
				if (isValid && pants is not null && (Game1.player.pantsItem.Value is null || Game1.player.pantsItem.Value.Name != pants))
					isValid = false;
				if (isValid && boots is not null && (Game1.player.boots.Value is null || Game1.player.boots.Value.Name != boots))
					isValid = false;
				if (isValid && ring is not null && (Game1.player.leftRing.Value is null || !isValidRing(Game1.player.leftRing.Value, ring)) && (Game1.player.rightRing.Value is null || !isValidRing(Game1.player.rightRing.Value, ring)))
					isValid = false;
				if (isValid)
				{
					buffsToAdd[key] = new (value, id);
				}
				buffsToRemove[key] = (id, GetAdditionalBuffsAsTupleList(value)?.Select(t => t.Item1).ToList());
			}
			foreach (var kvp in buffsToRemove)
			{
				if (!buffsToAdd.ContainsKey(kvp.Key))
				{
					string id = kvp.Value.Item1;
					List<string> additionalBuffsIds = kvp.Value.Item2;

					if (Game1.player.hasBuff(id))
					{
						Game1.player.buffs.Remove(id);
						if (additionalBuffsIds is not null)
						{
							foreach (string additionalBuffsId in additionalBuffsIds)
							{
								Game1.player.buffs.Remove(additionalBuffsId);
							}
						}
					}
				}
			}
			foreach (var kvp in buffsToAdd)
			{
				CreateOrUpdateBuff(Game1.player, kvp.Value.Item2, kvp.Value.Item1);
			}
		}

		public static void ApplyBuffsOther()
		{
			foreach(var kvp in buffDict)
			{
				var key = kvp.Key;
				var value = kvp.Value;
				string id = GetBuffId(key, value);

				if (id is null)
					continue;

				string consume = null;
				string hat = null;
				string shirt = null;
				string pants = null;
				string boots = null;
				string ring = null;

				foreach (var p in value)
				{
					switch (p.Key.ToLower())
					{
						case "consume":
							consume = GetString(p.Value);
							break;
						case "hat":
							hat = GetString(p.Value);
							break;
						case "shirt":
							shirt = GetString(p.Value);
							break;
						case "pants":
							pants = GetString(p.Value);
							break;
						case "boots":
							boots = GetString(p.Value);
							break;
						case "ring":
							ring = GetString(p.Value);
							break;
					}
					if (consume is not null || hat is not null || shirt is not null || pants is not null || boots is not null || ring is not null)
						break;
				}

				if (consume is not null || hat is not null || shirt is not null || pants is not null || boots is not null || ring is not null)
					continue;

				CreateOrUpdateBuff(Game1.player, id, value);
			}
		}

		public static void UpdateBuffs()
		{
			var oldBuffDict = buffDict;
			SHelper.GameContent.InvalidateCache(dictKey);
			buffDict = SHelper.GameContent.Load<Dictionary<string, Dictionary<string, object>>>(dictKey);
			foreach (BuffFrameworkAPI instance in APIInstances)
			{
				foreach (var kvp in instance.dictionary)
				{
					if (kvp.Value.Item2 is null || kvp.Value.Item2())
					{
						buffDict.TryAdd(kvp.Key, kvp.Value.Item1);
					}
					else
					{
						buffDict.Remove(kvp.Key);
					}
				}
			}
			foreach (var kvp in oldBuffDict)
			{
				var key = kvp.Key;
				var value = kvp.Value;

				if (!buffDict.ContainsKey(key))
				{
					string id = GetBuffId(key, value);

					if (id is null)
						continue;

					List<string> additionalBuffsIds = GetAdditionalBuffsAsTupleList(value)?.Select(t => t.Item1).ToList();

					if (Game1.player.hasBuff(id))
					{
						Game1.player.buffs.Remove(id);
						if (additionalBuffsIds is not null)
						{
							foreach (string additionalBuffsId in additionalBuffsIds)
							{
								Game1.player.buffs.Remove(additionalBuffsId);
							}
						}
					}
				}
			}
			ApplyBuffsOnEquip();
			ApplyBuffsOther();
		}

		public static string GetBuffId(string key, Dictionary<string, object> value)
		{
			string which = null;
			string buffId = null;
			string id;

			foreach (var p in value)
			{
				switch (p.Key.ToLower())
				{
					case "which":
						which = GetIntAsString(p.Value);
						break;
					case "id":
					case "buffid":
						buffId = GetString(p.Value);
						break;
				}
			}

			if (which is not null && int.Parse(which) >= 0)
			{
				id = which;
			}
			else
			{
				if (buffId is not null)
				{
					id = buffId;
				}
				else
				{
					buffDict.Remove(key);
					SMonitor.Log($"{key}: Which and Id (or BuffId) fields are both missing", LogLevel.Error);
					return null;
				}
			}
			return id;
		}

		public static void CreateOrUpdateBuff(Farmer who, string id, Dictionary<string, object> value, int defaultDuration = Buff.ENDLESS)
		{
			Buff buff = CreateBuff(id, value, defaultDuration);
			List<Buff> additionalBuffs = CreateAdditionalBuffs(buff, value);

			if (who.buffs.IsApplied(buff.id))
			{
				who.buffs.AppliedBuffs[buff.id].millisecondsDuration = who.buffs.AppliedBuffs[buff.id].totalMillisecondsDuration;
			}
			else
			{
				who.buffs.Apply(buff);
			}
			if (additionalBuffs is not null)
			{
				foreach (Buff additionalBuff in additionalBuffs)
				{
					if (who.buffs.IsApplied(additionalBuff.id))
					{
						if (additionalBuff.totalMillisecondsDuration == Buff.ENDLESS)
						{
							who.buffs.AppliedBuffs[additionalBuff.id].millisecondsDuration = additionalBuff.totalMillisecondsDuration;
						}
						else if (who.buffs.AppliedBuffs[additionalBuff.id].totalMillisecondsDuration != Buff.ENDLESS)
						{
							who.buffs.AppliedBuffs[additionalBuff.id].millisecondsDuration = Math.Max(additionalBuff.totalMillisecondsDuration, who.buffs.AppliedBuffs[additionalBuff.id].totalMillisecondsDuration);
						}
						who.buffs.AppliedBuffs[additionalBuff.id].visible = who.buffs.AppliedBuffs[additionalBuff.id].visible && additionalBuff.visible;
					}
					else
					{
						who.buffs.Apply(additionalBuff);
					}
				}
			}
		}

		private static Buff CreateBuff(string id, Dictionary<string, object> value, int defaultDuration = Buff.ENDLESS)
		{
			string iconTexture = null;
			int duration = defaultDuration;
			int maxDuration = defaultDuration;

			foreach (var p in value)
			{
				switch (p.Key.ToLower())
				{
					case "texturepath":
					case "icontexture":
						iconTexture = GetString(p.Value);
						break;
					case "duration":
						duration = GetInt(p.Value) * 1000;
						break;
					case "maxduration":
						maxDuration = GetInt(p.Value) * 1000;
						break;
				}
			}

			int millisecondsDuration = (maxDuration > 0 && maxDuration > duration && duration != Buff.ENDLESS && maxDuration != Buff.ENDLESS) ? Game1.random.Next(duration, maxDuration + 1) : duration;

			Buff buff = new(id)
			{
				millisecondsDuration = millisecondsDuration,
				totalMillisecondsDuration = millisecondsDuration
			};

			if (!string.IsNullOrEmpty(iconTexture))
			{
				Texture2D texture = SHelper.GameContent.Load<Texture2D>(iconTexture);
				int textureX = 0;
				int textureY = 0;
				int textureWidth = texture.Width;
				int textureHeight = texture.Height;

				foreach (var p in value)
				{
					switch (p.Key.ToLower())
					{
						case "texturex":
							textureX = GetInt(p.Value);
							break;
						case "texturey":
							textureY = GetInt(p.Value);
							break;
						case "texturewidth":
							textureWidth = GetInt(p.Value);
							break;
						case "textureheight":
							textureHeight = GetInt(p.Value);
							break;
					}
				}
				buff.iconTexture = ResizeTexture(ExtractTexture(texture, textureX, textureY, textureWidth, textureHeight), 16, 16);
			}
			else
			{
				buff.iconTexture ??= ExtractTexture(Game1.mouseCursors, 320, 496, 16, 16);
			}

			foreach (var p in value)
			{
				switch (p.Key.ToLower())
				{
					case "iconspriteindex":
					case "sheetindex":
					case "iconsheetindex":
						buff.iconSheetIndex = Math.Max(0, GetInt(p.Value));
						if (string.IsNullOrEmpty(iconTexture))
						{
							buff.iconTexture = Game1.buffsIcons;
						}
						break;
					case "name":
					case "displayname":
						buff.displayName = GetString(p.Value, true);
						break;
					case "displaydescription":
					case "description":
						buff.description = GetString(p.Value, true);
						break;
					case "source":
						buff.source = GetString(p.Value);
						break;
					case "displaysource":
						buff.displaySource = GetString(p.Value, true);
						break;
					case "visibility":
					case "visible":
						buff.visible = GetBool(p.Value);
						break;
					case "farming":
					case "farminglevel":
						buff.effects.FarmingLevel.Value = GetFloat(p.Value);
						break;
					case "mining":
					case "mininglevel":
						buff.effects.MiningLevel.Value = GetFloat(p.Value);
						break;
					case "fishing":
					case "fishinglevel":
						buff.effects.FishingLevel.Value = GetFloat(p.Value);
						break;
					case "foraging":
					case "foraginglevel":
						buff.effects.ForagingLevel.Value = GetFloat(p.Value);
						break;
					case "combat":
					case "combatlevel":
						buff.effects.CombatLevel.Value = GetFloat(p.Value);
						break;
					case "attack":
						buff.effects.Attack.Value = GetFloat(p.Value);
						break;
					case "attackmultiplier":
						buff.effects.AttackMultiplier.Value = GetFloat(p.Value);
						break;
					case "criticalchancemultiplier":
						buff.effects.CriticalChanceMultiplier.Value = GetFloat(p.Value);
						break;
					case "criticalpowermultiplier":
						buff.effects.CriticalPowerMultiplier.Value = GetFloat(p.Value);
						break;
					case "weaponprecisionmultiplier":
						buff.effects.WeaponPrecisionMultiplier.Value = GetFloat(p.Value);
						break;
					case "weaponspeedmultiplier":
						buff.effects.WeaponSpeedMultiplier.Value = GetFloat(p.Value);
						break;
					case "weightmultiplier":
					case "knockbackmultiplier":
						buff.effects.KnockbackMultiplier.Value = GetFloat(p.Value);
						break;
					case "defense":
						buff.effects.Defense.Value = GetFloat(p.Value);
						break;
					case "immunity":
						buff.effects.Immunity.Value = GetFloat(p.Value);
						break;
					case "maxenergy":
					case "maxstamina":
						buff.effects.MaxStamina.Value = GetFloat(p.Value);
						break;
					case "luck":
						buff.effects.LuckLevel.Value = GetFloat(p.Value);
						break;
					case "magneticradius":
						buff.effects.MagneticRadius.Value = GetFloat(p.Value);
						break;
					case "speed":
						buff.effects.Speed.Value = GetFloat(p.Value);
						break;
					case "glowcolor":
					case "glow":
						if (p.Value is JObject j)
						{
							buff.glow = new Color((byte)(long)j["R"], (byte)(long)j["G"], (byte)(long)j["B"], (byte)(long)j["A"]);
						}
						else
						{
							Color? c = Utility.StringToColor(p.Value.ToString());

							if (c.HasValue)
							{
								buff.glow = c.Value;
							}
						}
						break;
					case "healthregen":
					case "healthregeneration":
						HealthRegenerationBuffs.TryAdd(id, GetFloatAsString(p.Value));
						break;
					case "energyregen":
					case "energyregeneration":
					case "staminaregen":
					case "staminaregeneration":
						StaminaRegenerationBuffs.TryAdd(id, GetFloatAsString(p.Value));
						break;
					case "glowrate":
						GlowRateBuffs.TryAdd(id, GetFloatAsString(p.Value));
						break;
					case "sound":
						soundBuffs.TryAdd(id, (GetString(p.Value), null));
						break;
				}
			}
			return buff;
		}

		public static List<Buff> CreateAdditionalBuffs(Buff buff, Dictionary<string, object> value)
		{
			List<(string, bool)> additionalBuffsAsTupleList = GetAdditionalBuffsAsTupleList(buff, value);

			if (additionalBuffsAsTupleList is not null)
			{
				List<Buff> additionalBuffs = new();

				foreach ((string id, bool visible) in additionalBuffsAsTupleList)
				{
					additionalBuffs.Add(new Buff(id)
					{
						millisecondsDuration = buff.millisecondsDuration,
						totalMillisecondsDuration = buff.totalMillisecondsDuration,
						visible = visible,
						source = buff.source,
						displaySource = buff.displaySource
					});
				}
				return additionalBuffs;
			}
			return null;
		}

		public static List<(string, bool)> GetAdditionalBuffsAsTupleList(Buff buff, Dictionary<string, object> value)
		{
			return GetAdditionalBuffsAsTupleList(value, buff.visible);
		}

		public static List<(string, bool)> GetAdditionalBuffsAsTupleList(Dictionary<string, object> value, bool visibility = true)
		{
			foreach (var p in value)
			{
				switch (p.Key.ToLower())
				{
					case "additionalbuffs":
						return GetAdditionalBuffsAsTupleListInternal(p.Value, visibility);
				}
			}

			static List<(string, bool)> GetAdditionalBuffsAsTupleListInternal(object value, bool defaultVisibility)
			{
				List<object> additionalBuffsList = (value as JArray)?.ToObject<List<object>>();

				if (additionalBuffsList is not null)
				{
					List<(string, bool)> additionalBuffsAsTupleList = new();

					foreach (object additionalBuff in additionalBuffsList)
					{
						Dictionary<string, object> additionalBuffDictionary = (additionalBuff as JObject)?.ToObject<Dictionary<string, object>>();

						if (additionalBuffDictionary is not null)
						{
							string which = null;
							string buffId = null;
							bool visible = defaultVisibility;
							string id = null;

							foreach (var p in additionalBuffDictionary)
							{
								switch (p.Key.ToLower())
								{
									case "which":
										which = GetIntAsString(p.Value);
										break;
									case "id":
									case "buffid":
										buffId = GetString(p.Value);
										break;
									case "visibility":
									case "visible":
										visible = GetBool(p.Value);
										break;
								}
							}
							if (which is not null && int.Parse(which) >= 0)
							{
								id = which;
							}
							else if (buffId is not null)
							{
								id = buffId;
							}
							if (id is not null)
							{
								additionalBuffsAsTupleList.Add((id, visible));
							}
						}
					}
					return additionalBuffsAsTupleList;
				}
				return null;
			}

			return null;
		}

		public static bool InsensitiveTryGetValue<TKey, TValue>(Dictionary<TKey, TValue> dictionary, TKey key, out TValue value)
		{
			if (dictionary == null)
				throw new ArgumentNullException(nameof(dictionary));

			foreach (var kvp in dictionary)
			{
				if (StringComparer.OrdinalIgnoreCase.Equals(kvp.Key, key))
				{
					value = kvp.Value;
					return true;
				}
			}
			value = default;
			return false;
		}

		public static int GetInt(object value)
		{
			if (value is int i)
			{
				return i;
			}
			else if (value is long l)
			{
				return (int)l;
			}
			else if (value is float f)
			{
				return (int)f;
			}
			else if (value is double d)
			{
				return (int)d;
			}
			else if (value is string s && int.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out i))
			{
				return i;
			}
			else
			{
				return 0;
			}
		}

		public static float GetFloat(object value)
		{
			if (value is int i)
			{
				return i;
			}
			else if (value is long l)
			{
				return l;
			}
			else if (value is float f)
			{
				return f;
			}
			else if (value is double d)
			{
				return (float)d;
			}
			else if (value is string s && float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out f))
			{
				return f;
			}
			else
			{
				return 0f;
			}
		}

		public static bool GetBool(object value)
		{
			if (value is int i)
			{
				return i != 0;
			}
			else if (value is long l)
			{
				return l != 0;
			}
			else if (value is float f)
			{
				return f != 0f;
			}
			else if (value is double d)
			{
				return d != 0d;
			}
			else if (value is bool b)
			{
				return b;
			}
			else if (value is string s)
			{
				string sToLower = s.ToLower();

				if (sToLower.Equals("t") || sToLower.Equals("true"))
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return false;
			}
		}

		public static string GetString(object value, bool tokenizable = false)
		{
			if (value is string s)
			{
				return tokenizable ? TokenParser.ParseText(s) : s;
			}
			else
			{
				return tokenizable ? TokenParser.ParseText(value.ToString()) : value.ToString();
			}
		}

		public static string GetIntAsString(object value)
		{
			if (value is int i)
			{
				return i.ToString();
			}
			else if (value is long l)
			{
				return ((int)l).ToString();
			}
			else if (value is string s)
			{
				return s;
			}
			else
			{
				return "0";
			}
		}

		public static string GetFloatAsString(object value)
		{
			if (value is int i)
			{
				return i.ToString();
			}
			else if (value is long l)
			{
				return ((int)l).ToString();
			}
			if (value is float f)
			{
				return f.ToString();
			}
			else if (value is double d)
			{
				return ((float)d).ToString();
			}
			else if (value is string s)
			{
				return s;
			}
			else
			{
				return "0";
			}
		}

		public static Texture2D ExtractTexture(Texture2D sourceTexture, int x, int y, int width, int height)
		{
			if (x == 0 && y == 0 && width == sourceTexture.Width && height == sourceTexture.Height)
				return sourceTexture;

			Texture2D extractedTexture = new(sourceTexture.GraphicsDevice, width, height);
			Color[] data = new Color[width * height];

			sourceTexture.GetData(0, new Rectangle(x, y, width, height), data, 0, data.Length);
			extractedTexture.SetData(data);
			return extractedTexture;
		}

		public static Texture2D ResizeTexture(Texture2D sourceTexture, int newWidth, int newHeight)
		{
			if (sourceTexture.Width == newWidth && sourceTexture.Height == newHeight)
				return sourceTexture;

			Texture2D resizedTexture = new(sourceTexture.GraphicsDevice, newWidth, newHeight);
			Color[] sourceData = new Color[sourceTexture.Width * sourceTexture.Height];
			Color[] resizedData = new Color[newWidth * newHeight];
			sourceTexture.GetData(sourceData);

			float scaleX = (float)sourceTexture.Width / newWidth;
			float scaleY = (float)sourceTexture.Height / newHeight;

			for (int y = 0; y < newHeight; y++)
			{
				for (int x = 0; x < newWidth; x++)
				{
					float sourceX = x * scaleX;
					float sourceY = y * scaleY;
					int sourceXFloor = (int)Math.Floor(sourceX);
					int sourceYFloor = (int)Math.Floor(sourceY);
					int sourceXCeil = Math.Min(sourceXFloor + 1, sourceTexture.Width - 1);
					int sourceYCeil = Math.Min(sourceYFloor + 1, sourceTexture.Height - 1);
					float weightX = sourceX - sourceXFloor;
					float weightY = sourceY - sourceYFloor;

					Color topLeft = sourceData[sourceYFloor * sourceTexture.Width + sourceXFloor];
					Color topRight = sourceData[sourceYFloor * sourceTexture.Width + sourceXCeil];
					Color bottomLeft = sourceData[sourceYCeil * sourceTexture.Width + sourceXFloor];
					Color bottomRight = sourceData[sourceYCeil * sourceTexture.Width + sourceXCeil];
					Color top = Color.Lerp(topLeft, topRight, weightX);
					Color bottom = Color.Lerp(bottomLeft, bottomRight, weightX);
					Color finalColor = Color.Lerp(top, bottom, weightY);
					resizedData[y * newWidth + x] = finalColor;
				}
			}
			resizedTexture.SetData(resizedData);
			return resizedTexture;
		}

		public static void ClearAll()
		{
			Game1.player.buffs.Clear();
			HealthRegenerationBuffs.Clear();
			StaminaRegenerationBuffs.Clear();
			GlowRateBuffs.Clear();
			soundBuffs.Clear();
		}
	}
}
