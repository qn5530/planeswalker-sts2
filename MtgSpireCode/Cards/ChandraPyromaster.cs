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

/// <summary>MTG: Chandra, Pyromaster (Archenemy: Nicol Bolas), art by Chris Rahn.<br/>Original text: +1: Chandra deals 1 damage to target player or planeswalker and 1 damage to up to one target creature that player or that planeswalker's controller controls. That creature can't block this turn. 0: Exile the top card of your library. You may play it this turn. −7: Exile the top ten cards of your library. Choose an instant or sorcery card exiled this way and copy it three times. You may cast the copies without paying their mana costs.</summary>
public sealed class ChandraPyromaster : PlaneswalkerCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DynamicVar("Power", 4m)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => [HoverTipFactory.FromPower<ChandraPower>(), HoverTipFactory.FromPower<RedManaPower>()];

    public ChandraPyromaster() : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(Owner.Creature, "PowerUp", Owner.Character.PowerUpAnimDelay);
        await PowerCmd.Apply<ChandraPower>(choiceContext, Owner.Creature, DynamicVars["Power"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Power"].UpgradeValueBy(2m);
    }
}
