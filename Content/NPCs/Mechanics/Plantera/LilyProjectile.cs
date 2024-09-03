using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;
using System.Threading;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

public class LilyProjectile : ModProjectile
{
    private bool IsLanded
    {
        get => Projectile.ai[0] == 1f;
        set => Projectile.ai[0] = value ? 1f : 0f;
    }

    private bool Initialized
    {
        get => Projectile.ai[1] == 1f;
        set => Projectile.ai[1] = value ? 1f : 0f;
    }

    private ref float Timer => ref Projectile.ai[2];
    private ref float TargetId => ref Projectile.localAI[0];

    private NPC Target => Main.npc[(int)TargetId];

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    public override void SetDefaults()
    {
        Projectile.Size = new(24);
        Projectile.timeLeft = 600;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.friendly = false;
    }

    public override void AI()
    {
        if (!Initialized)
        {
            Initialized = true;
            TargetId = NPC.FindFirstNPC(NPCID.Plantera);
        }

        if (TargetId == -1 || !Target.active || Target.type != NPCID.Plantera)
        {
            Projectile.Kill();
            return;
        }

        if (!IsLanded)
            Projectile.velocity.Y += 0.01f;
        else
        {
            Timer++;

            Projectile.frame = (int)MathHelper.Clamp(Timer / 8, 0, 2);

            if (Timer % 240 == 0)
            {
                Vector2 velocity = Projectile.DirectionTo(Target.Center) * 5f;
                int type = ModContent.ProjectileType<LilyPetalProjectile>();
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, velocity, type, 40, 0, Projectile.owner);
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity *= 0;
        return !(IsLanded = true);
    }

    public class LilyPetalProjectile : ModProjectile
    {
        private ref float Timer => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.Size = new(10);
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.tileCollide = false;
        }

        public override void AI() => Projectile.velocity = Projectile.velocity.RotatedBy(MathF.Sin(Timer++ * 0.9f));
    }
}