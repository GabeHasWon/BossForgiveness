using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

internal class PlanteraPacificationNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    internal int pacification = 1;

    int _flowerTimer = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Plantera;

    public override bool PreAI(NPC npc)
    {
        if (npc.life < npc.lifeMax)
            return true;

        _flowerTimer++;

        if (_flowerTimer > 600)
            ShootFlower(npc);

        return true;
    }

    private void ShootFlower(NPC npc)
    {
        float reduction = MathHelper.Clamp(_flowerTimer - 600f, 0, 100f) / 100f;
        npc.position -= npc.velocity * reduction;

        if (_flowerTimer == 700f && Main.netMode != NetmodeID.MultiplayerClient)
        {
            int type = ModContent.ProjectileType<LilyProjectile>();
            Vector2 velocity = npc.DirectionTo(Main.player[npc.target].Center) * 6;
            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, type, 0, 0f, Main.myPlayer);
            _flowerTimer = 0;
        }
    }
}
