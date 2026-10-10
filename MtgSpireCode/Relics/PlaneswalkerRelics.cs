using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Powers;
using Planeswalker.MtgSpireCode.Powers;

namespace Planeswalker.MtgSpireCode.Relics;

/// <summary>Sol Ring (starter): at the start of each combat, gain 2 Energy.</summary>
public sealed class SolRing : PlaneswalkerRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar(2)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.ForEnergy(this)];

    // AfterSideTurnStart runs after the energy reset, so the bonus survives (same hook as Booming Conch).
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
    }
}

/// <summary>Chromatic Lantern (common): at the start of each combat, gain 1 mana of every color.</summary>
public sealed class ChromaticLantern : PlaneswalkerRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Mana", 1m)];
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        HoverTipFactory.FromPower<WhiteManaPower>(), HoverTipFactory.FromPower<BlueManaPower>(), HoverTipFactory.FromPower<BlackManaPower>(),
        HoverTipFactory.FromPower<RedManaPower>(), HoverTipFactory.FromPower<GreenManaPower>()
    ];

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner.Creature) || Owner.PlayerCombatState.TurnNumber > 1) return;
        Flash();
        var ctx = new ThrowingPlayerChoiceContext();
        int n = DynamicVars["Mana"].IntValue;
        await Mana.Gain<WhiteManaPower>(ctx, Owner.Creature, n, null);
        await Mana.Gain<BlueManaPower>(ctx, Owner.Creature, n, null);
        await Mana.Gain<BlackManaPower>(ctx, Owner.Creature, n, null);
        await Mana.Gain<RedManaPower>(ctx, Owner.Creature, n, null);
        await Mana.Gain<GreenManaPower>(ctx, Owner.Creature, n, null);
    }
}

/// <summary>Sensei's Divining Top (uncommon): at the start of your turn, Scry 2.</summary>
public sealed class SenseisDivingTop : PlaneswalkerRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Scry", 2m)];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner) return;
        Flash();
        await ScryCmd.Execute(choiceContext, Owner, DynamicVars["Scry"].IntValue);
    }
}
