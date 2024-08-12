using BossForgiveness.Content.NPCs.Vanilla.Enemies;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Enemies;

internal class NymphPacificationNPC : GlobalNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Nymph || entity.type == NPCID.LostGirl;

    public override bool PreAI(NPC npc)
    {
        foreach (var proj in Main.ActiveProjectiles)
        {
            if (proj.type == ProjectileID.PurificationPowder && npc.Hitbox.Intersects(proj.Hitbox))
            {
                npc.Pacify<LostGirlPacified>();
                return false;
            }
        }

        return true;
    }
}
