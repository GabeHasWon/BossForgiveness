using Terraria.ID;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.NPCs.Mechanics.LunaticCultist;

internal class LightningPredictorProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/NPC_0";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 1;
        Projectile.friendly = false;
        Projectile.hostile = false;
        Projectile.timeLeft = 60;
    }

    public override void AI()
    {
        Dust.NewDust(Projectile.Center, 1, 1, DustID.Electric);

        if (Projectile.timeLeft is 1 or 12 or 23 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 vel = Projectile.DirectionTo(Main.player[Player.FindClosest(Projectile.Center, 1, 1)].Center) * 8;
            int type = ProjectileID.CultistBossLightningOrbArc;
            Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, vel, type, 40, 0, Main.myPlayer, vel.ToRotation(), Main.rand.Next());
        }
    }
}

internal class AdjustedLightningProjectile : ModProjectile
{
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.CultistBossLightningOrbArc;

    public override void SetDefaults()
    {
        Projectile.CloneDefaults(ProjectileID.CultistBossLightningOrbArc);
        Projectile.tileCollide = false;
        Projectile.timeLeft = 50;

        AIType = ProjectileID.CultistBossLightningOrbArc;
    }
}