using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using BossForgiveness.Content.Items.ForVanilla;

namespace BossForgiveness.Content.Systems.WorldGeneration;

internal class ChestSystem : ModSystem
{
    public override void PostWorldGen()
    {
        for (int chestIndex = 0; chestIndex < Main.maxChests; chestIndex++)
        {
            Chest chest = Main.chest[chestIndex];
            
            if (chest == null)
                continue;
            
            Tile chestTile = Main.tile[chest.x, chest.y];

            if (chestTile.TileType != TileID.Containers || chestTile.TileFrameX != 16 * 36)
                continue;

            for (int i = 0; i < Chest.maxItems; i++)
            {
                if (chest.item[i].type == ItemID.None)
                {
                    chest.item[i].SetDefaults(ModContent.ItemType<GolemTaser>());
                    return;
                }
            }
        }
    }
}
