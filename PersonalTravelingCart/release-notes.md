[← back to readme](../README.md)

# Release notes

## 0.3.6-unofficial.1-mouahrara
Released on 27 March, 2025, for SMAPI 4.1.0 or later.
* Migrated to 1.6.0 and project cleanup 🚀
* Renamed the project to match its official name 📝
* Improved Harmony patch implementation using Harmony's code API ✨
* Added multiplayer support ✨
* Added a collision box to the traveling cart ✨
* Added **Warp Horses On Day Start** option ✨
* Added **ptc_warp_horses_to_stables** and **ptc_hitch_carts_to_horses** console commands ✨
* Added translation support ✨
* Added French translation 🇫🇷
* Fixed an issue that caused the game to crash when exiting the traveling cart in local multiplayer 🔧
* Fixed an issue that caused the game to crash when attempting to teleport the player to a non-existent location 🔧
* Fixed an issue where it was not possible to leave the traveling cart if the **Draw Cart Exterior** option was set to **false** 🔧
* Fixed an issue where it was impossible to hitch the horse to the traveling carriage when it was facing down 🔧
* Fixed an issue where the player could enter the traveling cart, regardless of the distance 🔧
* Fixed an issue where the traveling cart was drawn behind some game elements when facing up 🔧
* Fixed an issue where a horse could be lost on a replacement map of a passive festival 🔧
* Fixed other minor bugs 🔧
* Removed unused cart.xcf asset 🗑️

> [!NOTE]
> This unofficial update modifies the expected structure for Personal Traveling Cart (PTC) content packs. As a result, existing content packs such as [Pumpkin Carriage (PTC)](https://www.nexusmods.com/stardewvalley/mods/15993) or [Sunflower Carriage](https://www.nexusmods.com/stardewvalley/mods/15845) will need to be updated to follow the new structure in order to be compatible with this unofficial update.
>
> This update introduces two new fields:
> * **middleRect**: The part of the spritesheet drawn at a mid-depth level, just behind the farmer.
> * **collisionRect**: The section of the rectangles (backRect, middleRect, and frontRect) that defines collision boundaries.
>
> Here is an example of a **content.json** file:
> ```json
> {
> 	"Format": "2.5.3",
> 	"Changes": [
> 		{
> 			"Action": "EditData",
> 			"Target": "aedenthorn.PersonalTravellingCart/dictionary",
> 			"Entries": {
> 				"PTCAedenthornCP": {
> 					"mapPath": "aedenthorn.PTCAedenthornCP/map",
> 					"spriteSheetPath": "aedenthorn.PTCAedenthornCP/cart",
> 					"entryTile": {
> 						"X": 6,
> 						"Y": 6
> 					},
> 					"left": {
> 						"backRect": {
> 							"X": 0,
> 							"Y": 0,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"middleRect": {
> 							"X": 0,
> 							"Y": 128,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"frontRect": {
> 							"X": 0,
> 							"Y": 256,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"clickRect": {
> 							"X": 17,
> 							"Y": 26,
> 							"Width": 95,
> 							"Height": 57
> 						},
> 						"hitchRect": {
> 							"X": 0,
> 							"Y": 63,
> 							"Width": 19,
> 							"Height": 13
> 						},
> 						"collisionRect": {
> 							"X": 17,
> 							"Y": 50,
> 							"Width": 95,
> 							"Height": 40
> 						},
> 						"cartOffset": "40, -296",
> 						"playerOffset": "-74, -20",
> 						"frames": 2,
> 						"framerate": 64
> 					},
> 					"right": {
> 						"backRect": {
> 							"X": 0,
> 							"Y": 384,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"middleRect": {
> 							"X": 0,
> 							"Y": 512,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"frontRect": {
> 							"X": 0,
> 							"Y": 640,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"clickRect": {
> 							"X": 17,
> 							"Y": 26,
> 							"Width": 95,
> 							"Height": 57
> 						},
> 						"hitchRect": {
> 							"X": 110,
> 							"Y": 63,
> 							"Width": 19,
> 							"Height": 13
> 						},
> 						"collisionRect": {
> 							"X": 17,
> 							"Y": 50,
> 							"Width": 95,
> 							"Height": 40
> 						},
> 						"cartOffset": "-444, -296",
> 						"playerOffset": "74, -20",
> 						"frames": 2,
> 						"framerate": 64
> 					},
> 					"up": {
> 						"backRect": {
> 							"X": 0,
> 							"Y": 768,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"middleRect": {
> 							"X": 0,
> 							"Y": 0,
> 							"Width": 0,
> 							"Height": 0
> 						},
> 						"frontRect": {
> 							"X": 0,
> 							"Y": 896,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"clickRect": {
> 							"X": 42,
> 							"Y": 27,
> 							"Width": 44,
> 							"Height": 87
> 						},
> 						"hitchRect": {
> 							"X": 52,
> 							"Y": 0,
> 							"Width": 23,
> 							"Height": 27
> 						},
> 						"collisionRect": {
> 							"X": 42,
> 							"Y": 51,
> 							"Width": 44,
> 							"Height": 67
> 						},
> 						"cartOffset": "-204, -60",
> 						"playerOffset": "0, -320",
> 						"frames": 0
> 					},
> 					"down": {
> 						"backRect": {
> 							"X": 0,
> 							"Y": 1024,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"middleRect": {
> 							"X": 0,
> 							"Y": 0,
> 							"Width": 0,
> 							"Height": 0
> 						},
> 						"frontRect": {
> 							"X": 0,
> 							"Y": 1152,
> 							"Width": 128,
> 							"Height": 128
> 						},
> 						"clickRect": {
> 							"X": 42,
> 							"Y": 14,
> 							"Width": 44,
> 							"Height": 87
> 						},
> 						"hitchRect": {
> 							"X": 52,
> 							"Y": 88,
> 							"Width": 23,
> 							"Height": 27
> 						},
> 						"collisionRect": {
> 							"X": 42,
> 							"Y": 38,
> 							"Width": 44,
> 							"Height": 76
> 						},
> 						"cartOffset": "-204, -472",
> 						"playerOffset": "0, 192",
> 						"frames": 0
> 					}
> 				}
> 			}
> 		},
> 		{
> 			"Action": "Load",
> 			"Target": "aedenthorn.PTCAedenthornCP/cart",
> 			"FromFile": "assets/cart.png"
> 		},
> 		{
> 			"Action": "Load",
> 			"Target": "aedenthorn.PTCAedenthornCP/map",
> 			"FromFile": "assets/Cart.tmx"
> 		}
> 	]
> }
> ```
>
> A sample content pack can be downloaded [here](https://raw.githubusercontent.com/mouahrara/aedenthorn/refs/heads/master/PersonalTravelingCart/MiscellaneousFiles/PTCExample.zip).
