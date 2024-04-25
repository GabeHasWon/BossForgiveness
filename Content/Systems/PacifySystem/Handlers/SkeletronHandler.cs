using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class SkeletronHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.SkeletronHead;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 1222 * 60 * 60 
        && !NPC.AnyNPCs(ModContent.NPCType<SkeletronPacified>()) && (!Main.dayTime || Main.remixWorld);
    public override void OnPacify(NPC npc) => TransformInto<SkeletronPacified>(npc);
}
