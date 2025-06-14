using BossForgiveness.Content.NPCs.Mechanics.Lunar.Nebula;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class Telelink : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(32, 26);
        Item.value = 0;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Lime;
    }

    public override void HoldItem(Player player) => player.GetModPlayer<NebulaLinkPlayer>().canLink = true;
}
