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

/// <summary>MTG: Time Warp (Marvel Super Heroes Commander), art by Gintas Galvanauskas.<br/>Original text: Target player takes an extra turn after this one.</summary>
public sealed class TimeWarp : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new EnergyVar(3),
        new CardsVar(4),
        new DynamicVar("Need", 4m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlueManaPower>(), EnergyHoverTip, HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override bool IsPlayable => Owner?.Creature == null || Owner.Creature.GetPowerAmount<BlueManaPower>() >= DynamicVars["Need"].IntValue;

    public TimeWarp() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!await Mana.Spend<BlueManaPower>(Owner.Creature, DynamicVars["Need"].IntValue)) return;
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue, Owner);
        await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
