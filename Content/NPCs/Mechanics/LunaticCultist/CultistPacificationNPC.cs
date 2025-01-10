using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.LunaticCultist;

internal class CultistPacificationNPC : GlobalNPC, ICustomBarNPC
{
    public override bool InstancePerEntity => true;

    public bool enraged = false;

    private bool initialized = false;
    private Vector2 swingOffset = Vector2.Zero;
    private bool willOrbit = false;
    private float pacifyTime = 0;
    private Vector2 lightningPos = Vector2.Zero;

    public static Vector2 HandPos(NPC npc, bool left) 
    {
        Vector2 leftVec = npc.Center + new Vector2(npc.spriteDirection == -1 ? 16 : -16, 6);
        return left ? leftVec : npc.Center + new Vector2(npc.spriteDirection == 1 ? 16 : -16, 6);
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.CultistBoss;

    public override bool PreAI(NPC npc)
    {
        if (enraged)
        {
            NewAI(npc);
            return false;
        }

        return true;
    }

    private void NewAI(NPC npc)
    {
        ref float timer = ref npc.ai[0];
        ref float state = ref npc.ai[1];
        ref float swingPlayer = ref npc.ai[2];

        if (!initialized)
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

            initialized = true;
        }


        if (timer == 0 && state != 1)
            willOrbit = Main.rand.NextBool(3);
        else if (state == 1)
            willOrbit = false;

        timer++;

        npc.TargetClosest();
        npc.spriteDirection = npc.direction;

        Player player = Main.player[npc.target];

        pacifyTime += MathF.Max((300 - player.Distance(npc.Center)) / 150f, 0);

        if (pacifyTime > 1000)
        {
            npc.Pacify<CultistPacified>();
            return;
        }

        if (willOrbit || state == 1 && Main.rand.NextBool(2, 3))
        {
            for (int i = 0; i < 8; ++i)
                if (Main.rand.NextBool())
                    Dust.NewDustPerfect(Vector2.Lerp(npc.Center, player.Center, i / 8f + (Main.rand.NextFloat(0.1f) - 0.05f)), DustID.Shadowflame);
        }

        npc.damage = 0;
        npc.velocity += npc.DirectionTo(player.Center) * 0.4f;

        if (npc.velocity.LengthSquared() > 14 * 14)
            npc.velocity = Vector2.Normalize(npc.velocity) * 14;

        if (state == 0)
            FireballAttack(npc, player, ref timer, ref state);
        else if (state == 1)
            SwingAttack(npc, player, ref timer, ref swingPlayer, ref state);
        else if (state == 2)
            LightningAttack(npc, player, ref timer, ref state);
    }

    private void LightningAttack(NPC npc, Player player, ref float timer, ref float state)
    {
        if (timer == 1)
            lightningPos = player.Center + Main.rand.NextVector2CircularEdge(400, 400) * Main.rand.NextFloat(0.8f, 1f);
        else if (timer == 180)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 vel = lightningPos.DirectionTo(player.Center) * 8;
                Projectile.NewProjectile(npc.GetSource_FromAI(), lightningPos, vel, ProjectileID.CultistBossLightningOrbArc, 40, 0, Main.myPlayer);
            }
        }
        else if (timer > 180)
        {
            SetState(ref timer, ref state);
        }

        Dust.NewDust(lightningPos, 1, 1, DustID.Electric);
    }

    private void SwingAttack(NPC npc, Player player, ref float timer, ref float swingPlayer, ref float state)
    {
        const float SwingSpeed = 0.04f;

        if (timer <= 1)
        {
            swingPlayer = player.whoAmI;
            swingOffset = player.Center - npc.Center;
        }
        else if (timer < 200f)
        {
            npc.velocity = Vector2.Zero;
            npc.Center = Vector2.Lerp(npc.Center, Main.player[(int)swingPlayer].Center - swingOffset.RotatedBy((timer - 1) * SwingSpeed), MathF.Min(timer / 120f, 1));

            if (timer > 60 && timer % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(player.Center) * 7, ProjectileID.CultistBossIceMist, 30, 0);
        }
        else
        {
            npc.velocity = swingOffset.RotatedBy((timer - 1) * SwingSpeed) - swingOffset.RotatedBy(timer * SwingSpeed);
            SetState(ref timer, ref state);
        }
    }

    private void FireballAttack(NPC npc, Player player, ref float timer, ref float state)
    {
        if (timer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 vel = npc.DirectionTo(player.Center) * 8;

            for (int i = 0; i < 3; ++i)
            {
                Vector2 velocity = vel.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.7f, 1f);
                Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, ProjectileID.CultistBossFireBall, 20, 2f, Main.myPlayer);
            }

            npc.velocity -= vel * 2;

            SetState(ref timer, ref state);
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

    private void SetState(ref float timer, ref float state)
    {
        timer = 0;
        state = Main.rand.NextBool() ? 0 : 2;

        if (willOrbit)
        {
            state = 1;
            willOrbit = false;
        }
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = pacifyTime;
        barMax = 1000;
        return npc.life >= npc.lifeMax;
    }
}

internal class CultistRitualProjectile : GlobalProjectile
{
    public override bool InstancePerEntity => true;

    private int _timer = 0;

    public override bool AppliesToEntity(Projectile entity, bool lateInstantiation) => entity.type == ProjectileID.CultistRitual;

    public override void AI(Projectile projectile)
    {
        bool anyOneNearby = false;

        foreach (var plr in Main.ActivePlayers)
        {
            if (plr.DistanceSQ(projectile.Center) < 20 * 20)
            {
                Vector2 pos = plr.position + new Vector2(Main.rand.Next(plr.width), Main.rand.Next(plr.height));
                Dust.NewDustPerfect(pos, DustID.GoldFlame, new Vector2(0, Main.rand.NextFloat(-8, -4)), Scale: Main.rand.NextFloat(1.5f, 2f));

                if (_timer++ > 5 * 60)
                {
                    int cultist = NPC.FindFirstNPC(NPCID.CultistBoss);

                    if (cultist != -1)
                    {
                        Main.npc[cultist].GetGlobalNPC<CultistPacificationNPC>().enraged = true;
                        Main.npc[cultist].ai[0] = 0;
                    }
                }

                anyOneNearby = true;
            }
        }

        if (anyOneNearby)
            projectile.ai[0]--;
    }
}
