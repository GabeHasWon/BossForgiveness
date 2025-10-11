using BossForgiveness.Common;
using BossForgiveness.Content.NPCs.Mechanics.MoonLord;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Vortex;

internal class Vortoid : ModNPC
{
    private bool Exploding
    {
        get => NPC.localAI[0] == 1f;
        set => NPC.localAI[0] = value ? 1 : 0;
    }

    private Vector2 Target
    {
        get => new(NPC.localAI[1], NPC.localAI[2]);
        set => (NPC.localAI[1], NPC.localAI[2]) = (value.X, value.Y);
    }

    private ref float TargetFactor => ref NPC.localAI[3];

    private ref float Pillar => ref NPC.ai[0];
    private ref float Timer => ref NPC.ai[1];
    private ref float RotationSpeed => ref NPC.ai[2];
    private ref float TargetRotationSpeed => ref NPC.ai[3];

    private NPC PillarNPC => Main.npc[(int)Pillar];

    private Vector2 lastTarget = Vector2.Zero;

    public override void SetDefaults()
    {
        NPC.lifeMax = 900;
        NPC.Size = new Vector2(44, 60);
        NPC.aiStyle = -1;
        NPC.noTileCollide = true;
    }

    public override bool? CanBeHitByItem(Player player, Item item) => false;
    public override bool CanBeHitByNPC(NPC attacker) => false;
    public override bool? CanBeHitByProjectile(Projectile projectile) => false;

    public override void AI()
    {
        if (!Exploding && NPC.life == 1)
        {
            Exploding = true;
            Timer = 0;
        }

        if (Exploding)
        {
            Timer++;

            if (Timer > 50)
            {
                NPC.velocity *= 0.98f;
                NPC.velocity.Y += 0f;
                TargetRotationSpeed = MathHelper.Lerp(TargetRotationSpeed, RotationSpeed, 0.04f);
                NPC.rotation += TargetRotationSpeed;

                if (Timer > 90)
                {
                    NPC.active = false;
                    PillarNPC.active = false;
                    PillarNPC.netUpdate = true;

                    MoonLordPacificationTracker.LunarTowersPacified.Add(NPCID.LunarTowerVortex);

                    SpawnGoreForNPC(NPC);
                    SpawnGoreForNPC(PillarNPC);
                }

                return;
            }
            else if (Timer == 1)
            {
                RotationSpeed = Main.rand.NextFloat(-0.2f, 0.2f);
            }
            else if (Timer > 1)
            {
                NPC.TargetClosest();
                Player target = Main.player[NPC.target];
                Vector2 destination = target.Center + target.velocity * 30 - new Vector2(0, 150);
                NPC.velocity = (destination - NPC.Center) * (Timer / 50f) - new Vector2(0, 1);
                NPC.velocity *= 0.05f;
            }
        }

        if ((Target == Vector2.Zero || TargetFactor >= 1) && Main.netMode != NetmodeID.MultiplayerClient)
        {
            lastTarget = Target == Vector2.Zero ? NPC.Center : Target;

            do 
            {
                Target = PillarNPC.Center + Main.rand.NextVector2Circular(800, 800);
            } while (Collision.SolidCollision(Target, 32, 48));

            TargetFactor = 0;
            NPC.netUpdate = true;
        }

        TargetFactor += 0.006f;
        NPC.Center = Vector2.Lerp(lastTarget, Target, EaseFunction.EaseCubicInOut.Ease(TargetFactor));
    }

    private static void SpawnGoreForNPC(NPC npc)
    {
        for (int i = 0; i < 60; ++i)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.4f, 1.5f);
            var offset = new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
            Dust.NewDustPerfect(npc.position + offset, DustID.Vortex, vel, Scale: Main.rand.NextFloat(0.8f, 1.6f));
        }

        for (int i = 0; i < 8; ++i)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(5, 5) * Main.rand.NextFloat(0.8f, 1.8f);
            Gore.NewGore(npc.GetSource_Death(), npc.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
        }
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Exploding);
        writer.Write((Half)TargetFactor);
        writer.Write(Target.X);
        writer.Write(Target.Y);
        writer.Write(lastTarget.X);
        writer.Write(lastTarget.Y);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Exploding = reader.ReadBoolean();
        TargetFactor = (float)reader.ReadHalf();
        Target = new(reader.ReadSingle(), reader.ReadSingle());
        lastTarget = new(reader.ReadSingle(), reader.ReadSingle());
    }
}
