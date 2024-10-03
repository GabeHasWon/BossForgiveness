using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

internal class GrowthProjectileFX : ModProjectile
{
    public override string Texture => "Terraria/Images/NPC_0";

    private ref float TargetId => ref Projectile.ai[0];

    private NPC Target => Main.npc[(int)TargetId];

    public override void SetDefaults()
    {
        Projectile.Size = new(8);
        Projectile.timeLeft = 600;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.hide = true;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        Vector2 vel = Projectile.velocity * 0.2f;
        Dust.NewDust(Projectile.position, 1, 1, DustID.HealingPlus, vel.X * 0.2f, vel.Y * 0.2f, Scale: 2f);

        Projectile.velocity = Projectile.DirectionTo(Target.Center) * 8;
        Projectile.timeLeft++;

        if (Projectile.DistanceSQ(Target.Center) < 14 * 14)
        {
            Target.GetGlobalNPC<PlanteraPacificationNPC>().pacification++;
            Projectile.Kill();
        }
    }
}
