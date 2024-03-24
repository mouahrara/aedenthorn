using StardewValley;

namespace AllChestsMenu
{
	public partial class ModEntry
	{
		public class ShipObjective_OnItemShipped_Patch
		{
			public static bool Prefix(Item item)
			{
				return item is not null;
			}
		}
	}
}
