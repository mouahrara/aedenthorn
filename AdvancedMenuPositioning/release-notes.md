[← back to readme](../README.md)

# Release notes

## 0.2.2-unofficial.1-mouahrara
Released on 3 May, 2025, for SMAPI 4.1.0 or later.
* Added support for repositioning the world map menu, the NPC profile menu, and the Junimo note menu ✨
* Fixed compatibility with [Custom Toolbar](https://www.nexusmods.com/stardewvalley/mods/11322) 🔧
* Fixed an issue where the position of the crafting menu items was inconsistent 🔧
* Fixed an issue where mouse scrolling was propagating to all detached menus 🔧
* Fixed an issue where the background dimming effect could remain after detaching some menus 🔧
* Fixed an issue where menu click detection did not scale properly with the UI scale setting 🔧

## 0.2.1-unofficial.2-mouahrara
Released on 1 May, 2024, for SMAPI 4.0.0 or later.
* Fixed an issue with emoji menu 🔧

## 0.2.1-unofficial.1-mouahrara
Released on 24 March, 2024, for SMAPI 4.0.0 or later.
* Migrated to 1.6.0 and project cleanup 🚀
* Improved the internal logic for prioritizing action interceptions. Interacting with a menu now pushes it to the foreground ✨
* Replaced `Sbuttons` with `KeybindList` in the options ✨
* Added the Strict Key option to the GMCM menu to avoid conflicts between mod keys and default game keys. When activated, strict key mode ensures that only designated keys are pressed. This allows for default game actions associated with these keys to be performed (for example, in the crafting menu, **LeftShift** + **LeftMouse** will move the menu, and **LeftShift** + **LeftMouse** + **AnyOtherKey** will craft 5 items at once) ✨
* Added support for held right-clicks ✨
* Added translation support ✨
* Added French translation 🇫🇷
* Added Russian translation (thanks to [@Locked15](https://github.com/Locked15)) 🇷🇺
* Fixed an issue where moving one menu over another could cause unexpected changes in the menu being moved 🔧
* Fixed title screen menu issues. The mod is now disabled until a save is loaded 🔧
* Fixed confusion caused by detached menus remaining open when returning to the title screen 🔧
* Fixed an issue where clicks on the active menu were intercepted by detached menus behind it 🔧
* Fixed an issue where using a chest menu without moving it resulted in duplicated item references 🔧
* Fixed unintended side actions triggered by pressing the move keys while there is already a menu being moved 🔧
* Fixed an issue where using the 'Add to existing stacks' button on a detached menu would remove the item from inventory without updating the menu 🔧
