using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class PlanteraHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.Plantera;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 60 * 60 
        && !NPC.AnyNPCs(ModContent.NPCType<PlanteraPacified>()) && npc.Center.Y > Main.worldSurface * 16 && npc.life > npc.lifeMax / 2f;
    public override void OnPacify(NPC npc) => TransformInto<PlanteraPacified>(npc);
}
