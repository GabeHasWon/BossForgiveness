using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class DeerclopsHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.Deerclops;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 2 * 60 && !NPC.AnyNPCs(ModContent.NPCType<PacifiedDeerclops>());
    public override void OnPacify(NPC npc) => TransformInto<PacifiedDeerclops>(npc);
}
