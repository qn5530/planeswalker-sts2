using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using Planeswalker.MtgSpireCode.Extensions;

namespace Planeswalker.MtgSpireCode.Powers;

/// <summary>Base class for the mod's powers; icons load from Planeswalker/images/powers/{big/}{id}.png.</summary>
public abstract class PlaneswalkerPower : CustomPowerModel
{
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();

    public abstract override PowerType Type { get; }
    public abstract override PowerStackType StackType { get; }
}
