using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using System;
using Terraria.Audio;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

public class BlackPoppyProjectile : ModProjectile
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

        if (Projectile.timeLeft == 3)
        {
            for (int i = 0; i < 30; ++i)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(10, 10) * Main.rand.NextFloat(0.5f, 1f);
                Dust.NewDustPerfect(Projectile.Center, DustID.Torch, vel);
            }

            for (int i = 0; i < 6; ++i)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.3f, 1f);
                Gore.NewGorePerfect(Projectile.GetSource_Death(), Projectile.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
            }

            Projectile.Resize(180, 180);
            Projectile.hostile = true;
            Projectile.damage = 50;

            SoundEngine.PlaySound(SoundID.Item8, Projectile.Center);
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
            Timer++;

            Projectile.frame = (int)MathHelper.Clamp(Timer / 8, 0, 2);

            if (Projectile.timeLeft > 4)
            {
                if (Timer > 180f)
                {
                    Projectile.timeLeft = 4;
                }

                foreach (var player in Main.ActivePlayers)
                {
                    if (player.Hitbox.Intersects(Projectile.Hitbox))
                        Projectile.timeLeft = 4;
                }
            }
        }
    }

    public override Color? GetAlpha(Color lightColor)
    {
        var baseColor = Color.Lerp(lightColor, Color.Red, 0.4f);

        if (Timer > 90)
            return Color.Lerp(baseColor, Color.Black, (Timer - 90) / 90f);

        return baseColor;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity *= 0;
        return !(IsLanded = true);
    }
}