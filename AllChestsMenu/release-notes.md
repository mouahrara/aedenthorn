[← back to readme](../README.md)

# Release notes

## 0.3.2-unofficial.2-mouahrara
Released on 2 April, 2024, for SMAPI 4.0.0 or later.
* Added support for all types of chests (Junimo Chest, Big Chest, Custom chest from other mods...) ✨
* Added **Include Shipping Bin** option to include the shipping bin ✨
* Added **Unrestricted Shipping Bin** option to retrieve items from the shipping bin without being restricted to only the last item placed ✨
* Renaming a chest will now reposition it if the selected sorting option is ascending by name (NA) or descending by name (ND) ✨
* Improved controller support 🎮
* Fixed a bug with renaming Auto-Grabbers 🔧
* Fixed a bug with chest sorting systems 🔧
* Removed harmony patches 🗑️
* Removed **Chest Rows** option 🗑️

Please note that the shipping bin functions differently from other chests. Its storage space is infinite, but you can only retrieve the last item added. Additionally, items do not stack, and functions such as organize, put, take, rename, as well as adding to existing stacks and transferring items from/to another chest are disabled. Enabling the "Unrestricted Shipping Bin" option removes restrictions regarding access to items and their stackability.

## 0.3.2-unofficial.1-mouahrara
Released on 24 March, 2024, for SMAPI 4.0.0 or later.
* Migrated to 1.6.0 and project cleanup 🚀
* Improved Harmony patch implementation using Harmony's code API ✨
* Improved text localization 🌍
* Fixed an issue with the opening of special chests (Junimo Chest, Big Chest...). Special chests are still not compatible with the mod! However, you should now be able to use them outside of the mod context without any problems 🔧
