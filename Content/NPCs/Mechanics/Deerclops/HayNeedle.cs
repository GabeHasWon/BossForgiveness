using Terraria.ModLoader;
using Terraria;

namespace BossForgiveness.Content.NPCs.Mechanics.Deerclops;

public class HayNeedle : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.Size = new(6);
        Projectile.timeLeft = 240;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        if (Projectile.timeLeft < 30)
            Projectile.Opacity = Projectile.timeLeft / 30f;

        Projectile.velocity.Y += 0.2f;
    }
}

public class Splinter : HayNeedle
{
}
