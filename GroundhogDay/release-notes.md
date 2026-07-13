[← back to readme](../README.md)

# Release notes

## 0.2.1-unofficial.1-Setrilo
Released for SMAPI 4.0.0 or later.
* Fixed for Stardew Valley 1.6 🚀
* Patches `Game1.newDayAfterFade(Action)` instead of the private `Game1._newDayAfterFade()` iterator, which the .NET 6 JIT inlines at its call sites - silently bypassing a Harmony patch placed directly on it
* Updated the season check from the removed `Game1.currentSeason` string to the `Season` enum
* Also freezes `Game1.stats.DaysPlayed` so days-played-gated content (e.g. the vanilla earthquake event) doesn't fire early
* Also holds back `Farmer.mailForTomorrow` while the mod is enabled, so mail scheduled via any "AddMail ... tomorrow" trigger action (vanilla or a content pack) doesn't arrive on every repeated day
* Restricted the date rewind to the host so a farmhand doesn't desync the date in multiplayer
