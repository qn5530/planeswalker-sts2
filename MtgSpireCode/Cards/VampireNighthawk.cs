using System;
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

/// <summary>MTG: Vampire Nighthawk (Foundations), art by Jason Chan.<br/>Original text: Flying Deathtouch (Any amount of damage this deals to a creature is enough to destroy it.) Lifelink (Damage dealt by this creature also causes you to gain that much life.)</summary>
public sealed class VampireNighthawk : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Gain", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<VampireNighthawkOrb>(), HoverTipFactory.Static(StaticHoverTip.Channeling), HoverTipFactory.FromPower<BlackManaPower>()];

    public VampireNighthawk() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OrbCmd.Channel<VampireNighthawkOrb>(choiceContext, Owner);
        await Mana.Gain<BlackManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
