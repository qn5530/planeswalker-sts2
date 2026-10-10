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

/// <summary>MTG: Wrath of God (Commander Masters), art by Willian Murai.<br/>Original text: Destroy all creatures. They can't be regenerated.</summary>
public sealed class WrathOfGod : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move),
        new DynamicVar("Bonus", 5m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Have").WithMultiplier((CardModel card, Creature? _) => card.Owner.Creature.GetPowerAmount<WhiteManaPower>())
    ];

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<WhiteManaPower>(), HoverTipFactory.FromKeyword(CardKeyword.Exhaust)];

    public WrathOfGod() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = await Mana.SpendAll<WhiteManaPower>(Owner.Creature);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue + DynamicVars["Bonus"].BaseValue * x).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_giant_horizontal_slash")
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(4m);
    }
}
