using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class TruffleLeech : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(22);
        Item.value = 0;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Lime;
    }
}
