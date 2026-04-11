using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class ShimmerPortal : ModProjectile
{
    public override void SetStaticDefaults() => Main.projFrames[Type] = 8;

    public override void SetDefaults()
    {
        Projectile.timeLeft = 2;
        Projectile.tileCollide = false;
        Projectile.aiStyle = -1;
        Projectile.Size = new Vector2(36);
    }

    public override void AI()
    {
        Projectile.timeLeft++;

        Projectile.frameCounter++;

        if (Projectile.frameCounter < 15)
            Projectile.frame = Projectile.frameCounter / 5;
        else
            Projectile.frame = (Projectile.frameCounter - 15) / 5 % 5 + 3;

        if (Main.rand.NextBool(15))
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.ShimmerSpark, Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-2, 2));
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        int texHeight = tex.Height / Main.projFrames[Type];
        Rectangle src = new(0, texHeight * Projectile.frame, tex.Width, texHeight);
        Color color = Color.White with { A = 240 };
        
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, src, color * 0.4f, 0f, src.Size() / 2f, 1.3f, SpriteEffects.None, 0);
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, src, color, 0f, src.Size() / 2f, 1f, SpriteEffects.None, 0);
        return false;
    }
}
