using BossForgiveness.Content.NPCs.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class OmnileechItem : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(36, 44);
        Item.value = 0;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Lime;
        Item.useTime = Item.useAnimation = 30;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.consumable = true;
        Item.noUseGraphic = true;
    }

    public override bool? UseItem(Player player)
    {
        if (Main.netMode == NetmodeID.Server)
        {
            int npc = NPC.NewNPC(player.GetSource_ItemUse(Item), (int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<Omnileech>());
            NPC n = Main.npc[npc];
            n.velocity = new Microsoft.Xna.Framework.Vector2(0, -6);
        }

        return true;
    }

    public override void AddRecipes()
    {
        CreateRecipe()
            .AddIngredient<TruffleLeech>(20)
            .Register();
    }
}
