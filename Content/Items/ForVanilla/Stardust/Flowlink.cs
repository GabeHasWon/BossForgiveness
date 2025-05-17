using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal class Flowlink : StardustItem
{
    internal override int PlaceStyle => 4;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.Size = new Vector2(38, 30);
        Item.value = 0;
    }
}
