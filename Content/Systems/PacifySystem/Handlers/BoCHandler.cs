using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class BoCHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.BrainofCthulhu;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 1.5f * 2 * 2;

    public override void OnPacify(NPC npc)
    {
        TransformInto<BoCPacified>(npc);
        (npc.ModNPC as BoCPacified).BuildCreepers();
    }
}
