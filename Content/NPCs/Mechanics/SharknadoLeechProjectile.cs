using BossForgiveness.Content.NPCs.Misc;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class SharknadoLeechProjectile : GlobalProjectile
{
    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.Sharknado;

    public override bool PreAI(Projectile projectile)
    {
        if (projectile.timeLeft == 538 && Main.rand.NextBool(1))
        {
            var src = projectile.GetSource_FromThis();
            NPC.NewNPC(src, (int)projectile.Center.X, (int)projectile.Center.Y, ModContent.NPCType<TruffleLeechNPC>(), 0, projectile.whoAmI, -1);
        }

        return true;
    }
}
