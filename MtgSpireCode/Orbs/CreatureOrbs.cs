using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using Planeswalker.MtgSpireCode.Extensions;
using Planeswalker.MtgSpireCode.Powers;

namespace Planeswalker.MtgSpireCode.Orbs;

/// <summary>
/// A summoned creature. Creatures live in the Planeswalker's battlefield slots (the game's orb slots):
/// their Passive triggers at the end of every turn on its own, and Evoking one is "sacrificing" it for
/// a bigger one-shot effect. Visuals are a round portrait of the original MTG card art.
/// </summary>
public abstract class CreatureOrb : CustomOrbModel
{
    protected abstract decimal BasePassive { get; }
    protected abstract decimal BaseEvoke { get; }

    public override decimal PassiveVal => ModifyOrbValue(BasePassive);
    public override decimal EvokeVal => ModifyOrbValue(BaseEvoke);
    public override Color DarkenedColor => new("5a4a1c");

    private string FileName => Id.Entry.RemovePrefix().ToLowerInvariant();

    public override string CustomIconPath => $"orbs/{FileName}.png".ImagePath();
    // Points at a vanilla scene so the preloader is happy; the real sprite is built in CreateCustomSprite.
    public override string CustomSpritePath => SceneHelper.GetScenePath("orbs/orb_visuals/lightning_orb");
    public override string CustomPassiveSfx => "event:/sfx/characters/defect/defect_lightning_passive";
    public override string CustomEvokeSfx => "event:/sfx/characters/defect/defect_lightning_evoke";
    public override string CustomChannelSfx => "event:/sfx/characters/defect/defect_lightning_channel";

    public override Node2D CreateCustomSprite()
    {
        var root = new Node2D();
        // NOrb expects a "SpineSkeleton" child to drive; borrow the vanilla one and keep it invisible.
        var vanilla = ResourceLoader.Load<PackedScene>(CustomSpritePath)?.Instantiate<Node2D>();
        var spine = vanilla?.GetNodeOrNull<Node>("SpineSkeleton");
        if (vanilla != null && spine != null)
        {
            vanilla.RemoveChild(spine);
            root.AddChild(spine);
            if (spine is CanvasItem ci) ci.Visible = false;
        }
        vanilla?.QueueFree();

        var path = $"orbs/sprite/{FileName}.png".ImagePath();
        if (ResourceLoader.Exists(path))
        {
            root.AddChild(new Sprite2D { Texture = ResourceLoader.Load<Texture2D>(path) });
        }
        return root;
    }

    public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await TriggerPassive(choiceContext, null);
    }

    protected Creature? RandomEnemy()
    {
        var enemies = CombatState.HittableEnemies;
        return enemies.Count == 0 ? null : Owner.RunState.Rng.CombatTargets.NextItem(enemies);
    }

    /// <summary>Creature combat damage: hits one random enemy (or all enemies) for the given amount.</summary>
    protected async Task<IEnumerable<Creature>> Strike(PlayerChoiceContext choiceContext, decimal amount, Creature? target, bool evoke, bool allEnemies = false)
    {
        List<Creature> targets = allEnemies
            ? CombatState.HittableEnemies.ToList()
            : target != null ? [target] : RandomEnemy() is { } e ? [e] : [];
        if (targets.Count == 0) return targets;
        if (evoke) ActivateEvoke(targets.ToArray()); else ActivatePassive();
        foreach (var t in targets) VfxCmd.PlayOnCreature(t, "vfx/vfx_attack_slash");
        if (evoke) PlayEvokeSfx(); else PlayPassiveSfx();
        await CreatureCmd.Damage(choiceContext, targets, amount, ValueProp.Unpowered, Owner.Creature);
        return targets;
    }
}

/// <summary>Savannah Lions (White, 2/1): passive 2 damage to a random enemy, evoke 5.</summary>
public sealed class SavannahLionsOrb : CreatureOrb
{
    protected override decimal BasePassive => 2m;
    protected override decimal BaseEvoke => 5m;
    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target) => await Strike(choiceContext, PassiveVal, target, false);
    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext) => await Strike(choiceContext, EvokeVal, null, true);
}

/// <summary>Grizzly Bears (Green, 2/2): passive 3 damage to a random enemy, evoke 7.</summary>
public sealed class GrizzlyBearsOrb : CreatureOrb
{
    protected override decimal BasePassive => 3m;
    protected override decimal BaseEvoke => 7m;
    public override Color DarkenedColor => new("1f5a2e");
    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target) => await Strike(choiceContext, PassiveVal, target, false);
    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext) => await Strike(choiceContext, EvokeVal, null, true);
}

/// <summary>Shivan Dragon (Red, 5/5 flying): passive 5 damage to ALL enemies, evoke 12 to ALL.</summary>
public sealed class ShivanDragonOrb : CreatureOrb
{
    protected override decimal BasePassive => 5m;
    protected override decimal BaseEvoke => 12m;
    public override Color DarkenedColor => new("7a1d10");
    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target) => await Strike(choiceContext, PassiveVal, null, false, allEnemies: true);
    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext) => await Strike(choiceContext, EvokeVal, null, true, allEnemies: true);
}

/// <summary>Vampire Nighthawk (Black, lifelink): passive 2 damage and heal 2, evoke 6 damage and heal 6.</summary>
public sealed class VampireNighthawkOrb : CreatureOrb
{
    protected override decimal BasePassive => 2m;
    protected override decimal BaseEvoke => 6m;
    public override Color DarkenedColor => new("2b1a3a");

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        var hit = await Strike(choiceContext, PassiveVal, target, false);
        if (hit.Any()) await CreatureCmd.Heal(Owner.Creature, PassiveVal);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        var hit = await Strike(choiceContext, EvokeVal, null, true);
        if (hit.Any()) await CreatureCmd.Heal(Owner.Creature, EvokeVal);
        return hit;
    }
}

/// <summary>Serra Angel (White, 4/4 flying vigilance): passive 3 damage and 3 Block, evoke 8 damage and 8 Block.</summary>
public sealed class SerraAngelOrb : CreatureOrb
{
    protected override decimal BasePassive => 3m;
    protected override decimal BaseEvoke => 8m;
    public override Color DarkenedColor => new("6a5a2a");
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.Static(StaticHoverTip.Block)];

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        await Strike(choiceContext, PassiveVal, target, false);
        await CreatureCmd.GainBlock(Owner.Creature, PassiveVal, ValueProp.Unpowered, null);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        var hit = await Strike(choiceContext, EvokeVal, null, true);
        await CreatureCmd.GainBlock(Owner.Creature, EvokeVal, ValueProp.Unpowered, null);
        return hit;
    }
}

/// <summary>Birds of Paradise (Green mana dork): passive gain 1 Green mana, evoke gain 2 Energy.</summary>
public sealed class BirdsOfParadiseOrb : CreatureOrb
{
    protected override decimal BasePassive => 1m;
    protected override decimal BaseEvoke => 2m;
    public override Color DarkenedColor => new("1f5a2e");
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<GreenManaPower>()];

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        ActivatePassive();
        PlayPassiveSfx();
        await Mana.Gain<GreenManaPower>(choiceContext, Owner.Creature, (int)PassiveVal, null);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        PlayEvokeSfx();
        ActivateEvoke([Owner.Creature]);
        await PlayerCmd.GainEnergy(EvokeVal, Owner);
        return [Owner.Creature];
    }
}
