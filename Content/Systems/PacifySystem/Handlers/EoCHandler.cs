using BossForgiveness.Content.NPCs;
using BossForgiveness.Content.NPCs.Mechanics;
using BossForgiveness.Content.NPCs.Vanilla;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class EoCHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.EyeofCthulhu;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<EoCPacificationNPC>().IsContent && npc.ai[0] <= 2 && !NPC.AnyNPCs(ModContent.NPCType<EyePacified>());

    public override void OnPacify(NPC npc) => npc.Pacify<EyePacified>();
}
