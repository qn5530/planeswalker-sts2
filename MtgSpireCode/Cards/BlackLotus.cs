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

/// <summary>MTG: Black Lotus (Vintage Masters), art by Chris Rahn.<br/>Original text: {T}, Sacrifice this artifact: Add three mana of any one color.</summary>
public sealed class BlackLotus : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Gain", 2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WhiteManaPower>(), HoverTipFactory.FromPower<BlueManaPower>(), HoverTipFactory.FromPower<BlackManaPower>(), HoverTipFactory.FromPower<RedManaPower>(), HoverTipFactory.FromPower<GreenManaPower>(), HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public BlackLotus() : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Mana.Gain<WhiteManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
        await Mana.Gain<BlueManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
        await Mana.Gain<BlackManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
        await Mana.Gain<RedManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
        await Mana.Gain<GreenManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Gain"].UpgradeValueBy(1m);
    }
}
