using BossForgiveness.Content.NPCs.Mechanics.Plantera;
using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class PlanteraHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.Plantera;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PlanteraPacificationNPC>().pacification > 2;
    public override void OnPacify(NPC npc) => TransformInto<PlanteraPacified>(npc);
}
