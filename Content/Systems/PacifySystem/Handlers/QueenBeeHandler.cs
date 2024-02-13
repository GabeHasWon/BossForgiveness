using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class QueenBeeHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.QueenBee;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 0.75f * 60 * 60 && !NPC.AnyNPCs(ModContent.NPCType<PacifiedQueenBee>());
    public override void OnPacify(NPC npc) => TransformInto<PacifiedQueenBee>(npc);
}
