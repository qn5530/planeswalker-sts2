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

/// <summary>MTG: Blue Sun's Zenith (Masters 25), art by Izzy.<br/>Original text: Target player draws X cards. Shuffle Blue Sun's Zenith into its owner's library.</summary>
public sealed class BlueSunsZenith : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new CardsVar(1),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Have").WithMultiplier((CardModel card, Creature? _) => card.Owner.Creature.GetPowerAmount<BlueManaPower>())
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlueManaPower>()];

    public BlueSunsZenith() : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = await Mana.SpendAll<BlueManaPower>(Owner.Creature);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue + x, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
