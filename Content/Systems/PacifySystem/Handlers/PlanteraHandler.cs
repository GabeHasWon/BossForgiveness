using BossForgiveness.Content.NPCs.Mechanics.Plantera;
using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class PlanteraHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.Plantera;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PlanteraPacificationNPC>().pacification >= PlanteraPacificationNPC.MaxPacificationsNeeded;

    public override void OnPacify(NPC npc)
    {
        TransformInto<PlanteraPacified>(npc);

        for (int i = 0; i < 20; ++i)
        {
            Vector2 vel = Main.rand.NextVector2Circular(12, 12);
            Dust.NewDust(npc.Center, 1, 1, DustID.HealingPlus, vel.X, vel.Y);
        }
    }
}
