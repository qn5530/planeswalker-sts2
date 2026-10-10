using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Planeswalker.MtgSpireCode.Powers;

/// <summary>
/// Colored mana. Each color is a stacking buff on the Planeswalker that lasts for the whole combat.
/// "Gain" cards add mana of their color; "Spend" cards consume it for bigger effects; one finisher per
/// color cannot be played at all without enough mana of its color.
/// </summary>
public abstract class ManaPower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
}

public sealed class WhiteManaPower : ManaPower { }
public sealed class BlueManaPower : ManaPower { }
public sealed class BlackManaPower : ManaPower { }
public sealed class RedManaPower : ManaPower { }
public sealed class GreenManaPower : ManaPower { }

/// <summary>Kiln Fiend: whenever you gain Red mana, deal Amount damage to a random enemy for each mana gained.</summary>
public sealed class KilnFiendPower : PlaneswalkerPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public async Task OnRedManaGained(PlayerChoiceContext choiceContext, int gained)
    {
        if (CombatState == null) return;
        for (int i = 0; i < gained; i++)
        {
            var enemies = CombatState.HittableEnemies;
            if (enemies.Count == 0) return;
            Flash();
            var target = Owner.Player.RunState.Rng.CombatTargets.NextItem(enemies);
            VfxCmd.PlayOnCreature(target, "vfx/vfx_attack_slash");
            await CreatureCmd.Damage(choiceContext, target, Amount, ValueProp.Unpowered, Owner);
        }
    }
}

/// <summary>All mana bookkeeping goes through here so that "whenever you gain mana" triggers fire.</summary>
public static class Mana
{
    public static int Of<T>(Creature owner) where T : ManaPower => owner.GetPowerAmount<T>();

    public static async Task Gain<T>(PlayerChoiceContext choiceContext, Creature owner, int amount, CardModel? source) where T : ManaPower
    {
        if (amount <= 0) return;
        await PowerCmd.Apply<T>(choiceContext, owner, amount, owner, source);
        if (typeof(T) == typeof(RedManaPower) && owner.GetPower<KilnFiendPower>() is { } fiend)
        {
            await fiend.OnRedManaGained(choiceContext, amount);
        }
    }

    /// <summary>Removes every point of one color and returns how many there were.</summary>
    public static async Task<int> SpendAll<T>(Creature owner) where T : ManaPower
    {
        int n = Of<T>(owner);
        if (n > 0) await PowerCmd.Remove<T>(owner);
        return n;
    }

    /// <summary>Spends exactly the given amount if available; returns false (and spends nothing) otherwise.</summary>
    public static async Task<bool> Spend<T>(Creature owner, int amount) where T : ManaPower
    {
        var power = owner.GetPower<T>();
        if (power == null || power.Amount < amount) return false;
        if (power.Amount == amount) await PowerCmd.Remove(power);
        else for (int i = 0; i < amount; i++) await PowerCmd.Decrement(power);
        return true;
    }
}
