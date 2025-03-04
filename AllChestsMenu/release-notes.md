[← back to readme](../README.md)

# Release notes

## 0.3.2-unofficial.5-mouahrara
Released on 8 March, 2025, for SMAPI 4.1.0 or later.
* Fixed a minor bug 🔧

## 0.3.2-unofficial.4-mouahrara
Released on 5 November, 2024, for SMAPI 4.1.0 or later.
* The chests from the [Overworld Chests](https://www.nexusmods.com/stardewvalley/mods/7710) mod are no longer added to the All Chests Menu ✨

## 0.3.2-unofficial.3-mouahrara
Released on 16 October, 2024, for SMAPI 4.0.0 or later.
* Manual reordering of chests is now persistent ✨
* Sorting by name is now based on the labels of the chests rather than their internal names. Additionally, secondary sorting will use the coordinates of the chests with a configurable priority in X or Y (default), which should display the chests in a more intuitive order ✨
* Added **Include Fridge**, **Include Mini-Fridges**, **Include Mini-Shipping Bins**, **Include Junimo Chests**, **Include Auto-Grabbers** and **Secondary Sorting Priority** options ✨
* Fixed an issue where the held item was lost when the menu of a chest was opened from the All Chests Menu 🔧
* Fixed an issue where renaming Auto-Grabbers wasn't persistent 🔧
* Fixed minor bugs 🔧

## 0.3.2-unofficial.2-mouahrara
Released on 2 April, 2024, for SMAPI 4.0.0 or later.
* Added support for all types of chests (Junimo Chest, Big Chest, Custom chest from other mods...) ✨
* Added **Include Shipping Bin** option to include the shipping bin ✨
* Added **Unrestricted Shipping Bin** option to retrieve items from the shipping bin without being restricted to only the last item placed ✨
* Renaming a chest will now reposition it if the selected sorting option is ascending by name (NA) or descending by name (ND) ✨
* Improved controller support 🎮
* Fixed an issue with renaming Auto-Grabbers 🔧
* Fixed an issue with chest sorting systems 🔧
* Removed harmony patches 🗑️
* Removed **Chest Rows** option 🗑️

Please note that the shipping bin functions differently from other chests. Its storage space is infinite, but you can only retrieve the last item added. Additionally, items do not stack, and functions such as organize, put, take, rename, as well as adding to existing stacks and transferring items from/to another chest are disabled. Enabling the "Unrestricted Shipping Bin" option removes restrictions regarding access to items and their stackability.

## 0.3.2-unofficial.1-mouahrara
Released on 24 March, 2024, for SMAPI 4.0.0 or later.
* Migrated to 1.6.0 and project cleanup 🚀
* Improved Harmony patch implementation using Harmony's code API ✨
* Improved text localization 🌍
* Fixed an issue with the opening of special chests (Junimo Chest, Big Chest...). Special chests are still not compatible with the mod! However, you should now be able to use them outside of the mod context without any problems 🔧
