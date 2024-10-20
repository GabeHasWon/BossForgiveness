using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

public class PoppyProjectile : ModProjectile
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
        Projectile.Size = new(16);
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
        {
            Projectile.velocity.Y += 0.01f;
            Projectile.timeLeft++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
        else
        {
            CheckPlantTouch.CheckTouch(Projectile, Target);

            Timer++;

            Projectile.frame = (int)MathHelper.Clamp(Timer / 8, 0, 2);
            Projectile.Opacity = 1 - Timer / 180f;

            if (Projectile.Opacity <= 0)
            {
                Projectile.Kill();
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity *= 0;
        return !(IsLanded = true);
    }
}