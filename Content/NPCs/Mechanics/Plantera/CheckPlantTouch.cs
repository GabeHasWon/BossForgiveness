using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

internal class CheckPlantTouch
{
    public static void CheckTouch(Projectile projectile, NPC plantera)
    {
        foreach (var player in Main.ActivePlayers)
        {
            if (player.Hitbox.Intersects(projectile.Hitbox))
            {
                int type = ModContent.ProjectileType<GrowthProjectileFX>();
                var source = projectile.GetSource_FromAI();
                Projectile.NewProjectile(source, projectile.Center, Vector2.Zero, type, 0, 0, Main.myPlayer, plantera.whoAmI);
                projectile.Kill();
            }
        }
    }
}
