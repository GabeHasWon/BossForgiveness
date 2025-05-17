using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal class SkywardAntennae : StardustItem
{
    internal override int PlaceStyle => 2;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.Size = new Vector2(14, 34);
        Item.value = 0;
    }
}
