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

/// <summary>MTG: Shivan Dragon (Foundations), art by Donato Giancola.<br/>Original text: Flying {R}: This creature gets +1/+0 until end of turn.</summary>
public sealed class ShivanDragon : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Gain", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<ShivanDragonOrb>(), HoverTipFactory.Static(StaticHoverTip.Channeling), HoverTipFactory.FromPower<RedManaPower>()];

    public ShivanDragon() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OrbCmd.Channel<ShivanDragonOrb>(choiceContext, Owner);
        await Mana.Gain<RedManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
