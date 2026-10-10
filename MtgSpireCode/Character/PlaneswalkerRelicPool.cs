using BaseLib.Abstracts;
using Godot;
using Planeswalker.MtgSpireCode.Extensions;

namespace Planeswalker.MtgSpireCode.Character;

public class PlaneswalkerRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => ThePlaneswalker.Color;
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
