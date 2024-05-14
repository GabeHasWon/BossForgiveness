using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.BoC;

internal class CreeperPacificationNPC : GlobalNPC
{
    private const int MaxRage = 4;

    public override bool InstancePerEntity => true;

    public int rage = 0;

    public override bool PreAI(NPC npc)
    {
        if (NPC.crimsonBoss == -1)
            return true;

        NPC parent = Main.npc[NPC.crimsonBoss];
        bool sleepyParent = parent.active && parent.type == NPCID.BrainofCthulhu && parent.GetGlobalNPC<BoCPacificationNPC>().sleepyness >= BoCPacificationNPC.MaxSleepy;

        if (rage <= MaxRage)
            npc.position += npc.velocity * (rage * 0.25f);

        if (sleepyParent)
        {
            if (npc.noTileCollide && Collision.SolidCollision(npc.position, npc.width, npc.height))
                return true;

            npc.noTileCollide = true;
            npc.velocity.Y += 0.05f;
            return false;
        }

        return true;
    }
}
