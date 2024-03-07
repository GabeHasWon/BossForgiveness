using Terraria;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items;

internal class ChatItemIcon : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 0;

    public override void SetDefaults()
    {
        Item.Size = new(32, 26);
        Item.noUseGraphic = true;
    }
}
