using Terraria;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

internal class CheckPlantTouch
{
    public static void CheckTouch(Projectile projectile, NPC plantera)
    {
        foreach (var player in Main.ActivePlayers)
        {
            if (player.Hitbox.Intersects(projectile.Hitbox))
            {
                plantera.GetGlobalNPC<PlanteraPacificationNPC>().pacification++;
                projectile.Kill();
                return;
            }
        }
    }
}
