[← back to readme](../README.md)

# Release notes

## 0.6.1-unofficial.2-mouahrara
Released on 28 April, 2024, for SMAPI 4.0.0 or later.
* Fixed a minor bug 🔧

## 0.6.1-unofficial.1-mouahrara
Released on 30 March, 2024, for SMAPI 4.0.0 or later.
* Migrated to 1.6.0 and project cleanup 🚀
* Improved Harmony patch implementation using Harmony's code API ✨
* Added new buffs:
  * [CombatLevel](https://stardewvalleywiki.com/Combat)
  * [AttackMultiplier](https://stardewvalleywiki.com/Attack)
  * [CriticalChanceMultiplier](https://stardewvalleywiki.com/Crit._Chance)
  * [CriticalPowerMultiplier](https://stardewvalleywiki.com/Crit._Power)
  * WeaponPrecisionMultiplier
  * [WeaponSpeedMultiplier](https://stardewvalleywiki.com/Speed#Weapon_Speed)
  * [KnockbackMultiplier](https://stardewvalleywiki.com/Weight)
  * [Immunity](https://stardewvalleywiki.com/Immunity)
* **Description** and **DisplaySource** are now [tokenizable strings](https://stardewvalleywiki.com/Modding:Tokenizable_strings) ✨
* Improved the **Consume** field, which now supports QualifiedItemId, ItemId, item categories and item names ✨
* Fields are now case insensitive, and aliases have been added for some of them:
  * Id ↔ BuffId
  * IconSheetIndex ↔ SheetIndex
  * FarmingLevel ↔ Farming
  * MiningLevel ↔ Mining
  * FishingLevel ↔ Fishing
  * ForagingLevel ↔ Foraging
  * CombatLevel ↔ Combat
  * KnockbackMultiplier ↔ WeightMultiplier
  * HealthRegeneration ↔ HealthRegen
  * StaminaRegeneration ↔ StaminaRegen
* An error icon is now displayed if the **SheetIndex** and **TexturePath** fields are both missing ✨
* Sounds are now paused during events and festivals 🔧
* Fixed an issue where the **Glow** field did not correctly consider the alpha (A) value 🔧
* Removed **TextureScale** field. Textures are now automatically resized to the correct size 🗑️
* Removed the unimplemented **Crafting** and **Digging** buffs 🗑️

Please note that a large part of the code has been rewritten to implement custom buffs thanks to the overhaul of buffs in version 1.6. The update should still be fully compatible with the old content packs. With these changes, the **Which** field is deprecated; it is kept for backward compatibility, but it is now recommended to always use **Id**. Additionally, it is no longer necessary to write the effects of buffs in the description for most of them, as they are automatically displayed by the game.
