using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Stardust;

internal class StardustPortal : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(120);
        Projectile.hide = true;
    }

    public override void DrawBehind(int index, List<int> bNPCAndTiles, List<int> behindNPCs, List<int> bP, List<int> overPlayers, List<int> overWiresUI)
    {
        behindNPCs.Add(index);
    }

    public override void AI() => Projectile.rotation += 0.06f;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Vector2 pos = Projectile.Center - Main.screenPosition;
        
        for (int i = 0; i < 4; ++i)
        {
            float rot = (i % 2 == 0 ? -Projectile.rotation : Projectile.rotation) * (1 + i * 0.5f);
            float scale = 1.2f - i * 0.2f;
            float opacity = 0.4f + i * 0.2f;

            Main.spriteBatch.Draw(tex, pos, null, Color.White * opacity, rot, tex.Size() / 2f, scale, SpriteEffects.None, 0);
        }

        return false;
    }
}
