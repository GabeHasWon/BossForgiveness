using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class QueenSlimeHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.QueenSlimeBoss;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 60 * 60 && npc.life > npc.lifeMax / 2 
        && !NPC.AnyNPCs(ModContent.NPCType<QueenSlimePacified>());
    public override void OnPacify(NPC npc) => TransformInto<QueenSlimePacified>(npc);
}
