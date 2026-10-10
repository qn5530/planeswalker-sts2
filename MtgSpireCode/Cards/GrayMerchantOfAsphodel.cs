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

/// <summary>MTG: Gray Merchant of Asphodel (Duskmourn: House of Horror Commander), art by Scott Murphy.<br/>Original text: When this creature enters, each opponent loses X life, where X is your devotion to black. You gain life equal to the life lost this way. (Each {B} in the mana costs of permanents you control counts toward your devotion to black.)</summary>
public sealed class GrayMerchantOfAsphodel : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(4m, ValueProp.Move),
        new DynamicVar("Bonus", 3m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Have").WithMultiplier((CardModel card, Creature? _) => card.Owner.Creature.GetPowerAmount<BlackManaPower>())
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<BlackManaPower>()];

    public GrayMerchantOfAsphodel() : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = await Mana.SpendAll<BlackManaPower>(Owner.Creature);
        decimal drain = DynamicVars.Damage.BaseValue + DynamicVars["Bonus"].BaseValue * x;
        await DamageCmd.Attack(drain).FromCard(this, cardPlay).TargetingAllOpponents(CombatState)
            .WithHitFx("vfx/vfx_bloody_impact")
            .Execute(choiceContext);
        await CreatureCmd.Heal(Owner.Creature, drain);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Damage"].UpgradeValueBy(2m);
        DynamicVars["Bonus"].UpgradeValueBy(1m);
    }
}
