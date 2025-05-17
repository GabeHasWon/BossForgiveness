using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal class ClaspCell : StardustItem
{
    internal override int PlaceStyle => 3;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.Size = new Vector2(32, 18);
        Item.value = 0;
    }
}
