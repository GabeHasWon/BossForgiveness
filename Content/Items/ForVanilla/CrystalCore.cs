using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class CrystalCore : ModItem
{
    public override string Texture => "Terraria/Images/Extra_" + ExtrasID.QueenSlimeCrystalCore;

    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 0;

    public override void SetDefaults()
    {
        Item.Size = new(32, 26);
        Item.noUseGraphic = true;
    }
}