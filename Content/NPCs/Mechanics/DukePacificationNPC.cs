using BossForgiveness.Content.NPCs.Misc;
using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class DukePacificationNPC : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.DukeFishron;

    public override bool PreAI(NPC npc)
    {
        if (Omnileech.Self > -1)
        {
            npc.localAI[3]++;
            npc.velocity = npc.DirectionTo(Main.npc[Omnileech.Self].Center) * npc.localAI[3] * 0.2f;
            npc.rotation = npc.velocity.ToRotation();

            if (npc.velocity.X < 0)
            {
                npc.spriteDirection = 1;
                npc.rotation -= MathHelper.Pi;
            }
            return false;
        }
        else
            npc.localAI[3] = 2;

        return true;
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (target.type == ModContent.NPCType<Omnileech>() && target.life <= 0 && npc.life == npc.lifeMax)
            npc.Transform(ModContent.NPCType<DukePacified>());
    }
}
