using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using StardewModdingAPI;

namespace MayoMart
{
	public partial class ModEntry
	{
		public static void ReplaceJojaWithMayo(IAssetData data)
		{
			IDictionary<string, string> dict = data.AsDictionary<string, string>().Data;

			foreach(string key in dict.Keys.ToArray())
			{
				string value = dict[key];

				ReplaceJojaWithMayo(ref value);
				dict[key] = value;
			}
		}

		public static void ReplaceJojaWithMayoStringDataFormat(IAssetData data, int[] fieldIndexes)
		{
			IDictionary<string, string> dict = data.AsDictionary<string, string>().Data;

			foreach(string key in dict.Keys.ToArray())
			{
				string value = dict[key];
				string[] array = value.Split('/');

				for (int i = 0; i < array.Length; i++)
				{
					if (fieldIndexes.Contains(i))
					{
						ReplaceJojaWithMayo(ref array[i]);
					}
				}
				dict[key] = string.Join('/', array);
			}
		}

		public static void ReplaceJojaWithMayo(ref string value)
		{
			value = Regex.Replace(value, $"{SHelper.Translation.Get("Replace.Capitalized.Joja")}(?!#)", SHelper.Translation.Get("Replace.Capitalized.Mayo"));
			value = Regex.Replace(value, $"{SHelper.Translation.Get("Replace.Uncapitalized.Joja")}(?!#)", SHelper.Translation.Get("Replace.Uncapitalized.Mayo"));
			value = Regex.Replace(value, $@"\b{SHelper.Translation.Get("Replace.FirstLetter.Joja")}\b(?!#)", SHelper.Translation.Get("Replace.FirstLetter.Mayo"));
			value = Regex.Replace(value, $@"\b{SHelper.Translation.Get("Replace.FirstLetter.Joja")}\b\.(?!#)", SHelper.Translation.Get("Replace.FirstLetter.Mayo") + ".");
		}
	}
}
