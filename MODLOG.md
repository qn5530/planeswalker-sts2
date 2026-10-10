# MODLOG - engine notes for the Planeswalker mod

Things learned while building the mod against Slay the Spire 2 v0.111.0 and BaseLib 3.4.7.
Paths are relative to the game install (`<game>`).

## Game and loader
- Engine: MegaDot (Mega Crit's Godot 4.5 fork), .NET 9. Ships 0Harmony.dll and MonoMod.
- Built-in mod loader reads `<game>/mods/<Id>/`: `<Id>.dll` + `<Id>.pck` + `<Id>.json` manifest
  (id, name, author, description, version, min_game_version, has_pck, has_dll, dependencies,
  affects_gameplay). Entry point is a class with `[ModInitializer(nameof(Initialize))]`.
  Every `*.json` in the mods folder is treated as a manifest.
- Modded runs use their own save profile (`modded/profile1`), so vanilla progress is untouched.
- Game log: `%APPDATA%\SlayTheSpire2\logs\godot.log`. `[INFO] Player 1 playing card <ID>` lines make
  automated card tests verifiable.

## Build
- `dotnet build -c Release` compiles the dll, packs `Planeswalker/` into the pck with
  `BSchneppe.StS2.PckPacker` (no Godot editor needed) and copies dll, pck and json to
  `<game>/mods/Planeswalker/`. The game locks the dll while running: close it before rebuilding.
- Card classes and `localization/eng/cards.json` are generated from the spec table in
  `tools/gen_cards.py`; images from Scryfall art crops by `tools/gen_images.py` (`tools/fetch.py`
  downloads the crops). `Cards/PlaneswalkerCard.cs` is the hand-written base class.

## Localization
- Vanilla format: flat keys `CARD_ID.title`, `CARD_ID.description`, `CARD_ID.UP.description`.
- Mod text lives in the pck at `res://<ModId>/localization/eng/*.json`. Model ids are
  `<ROOT_NAMESPACE_UPPER>-<CLASS_NAME_SNAKE_UPPER>` (BaseLib prefix), e.g.
  `PLANESWALKER-LIGHTNING_BOLT.title`; ancient dialogue keys mirror the Ironclad's
  (`NEOW.talk.PLANESWALKER-THE_PLANESWALKER.0-0.ancient`, 57 keys).
- Rich text colors the game renders: `[gold] [red] [blue] [green] [purple] [orange]`. White mana text
  uses gold, Black mana uses purple.
- The game appends keyword text ("Exhaust.") itself for `CardKeyword.Exhaust`; writing it in the
  description shows it twice.
- `{X:energyIcons()}` only works on an `EnergyVar`; a plain `DynamicVar` throws a formatting error.
- Live values in combat: `CalculationBaseVar(0)` + `CalculationExtraVar(1)` +
  `CalculatedVar("Have").WithMultiplier(static lambda)` with `{InCombat:...{Have}...|}` in the text
  (vanilla Barrage pattern). The multiplier lambda must not capture a model instance.

## Engine behaviour
- Asset paths must use forward slashes; `Path.Join` on Windows produces `res://Planeswalker\images\...`
  and an "Asset not cached" warning.
- Relic energy at combat start: `AfterSideTurnStart` (after the energy reset), like Booming Conch.
  `BeforeSideTurnStart` runs before the reset and the energy is lost.
- Powers: `AfterPlayerTurnStart` runs after energy reset and draw; `AfterSideTurnEnd` for end-of-turn
  effects; `AfterDamageReceived` / `ModifyDamageMultiplicative` for reactions to enemy attacks.
- A `TemporaryStrengthPower` subclass with `OriginModel => card` gives Flex-style temporary Strength.
- Orb slots work as battlefield slots for creature summons (`CustomOrbModel`, `OrbCmd.Channel`,
  `OrbCmd.Passive`, `OrbCmd.EvokeNext`). The slot count is baked into a run at creation.
- `IsPlayable` override gates a card; the game draws the unplayable icon on it by itself.

## Mana system
- Five `ManaPower` buffs (`Powers/ManaPowers.cs`), Counter stack, combat-scoped. Every gain goes
  through `Mana.Gain<T>()` so "whenever you gain Red mana" (Kiln Fiend) can trigger;
  `Mana.SpendAll<T>()` and `Mana.Spend<T>(n)` consume.
- Verified in game: every card plays without exceptions; Time Warp is refused below 4 Blue and plays
  after Black Lotus x2; Fireball with 8 Red mana hits 9 times; start-of-turn powers add their mana.

## Dev console (release build)
- Backtick toggles it (Escape also hides it but opens the pause menu).
- `card <ID> [pile]`, `relic add <ID>`, `energy <n>`, `heal <n>`, `block <n> <index>`,
  `damage <n> <index>`, `fight <ENCOUNTER_ID>` (also from the map), `kill all`, `godmode`,
  `act OVERGROWTH|HIVE|UNDERDOCKS|GLORY`, `trailer` (toggles UI-hiding hotkeys 0-9 and -/= speed).
- Encounter ids are the encounter class names in UPPER_SNAKE (`KAISER_CRAB_BOSS`,
  `PHROG_PARASITE_ELITE`, `SLIMES_NORMAL`, ...).

## Ideas
- Rest-site and merchant art (still the Ironclad placeholder).
- Custom card frame instead of the HSV-shifted red frame.
- Balance pass on upgrades; potions for the empty potion pool.
