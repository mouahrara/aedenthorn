using StardewModdingAPI;
using StardewValley;

namespace AllChestsMenu
{
	public partial class ModEntry
	{
		public static void OpenMenu()
		{
			if (Config.ModEnabled && Context.IsPlayerFree)
			{
				Game1.activeClickableMenu = new StorageMenu();
				Game1.playSound("bigSelect");
			}
		}
	}
}
