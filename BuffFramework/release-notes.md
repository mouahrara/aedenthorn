[← back to readme](../README.md)

# Release notes

## 0.6.1-unofficial.6-mouahrara
Released on 10 August, 2024, for SMAPI 4.0.0 or later.
* Improved the **Consume** field, which now supports ContextTags ✨
* Fixed an issue where buffs triggered by a consumable specified by its QualifiedItemId, ItemId or Name, were also triggered by all consumable items belonging to the default category (category 0) 🔧
* Fixed an issue where the duration field did not work as expected when applied to base game buffs 🔧
* Fixed an issue where some optional fields could not be manually set to `null` 🔧

## 0.6.1-unofficial.5-mouahrara
Released on 10 June, 2024, for SMAPI 4.0.0 or later.
* The **HealthRegeneration** and **StaminaRegeneration** fields now support floating-point values ✨
* Added Hungarian translation (thanks to [@martin66789](https://github.com/martin66789)) 🇭🇺

## 0.6.1-unofficial.4-mouahrara
Released on 05 June, 2024, for SMAPI 4.0.0 or later.
* Fixed an issue where if one or more entries specified an additional buff with equipment-based conditions, the buff would only apply if the equipment-based conditions for one of the entries for that specific buff were met, preventing the buff from being applied by any source other than the framework 🔧

## 0.6.1-unofficial.3-mouahrara
Released on 12 May, 2024, for SMAPI 4.0.0 or later.
* Added the following fields:
  * DisplayName
  * AdditionalBuffs
* Added the following aliases:
  * DisplayName ↔ Name
  * Description ↔ DisplayDescription
  * IconSheetIndex ↔ SheetIndex ↔ IconSpriteIndex
  * IconTexture ↔ TexturePath
  * Visible ↔ Visibility
  * MaxStamina ↔ MaxEnergy
  * StaminaRegeneration ↔ StaminaRegen ↔ EnergyRegeneration ↔ EnergyRegen
  * Glow ↔ GlowColor
* Fixed an issue where the buff icon was not displayed for base game buffs 🔧
* Fixed an issue where if multiple entries specified the same buff with equipment-based conditions, the buff would only apply if all equipment-based conditions from all entries for that specific buff were met 🔧

The new **AdditionalBuffs** field allows you to specify a list of buffs to apply in addition to the current custom buff. It's important to note that you cannot define another custom buff added via the framework as an additional buff. The **duration**, **source**, and **visibility** of buffs added in this way match those of the custom buff in which they are defined. However, you can modify the visibility by explicitly defining the **Visible** field.

For example, the following content pack adds a buff that grants +1 attack and +5 magnetism, while also applying the **Monster Musk** buff (Id: 24) invisibly when the Soul Sapper Ring is equipped:

```json
{
	"Format": "2.0.0",
	"Changes": [
		{
			"Action": "EditData",
			"Target": "aedenthorn.BuffFramework/dictionary",
			"Entries": {
				"YourName.YourModName/SoulSapperRing": {
					"Id": "YourName.YourModName/SoulSapperRing",
					"Name": "{{i18n:SoulSapperRing.Name}}",
					"Description": "{{i18n:SoulSapperRing.Description}}",
					"Source": "TheInternalNameOfYourBuffSource",
					"DisplaySource": "{{i18n:SoulSapperRing.Source}}",
					"IconSheetIndex": 14,
					"Ring": "Soul Sapper Ring",
					"Attack": "1",
					"MagneticRadius": "5",
					"AdditionalBuffs": [
						{
							"Id": "24",
							"Visible": false
						}
					],
					"Sound": "cowboy_powerup"
				}
			}
		}
	]
}
```

It is now **strongly recommended** to use this field if you intend to create a custom buff based on a base game buff to avoid potential conflicts with other mods.

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
