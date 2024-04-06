using System.Collections.Generic;
using StardewValley;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace FishingChestsExpanded
{
	public partial class ModEntry
	{
		public class ItemGrabMenu_Patch
		{
			public static void Prefix(ref IList<Item> inventory, object context)
			{
				if (!Config.EnableMod || context is not FishingRod)
					return;

				FishingRod fishingRod = context as FishingRod;
				bool treasure = false;

				foreach (Item item in inventory)
				{
					if (item.ItemId != fishingRod.whichFish.LocalItemId)
						treasure = true;
				}
				if (!treasure)
					return;

				Dictionary<string, string> data = Game1.content.Load<Dictionary<string, string>>("Data\\Fish");
				int difficulty = 5;

				if(data.ContainsKey(fishingRod.whichFish.LocalItemId))
				{
					_ = int.TryParse(data[fishingRod.whichFish.LocalItemId].Split('/')[1], out difficulty);
				}

				int coins = advancedLootFrameworkApi.GetChestCoins(difficulty, Config.IncreaseRate, Config.CoinBaseMin, Config.CoinBaseMax);

				IList<Item> items = advancedLootFrameworkApi.GetChestItems(treasuresList, Config.ItemListChances, Config.MaxItems, Config.MinItemValue, Config.MaxItemValue, difficulty, Config.IncreaseRate, Config.ItemsBaseMaxValue);
				bool vanilla = Game1.random.NextDouble() < Config.VanillaLootChance / 100f;

				foreach (Item item in inventory)
				{
					if (item.ItemId == fishingRod.whichFish.LocalItemId || vanilla)
					{
						items.Add(item);
					}
				}
				if (Game1.random.NextDouble() <= 0.33 && Game1.player.team.SpecialOrderRuleActive("DROP_QI_BEANS", null))
				{
					items.Add(new Object("890", Game1.random.Next(1, 3) + ((Game1.random.NextDouble() < 0.25) ? 2 : 0), false, -1, 0));
				}
				inventory = items;
				Game1.player.Money += coins;
				SMonitor.Log($"chest contains {coins} gold");
			}
		}

		public class FishingRod_startMinigameEndFunction_Patch
		{
			public static void Prefix()
			{
				if (!Config.EnableMod)
					return;

				if(Config.ChanceForTreasureChest >= 0)
				{
					FishingRod.baseChanceForTreasure = Config.ChanceForTreasureChest / 100f;
				}
			}
		}
	}
}
