[← back to readme](../README.md)

# Release notes

## 0.4.2-unofficial.2-mouahrara
Released on 5 November, 2024, for SMAPI 4.1.0 or later.
* Fixed an issue where the `GoldCoin` item could be added to chests. Note that this change does not affect the `GetChestCoins` method of the API 🔧

## 0.4.2-unofficial.1-mouahrara
Released on 7 April, 2024, for SMAPI 4.0.0 or later.
* Migrated to 1.6.0 and project cleanup 🚀
* Removed harmony patches 🗑️

Please note that as of version 1.6.0, the `coins` member has been removed from the `Chest` class. The **MakeChest** API method declarations have been adapted to reflect this change.
