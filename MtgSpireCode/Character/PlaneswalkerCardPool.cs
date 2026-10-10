using BaseLib.Abstracts;
using Godot;
using Planeswalker.MtgSpireCode.Extensions;

namespace Planeswalker.MtgSpireCode.Character;

public class PlaneswalkerCardPool : CustomCardPoolModel
{
    public override string Title => ThePlaneswalker.CharacterId; // internal name, not displayed

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    // HSV shift applied to the base card frame: pushes the red frame toward a warm gold.
    public override float H => 0.12f;
    public override float S => 0.85f;
    public override float V => 1.05f;

    public override Color DeckEntryCardColor => ThePlaneswalker.Color;
    public override bool IsColorless => false;
}
