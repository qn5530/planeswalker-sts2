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

/// <summary>MTG: Savannah Lions (Foundations), art by Winona Nelson.<br/>Original text: </summary>
public sealed class SavannahLions : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Gain", 1m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromOrb<SavannahLionsOrb>(), HoverTipFactory.Static(StaticHoverTip.Channeling), HoverTipFactory.FromPower<WhiteManaPower>()];

    public SavannahLions() : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await OrbCmd.Channel<SavannahLionsOrb>(choiceContext, Owner);
        await Mana.Gain<WhiteManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
