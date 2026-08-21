using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Attacks;

internal class VoidPortal : ModProjectile
{
    private ref float Timer => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.timeLeft = 400;
        Projectile.aiStyle = -1;
        Projectile.width = Projectile.height = 72;
    }

    public override void AI()
    {
        Timer++;

        if (Projectile.timeLeft < 100)
            Projectile.Opacity = Projectile.timeLeft / 100f;

        foreach (Player player in Main.ActivePlayers)
        {
            float dist = player.Distance(Projectile.Center);

            if (dist < 500)
            {
                player.position += player.DirectionTo(Projectile.Center) * (500 - dist) / 40f * Utils.GetLerpValue(0, 80, dist, true);
                Dust dust = SpawnDust(player.position, player.width, player.height);
                dust.velocity = player.DirectionTo(Projectile.Center) * 16;
            }
        }

        Vector2 newPos = Projectile.Center + new Vector2(Main.rand.NextFloat(500), 0).RotatedByRandom(MathHelper.Pi);
        Dust newDust = SpawnDust(newPos, 1, 1);
        newDust.velocity = newPos.DirectionTo(Projectile.Center) * 9;
    }

    private Dust SpawnDust(Vector2 pos, int width, int height)
    {
        int dust = Dust.NewDust(pos, width, height, DustID.WhiteTorch, newColor: Color.Black, Scale: 2);
        Main.dust[dust].noGravity = true;
        return Main.dust[dust];
    }

    public override bool PreDraw(ref Color lightColor) 
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;

        for (int i = 12; i >= 1; --i)
        {
            Color color = Color.Lerp(Color.DarkBlue, Color.Black, i / 6f) * (1 - i / 12f) * Projectile.Opacity;
            Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition + new Vector2(i, 0).RotatedBy(Timer * 0.8f / i), color);
        }

        Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, Color.Black * Projectile.Opacity);
        return false;
    }
}
