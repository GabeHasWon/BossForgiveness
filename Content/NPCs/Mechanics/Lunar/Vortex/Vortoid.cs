using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Vortex;

internal class Vortoid : ModNPC
{
    private bool Exploding
    {
        get => NPC.ai[0] == 1f;
        set => NPC.ai[0] = value ? 1 : 0;
    }

    private ref float Timer => ref NPC.ai[1];
    private ref float RotationSpeed => ref NPC.ai[2];
    private ref float TargetRotationSpeed => ref NPC.ai[3];

    public override void SetDefaults()
    {
        NPC.lifeMax = 15;
        NPC.dontTakeDamage = true;
        NPC.Size = new Vector2(44, 60);
        NPC.aiStyle = -1;
        NPC.noTileCollide = true;
    }

    public override void AI()
    {
        if (Exploding)
        {
            NPC.velocity *= 0.98f;
            NPC.velocity.Y += 0f;
            TargetRotationSpeed = MathHelper.Lerp(TargetRotationSpeed, RotationSpeed, 0.04f);
            NPC.rotation += TargetRotationSpeed;

            Timer++;

            if (Timer > 60)
            {
                NPC.active = false;

                for (int i = 0; i < 60; ++i) 
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.4f, 1.5f);
                    var offset = new Vector2(Main.rand.Next(NPC.width), Main.rand.Next(NPC.height));
                    Dust.NewDustPerfect(NPC.position + offset, DustID.Vortex, vel, Scale: Main.rand.NextFloat(0.8f, 1.6f));
                }

                for (int i = 0; i < 8; ++i)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(5, 5) * Main.rand.NextFloat(0.8f, 1.8f);
                    Gore.NewGore(NPC.GetSource_Death(), NPC.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
                }
            }

            return;
        }

        NPC.TargetClosest();
        Player target = Main.player[NPC.target];
        Vector2 destination = target.Center + target.velocity * 30 - new Vector2(0, 150);
        Timer++;
        NPC.velocity = (destination - NPC.Center) * (Timer / 300f) - new Vector2(0, 1);

        if (Timer >= 20f)
        {
            Exploding = true;
            RotationSpeed = Main.rand.NextFloat(-0.2f, 0.2f);
            Timer = 0;
        }
    }
}
