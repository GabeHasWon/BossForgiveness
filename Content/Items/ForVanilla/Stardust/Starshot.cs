using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal class Starshot : StardustItem
{
    internal override int PlaceStyle => 1;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.Size = new Vector2(24, 18);
        Item.value = 0;
    }
}
