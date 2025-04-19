using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal class Tracemap : StardustItem
{
    public override void SetDefaults()
    {
        Item.Size = new Vector2(36, 30);
        Item.value = 0;
    }
}
