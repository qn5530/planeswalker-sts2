# The Planeswalker — a Magic: The Gathering character for Slay the Spire 2

A new playable character whose card pool is built from Magic: The Gathering cards
(Magic Duels / Magic Origins era and the classics). Every card keeps its MTG name and art;
its effect is translated into Slay the Spire 2 mechanics.

Built on [BaseLib](https://github.com/Alchyr/BaseLib-StS2) by Alchyr, for Slay the Spire 2 v0.111.0.

## What's in the mod

**Character: The Planeswalker** (72 HP, 99 gold, 3 battlefield slots). Starter deck: 4x Lightning
Javelin (Strike, +1 Red mana), 4x Enshrouding Mist (Defend, +1 White mana), 1x Fiery Impulse,
1x Savannah Lions. Starter relic: **Sol Ring** (gain 2 Energy at the start of each combat).

### The mana system

Every color of mana is a stacking buff on the Planeswalker that **lasts the whole combat**
(W/U/B/R/G icons under the HP bar). Cards of a color *gain* mana when played; each color's payoff
cards *spend* it ("Spend ALL Red mana: deal 5 damage to ALL enemies, plus once more per mana
spent"), and one finisher per color cannot be played at all until you hold enough of its color
(Time Warp needs 4 Blue, Craterhoof Behemoth needs 3 Green). The card text shows your current
mana in combat.

### Cards (38)

| Color / playstyle | Gain mana | Spend or need mana | Creatures and powers |
|---|---|---|---|
| **Red - aggro, burn** | Lightning Bolt, Rite of Flame, Ball Lightning | Fireball (spend all) | Kiln Fiend (burns whenever you gain Red), Chandra Pyromaster (power), Shivan Dragon (creature) |
| **Blue - control** | Ponder, Brainstorm, Counterspell | Blue Sun's Zenith (spend all), Time Warp (needs 4) | Jace, Vryn's Prodigy (power) |
| **Green - creatures, midrange** | Rampant Growth, Grizzly Bears, Birds of Paradise (creature that taps for mana) | Giant Growth (spend 1), Overrun (spend all), Craterhoof Behemoth (needs 3) | Llanowar Elves (power) |
| **White - lifegain, defense** | Savannah Lions, Wall of Omens, Serra Angel, Comeuppance | Wrath of God (spend all) | Ajani's Pridemate (power) |
| **Black - sacrifice, drain** | Dark Ritual, Doom Blade, Vampire Nighthawk, Altar's Reap | Gray Merchant of Asphodel (spend all) | Dark Confidant (power), Reanimate |
| **Multicolor** | Lightning Helix (R+W), Black Lotus (2 of every color) | | |

Creatures are summoned into the three battlefield slots (the game's orb slots): their Passive
attacks on its own at the end of every turn, and Evoking one is sacrificing it for a bigger hit.
Instants (Counterspell, Comeuppance) are buffs that react when an enemy attacks you.

**Relics (3)**: Sol Ring (starter), Chromatic Lantern (common: 1 mana of every color at combat
start), Sensei's Divining Top (uncommon: Scry 2 at the start of each turn).

## Install

Download Planeswalker-v0.1.0.zip from the [Releases page](https://github.com/qn5530/planeswalker-sts2/releases) (compiled mod, no build needed).


1. Install BaseLib 3.4.7 or newer: put `BaseLib.dll`, `BaseLib.json` and `BaseLib.pck` in
   `<Slay the Spire 2>/mods/` (Steam Workshop subscription also works).
2. Copy the `Planeswalker` folder (`Planeswalker.dll`, `Planeswalker.pck`, `Planeswalker.json`)
   into `<Slay the Spire 2>/mods/Planeswalker/`.
3. Launch the game. The main menu shows "Running Modded"; The Planeswalker is the last portrait on
   the character select screen.

Modded runs use a separate save profile, so vanilla progress is untouched.

## Build from source

Requirements: .NET SDK 9, Slay the Spire 2 installed through Steam (the csproj finds it automatically;
override with `-p:Sts2Path="<path>"`).

```bash
dotnet build -c Release
```

The build compiles the DLL, packs the `Planeswalker/` asset folder into `Planeswalker.pck` with
[BSchneppe.StS2.PckPacker](https://www.nuget.org/packages/BSchneppe.StS2.PckPacker) (no Godot editor
needed), and copies everything into the game's `mods/Planeswalker/` folder.

Layout:

- `MtgSpireCode/` — C# sources (character, card pool, cards, powers, relics).
- `Planeswalker/images/` — card portraits (1000x760 and 250x190), power and relic icons, character UI.
- `Planeswalker/localization/eng/` — card, power, relic, character and ancient-dialogue text.

## Testing tips

The game ships a dev console (press the backtick key in a run). Useful commands:

```
card PLANESWALKER-LIGHTNING_BOLT
relic add PLANESWALKER-SENSEIS_DIVING_TOP
energy 10
fight PHANTASMAL_GARDENERS_ELITE
```

## Credits and legal

- Magic: The Gathering, the card names and the card art are the property of Wizards of the Coast.
  Art is pulled from the [Scryfall](https://scryfall.com) art crops of the original printings; the
  artist of each card is recorded in the C# doc comment of its class. This is a non-commercial fan
  project and is not affiliated with or endorsed by Wizards of the Coast or Mega Crit.
- Slay the Spire 2 by Mega Crit. BaseLib by Alchyr. PckPacker by BSchneppe.
- Rest-site and merchant art reuse the Ironclad's (via BaseLib's PlaceholderCharacterModel); combat uses the mod's own sprite.
- Built with AI assistance (Claude Code).
