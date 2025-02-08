using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.LunaticCultist;

internal class CultistPacificationNPC : GlobalNPC, ICustomBarNPC
{
    public const int MaxPacify = 1000;

    public override bool InstancePerEntity => true;

    private static float drawResetTimer = 0;

    public bool enraged = false;

    private bool initialized = false;
    private Vector2 swingOffset = Vector2.Zero;
    private bool swingClockwise = true;
    private bool willOrbit = false;
    private float pacifyTime = 0;
    private float dustCounter = 0;

    public static Vector2 HandPos(NPC npc, bool left) 
    {
        Vector2 leftVec = npc.Center + new Vector2(npc.spriteDirection == -1 ? 16 : -16, 6);
        return left ? leftVec : npc.Center + new Vector2(npc.spriteDirection == 1 ? 16 : -16, 6);
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.CultistBoss;

    public override void SetStaticDefaults() => PrioritizePreAINPC.PreAIHooks.Add(NPCID.CultistBoss, PriorityPreAI);

    public static bool PriorityPreAI(NPC npc)
    {
        if (npc.GetGlobalNPC<CultistPacificationNPC>().enraged)
        {
            NewAI(npc);
            return false;
        }

        return true;
    }

    private static void NewAI(NPC npc)
    {
        ref float timer = ref npc.ai[0];
        ref float state = ref npc.ai[1];
        ref float swingPlayer = ref npc.ai[2];

        CultistPacificationNPC cultistNPC = npc.GetGlobalNPC<CultistPacificationNPC>();

        if (!cultistNPC.initialized)
        {
            foreach (var other in Main.ActiveNPCs)
            {
                if (other.type == NPCID.CultistBossClone)
                {
                    other.active = false;

                    for (int i = 0; i < 3; ++i)
                        Gore.NewGore(other.GetSource_Death(), other.Center, Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.6f, 1f), GoreID.Smoke1 + Main.rand.Next(3));
                }
            }

            swingPlayer = -1;
            state = 0;
            timer = 0;

            cultistNPC.initialized = true;
        }

        if (timer == 0 && state != 1)
        {
            cultistNPC.willOrbit = Main.rand.NextBool(3);

            npc.netUpdate = true;
        }
        else if (state == 1)
            cultistNPC.willOrbit = false;

        timer++;

        npc.TargetClosest();
        npc.spriteDirection = npc.direction;

        Player player = Main.player[npc.target];

        float increase = MathF.Max((450f - player.Distance(npc.Center)) / 300f, 0);
        cultistNPC.pacifyTime += increase;
        cultistNPC.dustCounter += increase;

        while (cultistNPC.dustCounter > 1)
        {
            Vector2 pos = player.position + new Vector2(Main.rand.Next(player.width), Main.rand.Next(player.height));
            Dust.NewDustPerfect(pos, DustID.GoldFlame, new Vector2(0, Main.rand.NextFloat(-8, -4)), Scale: Main.rand.NextFloat(1.5f, 2f));

            cultistNPC.dustCounter--;
        }

        if (cultistNPC.pacifyTime > 1000)
        {
            npc.Pacify<CultistPacified>();
            return;
        }

        if (cultistNPC.willOrbit || state == 1 && Main.rand.NextBool(2, 3))
        {
            float reps = cultistNPC.willOrbit ? 8 : 14;

            for (int i = 0; i < reps; ++i)
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(Vector2.Lerp(npc.Center, player.Center, i / reps + (Main.rand.NextFloat(0.1f) - 0.05f)), DustID.Shadowflame);
        }

        npc.damage = 0;
        npc.velocity += npc.DirectionTo(player.Center) * 0.4f;

        if (npc.DistanceSQ(player.Center) > 700 * 700)
            npc.velocity += npc.DirectionTo(player.Center) * 0.6f;

        if (npc.velocity.LengthSquared() > 14 * 14)
            npc.velocity = Vector2.Normalize(npc.velocity) * 14;

        if (state == 0)
            cultistNPC.FireballAttack(npc, player, ref timer, ref state);
        else if (state == 1)
            cultistNPC.SwingAttack(npc, player, ref timer, ref swingPlayer, ref state);
        else if (state == 2)
            cultistNPC.LightningAttack(npc, ref timer, ref state);
        else if (state == 3)
            cultistNPC.TeleportAttack(npc, player, ref timer, ref state);
    }

    private void TeleportAttack(NPC npc, Player player, ref float timer, ref float state)
    {
        if (timer < 40)
        {
            for (int i = 0; i < 3; ++i)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2CircularEdge(120, 120);
                Vector2 vel = pos.DirectionTo(npc.Center) * 14;
                Dust.NewDustPerfect(pos, DustID.ShadowbeamStaff, vel, Scale: Main.rand.NextFloat(0.5f, 1.8f));
            }
        }

        if (timer == 40)
        {
            for (int i = 0; i < 40; ++i)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2CircularEdge(60, 60);
                Vector2 vel = pos.DirectionFrom(npc.Center) * Main.rand.NextFloat(12, 26);
                Dust.NewDustPerfect(pos, DustID.ShadowbeamStaff, vel, Scale: Main.rand.NextFloat(0.9f, 2.2f));
            }

            if (npc.life < npc.lifeMax / 4 || pacifyTime > MaxPacify * 0.75f && Main.netMode != NetmodeID.MultiplayerClient)
                NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.AncientDoom, 0, npc.whoAmI);

            Vector2 playerOffset = player.Center - npc.Center;
            npc.Center = player.Center + playerOffset;
            npc.netOffset = Vector2.Zero;
            npc.netUpdate = true;

            for (int i = 0; i < 40; ++i)
            {
                Vector2 pos = npc.Center + Main.rand.NextVector2CircularEdge(60, 60);
                Vector2 vel = pos.DirectionFrom(npc.Center) * Main.rand.NextFloat(12, 26);
                Dust.NewDustPerfect(pos, DustID.ShadowbeamStaff, vel, Scale: Main.rand.NextFloat(0.9f, 2.2f));
            }
        }
        else if (timer == 80)
            SetState(npc, ref timer, ref state);
    }

    private void LightningAttack(NPC npc, ref float timer, ref float state)
    {
        for (int i = 0; i < 2; ++i)
        {
            Vector2 vel = Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.8f, 1);
            Dust.NewDust(HandPos(npc, i == 0), 1, 1, DustID.Electric, vel.X, vel.Y, Scale: Main.rand.NextFloat(2, 3) * (timer % 60 / 240f));
        }

        if (timer > 0 && timer % 60 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            var lightningPos = npc.Center + Main.rand.NextVector2CircularEdge(60, 60) * Main.rand.NextFloat(0.8f, 1f);
            Projectile.NewProjectile(npc.GetSource_FromAI(), lightningPos, Vector2.Zero, ModContent.ProjectileType<LightningPredictorProjectile>(), 0, 0, Main.myPlayer);

            Dust.NewDust(lightningPos, 1, 1, DustID.Electric, 0, 0, Scale: Main.rand.NextFloat(2, 3));
        }
        else if (timer > 180)
            SetState(npc, ref timer, ref state);
    }

    private void SwingAttack(NPC npc, Player player, ref float timer, ref float swingPlayer, ref float state)
    {
        const float SwingSpeed = 0.04f;

        if (timer <= 1)
        {
            swingPlayer = player.whoAmI;
            swingOffset = player.Center - npc.Center;
            swingClockwise = MathF.Sign(npc.velocity.X) == 0;
        }
        else if (timer < 200f)
        {
            swingOffset = Vector2.Lerp(swingOffset, Vector2.Normalize(swingOffset) * 450, 0.02f);

            float rotation = (timer - 1) * SwingSpeed * (swingClockwise ? 1 : -1);
            npc.velocity = Vector2.Zero;
            npc.Center = Vector2.Lerp(npc.Center, Main.player[(int)swingPlayer].Center - swingOffset.RotatedBy(rotation), MathF.Min(timer / 120f, 1));

            if (timer > 60 && timer % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(player.Center) * 7, ProjectileID.CultistBossIceMist, 30, 0);
        }
        else
        {
            npc.velocity = swingOffset.RotatedBy((timer - 1) * SwingSpeed) - swingOffset.RotatedBy(timer * SwingSpeed) + player.velocity;
            SetState(npc, ref timer, ref state);
        }
    }

    private void FireballAttack(NPC npc, Player player, ref float timer, ref float state)
    {
        if (timer > 150)
        {
            Vector2 vel = npc.DirectionTo(player.Center) * 8;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; ++i)
                {
                    Vector2 velocity = vel.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.7f, 1f);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, ProjectileID.CultistBossFireBall, 20, 2f, Main.myPlayer);
                }
            }

            npc.velocity -= vel * 2;

            SetState(npc, ref timer, ref state);
        }
        else if (timer > 100)
        {
            npc.velocity *= 0.97f;

            for (int i = 0; i < 2; ++i)
            {
                Vector2 vel = Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.8f, 1);
                Dust.NewDust(HandPos(npc, i == 0), 1, 1, DustID.Torch, vel.X, vel.Y, Scale: Main.rand.NextFloat(2, 3) * ((timer - 100) / 50f));
            }
        }
    }

    private void SetState(NPC npc, ref float timer, ref float state)
    {
        timer = 0;

        if (npc.life < npc.lifeMax / 2 && pacifyTime < MaxPacify / 2)
            state = Main.rand.NextBool() ? 0 : 2;
        else
            state = Main.rand.Next(3) switch
            {
                0 => 0,
                1 => 2, 
                _ => 3,
            };

        if (willOrbit)
        {
            state = 1;
            willOrbit = false;
        }
    }

    public override Color? GetAlpha(NPC npc, Color drawColor)
    {
        if (npc.GetGlobalNPC<CultistPacificationNPC>().enraged)
        {
            float pacify = npc.GetGlobalNPC<CultistPacificationNPC>().pacifyTime / MaxPacify;
            float health = 1 - npc.life / (float)npc.lifeMax;
            return Color.Lerp(drawColor, Color.Purple, MathF.Max(pacify, health));
        }

        return null;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(willOrbit);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        willOrbit = bitReader.ReadBit();
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        drawResetTimer = npc.ai[0];

        if (npc.GetGlobalNPC<CultistPacificationNPC>().enraged)
            npc.ai[0] = 1;

        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => npc.ai[0] = drawResetTimer;

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = pacifyTime;
        barMax = MaxPacify;
        return npc.life >= npc.lifeMax;
    }
}
