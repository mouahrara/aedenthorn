[← back to readme](../README.md)

# Release notes

## 0.2.1-unofficial.3-mouahrara
Released on 9 December, 2024, for SMAPI 4.1.0 or later.
* Improved support for custom farms ✨

> [!TIP]
> The farm type of a custom farm corresponds to its **Id** in the asset `Data/AdditionalFarms`. To get the farm type of a custom farm added via **Content Patcher**, you can run the SMAPI console command `patch export "Data/AdditionalFarms"`, then check the **Id** of the farm in the file `{GamePath}/patch export/Data_AdditionalFarms.json`.

## 0.2.1-unofficial.2-mouahrara
Released on 12 May, 2024, for SMAPI 4.0.0 or later.
* Added the **FarmTypes** field to specify a list of farms where the item should be included in the starter package. If all types of farms in the list are preceded by `!`, then the list is considered as an exclusion list, and the item is added to the starter package of all farms except those in the list ✨
* Fixed an issue where the mod was not working on Meadowlands Farm 🔧

> [!NOTE]
> The following are the farm types for the base game farms:
> * Standard Farm: `Standard`
> * Riverland Farm: `Riverland`
> * Forest Farm: `Forest`
> * Hill-top Farm: `Hilltop`
> * Wilderness Farm: `Wilderness`
> * Four Corners Farm: `FourCorners`
> * Beach Farm: `Beach`
> * Meadowlands Farm: `MeadowlandsFarm`

## 0.2.1-unofficial.1-mouahrara
Released on 1 May, 2024, for SMAPI 4.0.0 or later.
* Migrated to 1.6.0 and project cleanup 🚀
* Added the **Tool** type to replace the types: **Axe**, **FishingRod**, **Hoe**, **Pan**, **Pickaxe**, **Shears**, and **WateringCan**. Note that the old types are kept for backward compatibility ✨
* The **BigCraftable** type replaces the **Chest** type. Note that the **Chest** type is kept for backward compatibility ✨
* The watering cans added to the starter package are now empty ✨
* Improved translation support ✨
* Added French translation 🇫🇷
* Fixed an issue where BigCraftables objects could not be added by their Id 🔧
* Fixed an issue where Furniture objects could not be added 🔧
* Fixed an issue where the starting package was invisible if no objects had been added 🔧
