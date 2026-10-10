using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Planeswalker.MtgSpireCode.Cards;

namespace Planeswalker.MtgSpireCode.Powers;

/// <summary>Chandra, Pyromaster: at the start of your turn, deal Amount damage to ALL enemies and gain 1 Red mana.</summary>
public sealed class ChandraPower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RedManaPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player || CombatState == null) return;
        Flash();
        await CreatureCmd.Damage(choiceContext, CombatState.HittableEnemies, Amount, ValueProp.Unpowered, Owner);
        await Mana.Gain<RedManaPower>(choiceContext, Owner, 1, null);
    }
}

/// <summary>Jace, Vryn's Prodigy: at the start of your turn, draw Amount additional cards and gain 1 Blue mana.</summary>
public sealed class JacePower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlueManaPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await CardPileCmd.Draw(choiceContext, Amount, player);
        await Mana.Gain<BlueManaPower>(choiceContext, Owner, 1, null);
    }
}

/// <summary>Llanowar Elves: at the start of your turn, gain Amount Green mana.</summary>
public sealed class LlanowarPower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<GreenManaPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        await Mana.Gain<GreenManaPower>(choiceContext, Owner, Amount, null);
    }
}

/// <summary>Dark Confidant: at the start of your turn, draw Amount cards, lose HP equal to their cost and gain 1 Black mana.</summary>
public sealed class DarkConfidantPower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlackManaPower>()];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player) return;
        Flash();
        var drawn = (await CardPileCmd.Draw(choiceContext, Amount, player)).ToList();
        int cost = drawn.Sum(c => c.EnergyCost.CostsX ? 0 : System.Math.Max(0, c.EnergyCost.GetResolved()));
        if (cost > 0)
        {
            VfxCmd.PlayOnCreatureCenter(Owner, "vfx/vfx_bloody_impact");
            await CreatureCmd.Damage(choiceContext, Owner, cost, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
        }
        await Mana.Gain<BlackManaPower>(choiceContext, Owner, 1, null);
    }
}

/// <summary>Ajani's Pridemate: whenever you heal HP, gain Amount Strength.</summary>
public sealed class AjanisPridematePower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<StrengthPower>()];

    public override async Task AfterCurrentHpChanged(Creature creature, decimal delta)
    {
        if (creature != Owner || delta <= 0) return;
        Flash();
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, Amount, Owner, null);
    }
}

/// <summary>Giant Growth: temporary Strength, removed at the end of the turn (same plumbing as the Flex potion).</summary>
public class GiantGrowthPower : TemporaryStrengthPower, ICustomPower
{
    public override AbstractModel OriginModel => ModelDb.Card<GiantGrowth>();
    public string? CustomPackedIconPath => ModelDb.Power<StrengthPower>().PackedIconPath;
    public string? CustomBigIconPath => ModelDb.Power<StrengthPower>().ResolvedBigIconPath;
}
