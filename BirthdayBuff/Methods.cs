using System.Reflection;
using StardewValley;

namespace BirthdayBuff
{
	public partial class ModEntry
	{
		private static bool IsBirthdayDay()
		{
			var happyBirthdayModCore = HappyBirthdayAPI.GetType().Assembly.GetType("Omegasis.HappyBirthday.HappyBirthdayModCore");
			var instance = happyBirthdayModCore.GetField("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null);
			var birthdayManager = instance.GetType().GetField("birthdayManager").GetValue(instance);
			var hasChosenBirthday = (bool)birthdayManager.GetType().GetMethod("hasChosenBirthday").Invoke(birthdayManager, null);

			if (!hasChosenBirthday)
				return false;

			var playerBirthdayData = birthdayManager.GetType().GetField("playerBirthdayData").GetValue(birthdayManager);
			var birthdayDay = (int)playerBirthdayData.GetType().GetField("BirthdayDay").GetValue(playerBirthdayData);
			var birthdaySeason = (string)playerBirthdayData.GetType().GetField( "BirthdaySeason").GetValue(playerBirthdayData);

			return Game1.player is not null && Game1.dayOfMonth == birthdayDay && Game1.currentSeason == birthdaySeason.ToLower();
		}
	}
}
