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

/// <summary>MTG: Overrun (Foundations), art by Carl Critchlow.<br/>Original text: Creatures you control get +3/+3 and gain trample until end of turn. (Each of those creatures can deal excess combat damage to the player or planeswalker it's attacking.)</summary>
public sealed class Overrun : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Times", 1m),
        new CalculationBaseVar(0m),
        new CalculationExtraVar(1m),
        new CalculatedVar("Have").WithMultiplier((CardModel card, Creature? _) => card.Owner.Creature.GetPowerAmount<GreenManaPower>())
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<GreenManaPower>()];

    public Overrun() : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int x = await Mana.SpendAll<GreenManaPower>(Owner.Creature);
        int times = Math.Min(5, DynamicVars["Times"].IntValue + x);
        for (int i = 0; i < times; i++)
            foreach (var orb in Owner.PlayerCombatState.OrbQueue.Orbs.ToList())
                await OrbCmd.Passive(choiceContext, orb, null);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Times"].UpgradeValueBy(1m);
    }
}
