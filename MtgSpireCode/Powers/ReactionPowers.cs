using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Planeswalker.MtgSpireCode.Powers;

/// <summary>Base for "instant" effects: a buff that reacts when an enemy attacks you, then fades at your next turn.</summary>
public abstract class ReactionPower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;

    protected bool IsAttackOnMe(Creature target, ValueProp props, Creature? dealer) =>
        target == Owner && dealer != null && dealer != Owner && props.IsPoweredAttack();

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player == Owner.Player) await PowerCmd.Remove(this);
    }
}

/// <summary>Counterspell: the next Amount enemy Attacks deal no damage to you.</summary>
public sealed class CounterspellPower : ReactionPower
{
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        return Amount > 0 && target == Owner && dealer != Owner && props.IsPoweredAttack() ? 0m : amount;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!IsAttackOnMe(target, props, dealer)) return;
        Flash();
        if (Amount <= 1) await PowerCmd.Remove(this);
        else await PowerCmd.Decrement(this);
    }
}

/// <summary>Comeuppance: until your next turn, whenever an enemy attacks you, deal that much damage back to it.</summary>
public sealed class ComeuppancePower : ReactionPower
{
    public override PowerStackType StackType => PowerStackType.Single;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (!IsAttackOnMe(target, props, dealer) || result.TotalDamage <= 0) return;
        Flash();
        VfxCmd.PlayOnCreature(dealer!, "vfx/vfx_attack_slash");
        await CreatureCmd.Damage(choiceContext, dealer!, result.TotalDamage, ValueProp.Unpowered, Owner);
    }
}
