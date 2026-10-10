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

/// <summary>MTG: Craterhoof Behemoth (Tarkir: Dragonstorm), art by Magali Villeneuve.<br/>Original text: Haste When this creature enters, creatures you control gain trample and get +X/+X until end of turn, where X is the number of creatures you control.</summary>
public sealed class CraterhoofBehemoth : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Need", 3m),
        new PowerVar<StrengthPower>(2m)
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<GreenManaPower>(), HoverTipFactory.Static(StaticHoverTip.Evoke), HoverTipFactory.FromPower<StrengthPower>(), HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    protected override bool IsPlayable => Owner?.Creature == null || Owner.Creature.GetPowerAmount<GreenManaPower>() >= DynamicVars["Need"].IntValue;

    public CraterhoofBehemoth() : base(3, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!await Mana.Spend<GreenManaPower>(Owner.Creature, DynamicVars["Need"].IntValue)) return;
        foreach (var orb in Owner.PlayerCombatState.OrbQueue.Orbs.ToList())
        {
            await orb.Evoke(choiceContext);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, DynamicVars["StrengthPower"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["StrengthPower"].UpgradeValueBy(1m);
    }
}
