using BossForgiveness.Content.NPCs.Mechanics;
using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class EoCHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.EyeofCthulhu;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 0.25f * 60 * 60 && npc.GetGlobalNPC<EoCPacificationNPC>().IsContent
        && npc.ai[0] <= 2 && !NPC.AnyNPCs(ModContent.NPCType<EyePacified>());

    public override void OnPacify(NPC npc) => TransformInto<EyePacified>(npc);
}
