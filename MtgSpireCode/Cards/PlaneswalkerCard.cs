using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Cards;
using Planeswalker.MtgSpireCode.Character;
using Planeswalker.MtgSpireCode.Extensions;

namespace Planeswalker.MtgSpireCode.Cards;

/// <summary>
/// Base class for every Planeswalker card. Portraits are loaded from
/// Planeswalker/images/card_portraits/{big/}{snake_case_id}.png (1000x760 big, 250x190 small).
/// Hand-written: the generator (tools/gen_cards.py) only emits the concrete card classes.
/// </summary>
[Pool(typeof(PlaneswalkerCardPool))]
public abstract class PlaneswalkerCard(int cost, CardType type, CardRarity rarity, TargetType target)
    : CustomCardModel(cost, type, rarity, target)
{
    private string FileName => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png";

    public override string CustomPortraitPath => FileName.BigCardImagePath();
    public override string PortraitPath => FileName.CardImagePath();
    public override string BetaPortraitPath => $"beta/{FileName}".CardImagePath();
}
