using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Planeswalker.MtgSpireCode.Character;
using Planeswalker.MtgSpireCode.Extensions;

namespace Planeswalker.MtgSpireCode.Relics;

/// <summary>Base class for the mod's relics; icons load from Planeswalker/images/relics/{big/}{id}.png.</summary>
[Pool(typeof(PlaneswalkerRelicPool))]
public abstract class PlaneswalkerRelic : CustomRelicModel
{
    private string FileName => Id.Entry.RemovePrefix().ToLowerInvariant();

    public override string PackedIconPath => $"{FileName}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{FileName}_outline.png".RelicImagePath();
    protected override string BigIconPath => $"{FileName}.png".BigRelicImagePath();
}
