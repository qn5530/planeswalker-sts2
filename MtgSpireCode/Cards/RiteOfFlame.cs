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

/// <summary>MTG: Rite of Flame (Coldsnap), art by Dany Orizio.<br/>Original text: Add {R}{R}, then add {R} for each card named Rite of Flame in each graveyard.</summary>
public sealed class RiteOfFlame : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Gain", 2m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<RedManaPower>()];

    public RiteOfFlame() : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await Mana.Gain<RedManaPower>(choiceContext, Owner.Creature, DynamicVars["Gain"].IntValue, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Gain"].UpgradeValueBy(1m);
    }
}
