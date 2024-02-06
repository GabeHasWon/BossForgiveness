using BossForgiveness.Content.Items.ForVanilla;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs;

internal class Loot : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.EyeofCthulhu)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EoCLeash>()));
    }
}
