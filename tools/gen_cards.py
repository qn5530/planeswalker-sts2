"""Generates the Planeswalker card classes and cards.json from one spec table.
Run: python -I -X utf8 gen_cards.py <project_root> <scryfall cards.json>
"""
import json, os, re, sys

ROOT = sys.argv[1]
SCRY = json.load(open(sys.argv[2], encoding="utf-8"))
OUT_CS = os.path.join(ROOT, "MtgSpireCode", "Cards")
OUT_LOC = os.path.join(ROOT, "Planeswalker", "localization", "eng", "cards.json")
PREFIX = "PLANESWALKER-"

def snake(cls):
    return re.sub(r"(?<!^)(?=[A-Z])", "_", cls).upper()

# --- reusable C# snippets -------------------------------------------------
ATK = ('ArgumentNullException.ThrowIfNull(cardPlay.Target);\n'
       '        await DamageCmd.Attack(DynamicVars.Damage.BaseValue){hits}.FromCard(this, cardPlay).Targeting(cardPlay.Target)\n'
       '            .WithHitFx("{fx}")\n'
       '            .Execute(choiceContext);')
ATK_ALL = ('await DamageCmd.Attack(DynamicVars.Damage.BaseValue){hits}.FromCard(this, cardPlay).TargetingAllOpponents(CombatState)\n'
           '            .WithHitFx("{fx}")\n'
           '            .Execute(choiceContext);')
BLOCK = 'await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);'
DRAW = 'await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);'
ENERGY = 'await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);'
HEAL = 'await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);'
HPLOSS = 'await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);'
POWERUP = 'await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);'
def apply_target(p): return f'await PowerCmd.Apply<{p}>(choiceContext, cardPlay.Target, DynamicVars["{p}"].BaseValue, Owner.Creature, this);'
def apply_self(p, var=None): return f'await PowerCmd.Apply<{p}>(choiceContext, Owner.Creature, DynamicVars["{var or p}"].BaseValue, Owner.Creature, this);'
def apply_all(p): return f'await PowerCmd.Apply<{p}>(choiceContext, CombatState?.HittableEnemies, DynamicVars["{p}"].BaseValue, Owner.Creature, this);'
def atk(fx="vfx/vfx_attack_slash", hits=1): return ATK.format(fx=fx, hits=f".WithHitCount({hits})" if hits > 1 else "")
def atk_all(fx="vfx/vfx_attack_slash"): return ATK_ALL.format(fx=fx, hits="")
SLASH, BLUNT, FIRE = "vfx/vfx_attack_slash", "vfx/vfx_attack_blunt", "vfx/vfx_attack_slash"
def up(var, n): return f'DynamicVars["{var}"].UpgradeValueBy({n}m);'
def upcost(n): return f'EnergyCost.UpgradeBy({n});'
def tip_power(p): return f'HoverTipFactory.FromPower<{p}>()'
EXH = 'HoverTipFactory.FromKeyword(CardKeyword.Exhaust)'
ETIP = 'EnergyHoverTip'
BTIP = 'HoverTipFactory.Static(StaticHoverTip.Block)'

# --- card spec -------------------------------------------------------------
# cls, mtg name, cost, type, rarity, target, vars, body lines, upgrade lines, description, ...
C = []
def card(cls, mtg, cost, type_, rarity, target, vars_, body, upgrade, desc, keywords=(), tags=(), tips=(), extra=None, hasx=False, title=None, members=(), color=None):
    C.append(dict(cls=cls, mtg=mtg, cost=cost, type=type_, rarity=rarity, target=target, vars=vars_, body=body,
                  upgrade=upgrade, desc=desc, keywords=keywords, tags=tags, tips=tips, extra=extra or {}, hasx=hasx, title=title or mtg, members=members, color=color))

DMG = lambda n: f"new DamageVar({n}m, ValueProp.Move)"
BLK = lambda n: f"new BlockVar({n}m, ValueProp.Move)"
CARDS = lambda n: f"new CardsVar({n})"
EN = lambda n: f"new EnergyVar({n})"
HL = lambda n: f"new HealVar({n}m)"
HP = lambda n: f"new HpLossVar({n}m)"
PV = lambda p, n: f"new PowerVar<{p}>({n}m)"
DV = lambda name, n: f'new DynamicVar("{name}", {n}m)'

# --- mana ------------------------------------------------------------------
# Five colors of mana, each a stacking power that lasts the whole combat (see Powers/ManaPowers.cs).
MANA = {"W": ("White", "[gold]White mana[/gold]"), "U": ("Blue", "[blue]Blue mana[/blue]"), "B": ("Black", "[purple]Black mana[/purple]"),
        "R": ("Red", "[red]Red mana[/red]"), "G": ("Green", "[green]Green mana[/green]")}
def mp(c): return f"{MANA[c][0]}ManaPower"
def mtxt(c): return MANA[c][1]
def mtip(c): return tip_power(mp(c))
def gain(c, var="Gain"): return f'await Mana.Gain<{mp(c)}>(choiceContext, Owner.Creature, DynamicVars["{var}"].IntValue, this);'
def spend_all(c): return f"int x = await Mana.SpendAll<{mp(c)}>(Owner.Creature);"
def HAVE(c): return ["new CalculationBaseVar(0m)", "new CalculationExtraVar(1m)",
                     f'new CalculatedVar("Have").WithMultiplier((CardModel card, Creature? _) => card.Owner.Creature.GetPowerAmount<{mp(c)}>())']
def have_txt(c): return "{InCombat:\n(You have {Have} " + MANA[c][0] + " mana)|}"
def gated(c): return [f'protected override bool IsPlayable => Owner?.Creature == null || Owner.Creature.GetPowerAmount<{mp(c)}>() >= DynamicVars["Need"].IntValue;']
def pay(c): return f'if (!await Mana.Spend<{mp(c)}>(Owner.Creature, DynamicVars["Need"].IntValue)) return;'
def summon(orb): return [f"await OrbCmd.Channel<{orb}>(choiceContext, Owner);"]
def orbtips(orb): return [f"HoverTipFactory.FromOrb<{orb}>()", "HoverTipFactory.Static(StaticHoverTip.Channeling)"]
def gain_txt(c, var="Gain"): return "Gain {" + var + ":diff()} " + mtxt(c) + "."

# ============================== BASIC ======================================
card("LightningJavelin", "Lightning Javelin", 1, "Attack", "Basic", "AnyEnemy", [DMG(6), DV("Gain", 1)], [atk(FIRE), gain("R")], [up("Damage", 3)],
     "Deal {Damage:diff()} damage.\n" + gain_txt("R"), tags=["CardTag.Strike"], tips=[mtip("R")], color="R")
card("EnshroudingMist", "Enshrouding Mist", 1, "Skill", "Basic", "Self", [BLK(5), DV("Gain", 1)], [BLOCK, gain("W")], [up("Block", 3)],
     "Gain {Block:diff()} [gold]Block[/gold].\n" + gain_txt("W"), tags=["CardTag.Defend"], tips=[mtip("W")], color="W")
card("FieryImpulse", "Fiery Impulse", 2, "Attack", "Basic", "AnyEnemy", [DMG(8), PV("VulnerablePower", 2)],
     [atk(FIRE), apply_target("VulnerablePower")], [up("Damage", 2), up("VulnerablePower", 1)],
     "Deal {Damage:diff()} damage.\nApply {VulnerablePower:diff()} [gold]Vulnerable[/gold].", tips=[tip_power("VulnerablePower")], color="R")

# ============================== RED: aggro / burn ==========================
card("LightningBolt", "Lightning Bolt", 1, "Attack", "Common", "AnyEnemy", [DMG(9), DV("Gain", 1)], [atk(FIRE), gain("R")], [up("Damage", 3)],
     "Deal {Damage:diff()} damage.\n" + gain_txt("R"), tips=[mtip("R")], color="R")
card("RiteOfFlame", "Rite of Flame", 0, "Skill", "Common", "Self", [DV("Gain", 2)], [gain("R")], [up("Gain", 1)],
     gain_txt("R"), tips=[mtip("R")], color="R")
card("KilnFiend", "Kiln Fiend", 1, "Power", "Uncommon", "Self", [DV("Power", 2)], [POWERUP, apply_self("KilnFiendPower", "Power")], [up("Power", 1)],
     "Whenever you gain " + mtxt("R") + ", deal {Power:diff()} damage to a random enemy for each mana gained.", tips=[tip_power("KilnFiendPower"), mtip("R")], color="R")
card("BallLightning", "Ball Lightning", 1, "Attack", "Common", "AnyEnemy", [DMG(14), DV("Gain", 1)], [atk(FIRE), gain("R")], [up("Damage", 4)],
     "Deal {Damage:diff()} damage.\n" + gain_txt("R"), keywords=["CardKeyword.Ethereal", "CardKeyword.Exhaust"],
     tips=["HoverTipFactory.FromKeyword(CardKeyword.Ethereal)", EXH, mtip("R")], color="R")
card("Fireball", "Fireball", 1, "Attack", "Uncommon", "AllEnemies", [DMG(5)] + HAVE("R"),
     [spend_all("R"),
      'await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(1 + x).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)\n            .WithHitFx("vfx/vfx_attack_slash")\n            .Execute(choiceContext);'],
     [up("Damage", 2)], "Spend ALL " + mtxt("R") + ".\nDeal {Damage:diff()} damage to ALL enemies, plus once more for each mana spent." + have_txt("R"), tips=[mtip("R")], color="R")
card("ChandraPyromaster", "Chandra, Pyromaster", 3, "Power", "Rare", "Self", [DV("Power", 4)],
     [POWERUP, apply_self("ChandraPower", "Power")], [up("Power", 2)],
     "At the start of your turn, deal {Power:diff()} damage to ALL enemies and gain 1 " + mtxt("R") + ".", tips=[tip_power("ChandraPower"), mtip("R")], color="R")
card("ShivanDragon", "Shivan Dragon", 3, "Skill", "Rare", "Self", [DV("Gain", 1)], summon("ShivanDragonOrb") + [gain("R")], [upcost(-1)],
     "[gold]Summon[/gold] Shivan Dragon.\n" + gain_txt("R"), tips=orbtips("ShivanDragonOrb") + [mtip("R")], color="R")

# ============================== BLUE: control ==============================
card("Ponder", "Ponder", 0, "Skill", "Common", "Self", ["new ScryVar(3m)", CARDS(1), DV("Gain", 1)],
     ["await ScryCmd.Execute(choiceContext, this);", DRAW, gain("U")], [up("Scry", 2)],
     "[gold]Scry[/gold] {Scry:diff()}.\nDraw {Cards:diff()} card.\n" + gain_txt("U"), tips=[mtip("U")], color="U")
card("Brainstorm", "Brainstorm", 1, "Skill", "Common", "Self", [CARDS(3), DV("Gain", 1)],
     [DRAW,
      "var toDiscard = await CardSelectCmd.FromHandForDiscard(choiceContext, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1), null, this);",
      "await CardCmd.Discard(choiceContext, toDiscard);", gain("U")],
     [upcost(-1)], "Draw {Cards:diff()} cards, then discard a card.\n" + gain_txt("U"),
     tips=[mtip("U")], extra={"selectionScreenPrompt": "Choose a card to discard."}, color="U")
card("Counterspell", "Counterspell", 1, "Skill", "Uncommon", "Self", [DV("Power", 1), DV("Gain", 1)], [apply_self("CounterspellPower", "Power"), gain("U")], [up("Power", 1)],
     "The next {Power:diff()} enemy {Power:plural:Attack|Attacks} against you this turn {Power:plural:deals|deal} no damage.\n" + gain_txt("U"),
     tips=[tip_power("CounterspellPower"), mtip("U")], color="U")
card("BlueSunsZenith", "Blue Sun's Zenith", 2, "Skill", "Uncommon", "Self", [CARDS(1)] + HAVE("U"),
     [spend_all("U"), "await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue + x, Owner);"], [up("Cards", 1)],
     "Spend ALL " + mtxt("U") + ".\nDraw {Cards:diff()} {Cards:plural:card|cards}, plus 1 for each mana spent." + have_txt("U"), tips=[mtip("U")], color="U")
card("JaceVrynsProdigy", "Jace, Vryn's Prodigy", 2, "Power", "Rare", "Self", [DV("Power", 1)],
     [POWERUP, apply_self("JacePower", "Power")], [upcost(-1)],
     "At the start of your turn, draw {Power:diff()} additional card and gain 1 " + mtxt("U") + ".", tips=[tip_power("JacePower"), mtip("U")], color="U")
card("TimeWarp", "Time Warp", 2, "Skill", "Rare", "Self", [EN(3), CARDS(4), DV("Need", 4)], [pay("U"), ENERGY, DRAW], [upcost(-1)],
     "Can only be played with {Need:diff()} " + mtxt("U") + ". Spend it.\nGain {Energy:energyIcons()}.\nDraw {Cards:diff()} cards.",
     keywords=["CardKeyword.Exhaust"], tips=[mtip("U"), ETIP, EXH], members=gated("U"), color="U")

# ============================== GREEN: creatures / midrange ================
card("LlanowarElves", "Llanowar Elves", 1, "Power", "Common", "Self", [DV("Power", 1)],
     [POWERUP, apply_self("LlanowarPower", "Power")], [up("Power", 1)],
     "At the start of your turn, gain {Power:diff()} " + mtxt("G") + ".", tips=[tip_power("LlanowarPower"), mtip("G")], color="G")
card("RampantGrowth", "Rampant Growth", 1, "Skill", "Common", "Self", [DV("Slots", 1), DV("Gain", 2)],
     ['await OrbCmd.AddSlots(Owner, DynamicVars["Slots"].IntValue);', gain("G")], [up("Gain", 1)],
     "Gain {Slots:diff()} creature slot.\n" + gain_txt("G"), tips=["HoverTipFactory.Static(StaticHoverTip.Channeling)", mtip("G")], color="G")
card("GrizzlyBears", "Grizzly Bears", 1, "Skill", "Common", "Self", [DV("Gain", 1)], summon("GrizzlyBearsOrb") + [gain("G")], [upcost(-1)],
     "[gold]Summon[/gold] Grizzly Bears.\n" + gain_txt("G"), tips=orbtips("GrizzlyBearsOrb") + [mtip("G")], color="G")
card("BirdsOfParadise", "Birds of Paradise", 1, "Skill", "Common", "Self", [DV("Gain", 1)], summon("BirdsOfParadiseOrb") + [gain("G")], [upcost(-1)],
     "[gold]Summon[/gold] Birds of Paradise.\n" + gain_txt("G"), tips=orbtips("BirdsOfParadiseOrb") + [mtip("G")], color="G")
card("GiantGrowth", "Giant Growth", 0, "Skill", "Common", "Self", [PV("StrengthPower", 3), DV("Spend", 1)],
     [apply_self("GiantGrowthPower", "StrengthPower"),
      'if (await Mana.Spend<GreenManaPower>(Owner.Creature, DynamicVars["Spend"].IntValue)) ' + apply_self("GiantGrowthPower", "StrengthPower")],
     [up("StrengthPower", 1)],
     "Gain {StrengthPower:diff()} [gold]Strength[/gold] until the end of this turn.\nIf you can, spend {Spend:diff()} " + mtxt("G") + " to gain it twice.",
     tips=[tip_power("StrengthPower"), mtip("G")], color="G")
card("Overrun", "Overrun", 2, "Skill", "Rare", "Self", [DV("Times", 1)] + HAVE("G"),
     [spend_all("G"),
      'int times = Math.Min(5, DynamicVars["Times"].IntValue + x);',
      'for (int i = 0; i < times; i++)',
      '    foreach (var orb in Owner.PlayerCombatState.OrbQueue.Orbs.ToList())',
      '        await OrbCmd.Passive(choiceContext, orb, null);'],
     [up("Times", 1)], "Spend ALL " + mtxt("G") + ".\nTrigger the [gold]Passive[/gold] of ALL your creatures {Times:diff()} {Times:plural:time|times}, plus once per mana spent (max 5)." + have_txt("G"),
     tips=[mtip("G")], color="G")
card("CraterhoofBehemoth", "Craterhoof Behemoth", 3, "Skill", "Rare", "Self", [DV("Need", 3), PV("StrengthPower", 2)],
     [pay("G"),
      'foreach (var orb in Owner.PlayerCombatState.OrbQueue.Orbs.ToList())',
      '{',
      '    await orb.Evoke(choiceContext);',
      '    ' + apply_self("StrengthPower"),
      '}'],
     [up("StrengthPower", 1)],
     "Can only be played with {Need:diff()} " + mtxt("G") + ". Spend it.\nALL your creatures [gold]Evoke[/gold] without leaving the battlefield. Gain {StrengthPower:diff()} [gold]Strength[/gold] for each.",
     keywords=["CardKeyword.Exhaust"], tips=[mtip("G"), "HoverTipFactory.Static(StaticHoverTip.Evoke)", tip_power("StrengthPower"), EXH], members=gated("G"), color="G")

# ============================== WHITE: lifegain / defense ==================
card("SavannahLions", "Savannah Lions", 1, "Skill", "Common", "Self", [DV("Gain", 1)], summon("SavannahLionsOrb") + [gain("W")], [upcost(-1)],
     "[gold]Summon[/gold] Savannah Lions.\n" + gain_txt("W"), tips=orbtips("SavannahLionsOrb") + [mtip("W")], color="W")
card("WallOfOmens", "Wall of Omens", 1, "Skill", "Common", "Self", [BLK(7), CARDS(1), DV("Gain", 1)], [BLOCK, DRAW, gain("W")], [up("Block", 3)],
     "Gain {Block:diff()} [gold]Block[/gold].\nDraw {Cards:diff()} card.\n" + gain_txt("W"), tips=[mtip("W")], color="W")
card("AjanisPridemate", "Ajani's Pridemate", 1, "Power", "Uncommon", "Self", [DV("Power", 1)],
     [POWERUP, apply_self("AjanisPridematePower", "Power")], [up("Power", 1)],
     "Whenever you heal HP, gain {Power:diff()} [gold]Strength[/gold].", tips=[tip_power("AjanisPridematePower"), tip_power("StrengthPower")], color="W")
card("SerraAngel", "Serra Angel", 2, "Skill", "Uncommon", "Self", [DV("Gain", 1)], summon("SerraAngelOrb") + [gain("W")], [upcost(-1)],
     "[gold]Summon[/gold] Serra Angel.\n" + gain_txt("W"), tips=orbtips("SerraAngelOrb") + [mtip("W")], color="W")
card("Comeuppance", "Comeuppance", 2, "Skill", "Rare", "Self", [DV("Power", 1), DV("Gain", 1)], [apply_self("ComeuppancePower", "Power"), gain("W")], [upcost(-1)],
     "Until your next turn, whenever an enemy attacks you, deal that much damage back to it.\n" + gain_txt("W"), tips=[tip_power("ComeuppancePower"), mtip("W")], color="W")
card("WrathOfGod", "Wrath of God", 3, "Attack", "Rare", "AllEnemies", [DMG(10), DV("Bonus", 5)] + HAVE("W"),
     [spend_all("W"),
      'await DamageCmd.Attack(DynamicVars.Damage.BaseValue + DynamicVars["Bonus"].BaseValue * x).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)\n            .WithHitFx("vfx/vfx_giant_horizontal_slash")\n            .Execute(choiceContext);'],
     [up("Damage", 4)], "Spend ALL " + mtxt("W") + ".\nDeal {Damage:diff()} damage to ALL enemies, plus {Bonus:diff()} for each mana spent." + have_txt("W"),
     keywords=["CardKeyword.Exhaust"], tips=[mtip("W"), EXH], color="W")

# ============================== BLACK: sacrifice / drain ===================
card("DarkRitual", "Dark Ritual", 0, "Skill", "Common", "Self", [DV("Gain", 3)], [gain("B")], [up("Gain", 1)],
     gain_txt("B"), keywords=["CardKeyword.Exhaust"], tips=[mtip("B"), EXH], color="B")
card("DoomBlade", "Doom Blade", 1, "Attack", "Uncommon", "AnyEnemy", [DMG(9), DV("Slay", 12), DV("Gain", 1)],
     [atk(SLASH),
      "var t = cardPlay.Target;",
      'if (!t.IsDead && t.CurrentHp <= DynamicVars["Slay"].IntValue)',
      "{",
      '    VfxCmd.PlayOnCreature(t, "vfx/vfx_bloody_impact");',
      "    await CreatureCmd.Damage(choiceContext, t, t.CurrentHp, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);",
      "}", gain("B")],
     [up("Damage", 3), up("Slay", 4)], "Deal {Damage:diff()} damage.\nIf the enemy then has {Slay:diff()} or less HP, slay it.\n" + gain_txt("B"), tips=[mtip("B")], color="B")
card("VampireNighthawk", "Vampire Nighthawk", 2, "Skill", "Uncommon", "Self", [DV("Gain", 1)], summon("VampireNighthawkOrb") + [gain("B")], [upcost(-1)],
     "[gold]Summon[/gold] Vampire Nighthawk.\n" + gain_txt("B"), tips=orbtips("VampireNighthawkOrb") + [mtip("B")], color="B")
card("AltarsReap", "Altar's Reap", 0, "Skill", "Common", "Self", [CARDS(2), DV("Gain", 2)],
     ["await OrbCmd.EvokeNext(choiceContext, Owner);", DRAW, gain("B")], [up("Cards", 1)],
     "[gold]Evoke[/gold] your first creature.\nDraw {Cards:diff()} cards.\n" + gain_txt("B"), tips=["HoverTipFactory.Static(StaticHoverTip.Evoke)", mtip("B")], color="B")
card("Reanimate", "Reanimate", 0, "Skill", "Uncommon", "Self", [HP(3)],
     ["var picked = (await CardSelectCmd.FromCombatPile(choiceContext, PileType.Exhaust.GetPile(Owner), Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1))).FirstOrDefault();",
      "if (picked == null) return;",
      "await CardPileCmd.Add(picked, PileType.Hand);", HPLOSS],
     [up("HpLoss", -2)], "Put a card from your [gold]Exhaust Pile[/gold] into your [gold]Hand[/gold].\nLose {HpLoss:diff()} HP.",
     keywords=["CardKeyword.Exhaust"], tips=[EXH], extra={"selectionScreenPrompt": "Choose a card to reanimate."}, color="B")
card("GrayMerchantOfAsphodel", "Gray Merchant of Asphodel", 3, "Attack", "Rare", "AllEnemies", [DMG(4), DV("Bonus", 3)] + HAVE("B"),
     [spend_all("B"),
      'decimal drain = DynamicVars.Damage.BaseValue + DynamicVars["Bonus"].BaseValue * x;',
      'await DamageCmd.Attack(drain).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)\n            .WithHitFx("vfx/vfx_bloody_impact")\n            .Execute(choiceContext);',
      'await CreatureCmd.Heal(Owner.Creature, drain);'],
     [up("Damage", 2), up("Bonus", 1)], "Spend ALL " + mtxt("B") + ".\nDeal {Damage:diff()} damage to ALL enemies, plus {Bonus:diff()} for each mana spent. Heal that much HP." + have_txt("B"),
     tips=[mtip("B")], color="B")
card("DarkConfidant", "Dark Confidant", 1, "Power", "Rare", "Self", [DV("Power", 1)],
     [POWERUP, apply_self("DarkConfidantPower", "Power")], [upcost(-1)],
     "At the start of your turn, draw {Power:diff()} additional card, lose HP equal to its cost and gain 1 " + mtxt("B") + ".", tips=[tip_power("DarkConfidantPower"), mtip("B")], color="B")

# ============================== MULTICOLOR =================================
card("LightningHelix", "Lightning Helix", 2, "Attack", "Uncommon", "AnyEnemy", [DMG(10), HL(3), DV("Gain", 1)], [atk(FIRE), HEAL, gain("R"), gain("W")],
     [up("Damage", 3), up("Heal", 1)], "Deal {Damage:diff()} damage.\nHeal {Heal:diff()} HP.\nGain {Gain:diff()} [red]Red[/red] and {Gain:diff()} " + mtxt("W") + ".",
     tips=[mtip("R"), mtip("W")], color="RW")
card("BlackLotus", "Black Lotus", 0, "Skill", "Rare", "Self", [DV("Gain", 2)], [gain(c) for c in "WUBRG"], [up("Gain", 1)],
     "Gain {Gain:diff()} mana of [gold]every[/gold] color.", keywords=["CardKeyword.Exhaust"], tips=[mtip(c) for c in "WUBRG"] + [EXH], color="C")

# --- emit ------------------------------------------------------------------
HEADER = """using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Cards.Variables;
using BaseLib.Commands;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Planeswalker.MtgSpireCode.Orbs;
using Planeswalker.MtgSpireCode.Powers;

namespace Planeswalker.MtgSpireCode.Cards;

"""
os.makedirs(OUT_CS, exist_ok=True)
loc = {}
written = []
for c in C:
    s = SCRY.get(c["mtg"], {})
    oracle = (s.get("oracle") or "").replace("\n", " ")
    doc = f'/// <summary>MTG: {s.get("name", c["mtg"])} ({s.get("set_name", "?")}), art by {s.get("artist", "?")}.<br/>Original text: {oracle}</summary>\n'
    lines = [HEADER, doc, f'public sealed class {c["cls"]} : PlaneswalkerCard\n{{\n']
    if c["hasx"]:
        lines.append("    protected override bool HasEnergyCostX => true;\n\n")
    if c["vars"]:
        lines.append("    protected override IEnumerable<DynamicVar> CanonicalVars =>\n    [\n        " + ",\n        ".join(c["vars"]) + "\n    ];\n\n")
    if c["keywords"]:
        lines.append("    public override IEnumerable<CardKeyword> CanonicalKeywords => [" + ", ".join(c["keywords"]) + "];\n\n")
    if c["tags"]:
        lines.append("    protected override HashSet<CardTag> CanonicalTags => [" + ", ".join(c["tags"]) + "];\n\n")
    if c["tips"]:
        lines.append("    protected override IEnumerable<IHoverTip> ExtraHoverTips => [" + ", ".join(c["tips"]) + "];\n\n")
    for m in c["members"]:
        lines.append("    " + m + "\n\n")
    lines.append(f'    public {c["cls"]}() : base({c["cost"]}, CardType.{c["type"]}, CardRarity.{c["rarity"]}, TargetType.{c["target"]})\n    {{\n    }}\n\n')
    lines.append("    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)\n    {\n        " + "\n        ".join(c["body"]) + "\n    }\n")
    if c["upgrade"]:
        lines.append("\n    protected override void OnUpgrade()\n    {\n        " + "\n        ".join(c["upgrade"]) + "\n    }\n")
    lines.append("}\n")
    path = os.path.join(OUT_CS, c["cls"] + ".cs")
    open(path, "w", encoding="utf-8", newline="\n").write("".join(lines))
    written.append(c["cls"])
    key = PREFIX + snake(c["cls"])
    loc[key + ".title"] = c["title"]
    loc[key + ".description"] = c["desc"]
    for k, v in c["extra"].items():
        loc[key + "." + k] = v

json.dump(dict(sorted(loc.items())), open(OUT_LOC, "w", encoding="utf-8"), indent=2, ensure_ascii=False)
# art file mapping for the image script
json.dump({c["cls"]: c["mtg"] for c in C}, open(os.path.join(os.path.dirname(__file__), "card_art_map.json"), "w"), indent=1)
print("cards:", len(written), "loc keys:", len(loc))
