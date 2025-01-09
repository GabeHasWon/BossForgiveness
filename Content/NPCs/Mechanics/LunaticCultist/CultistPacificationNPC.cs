using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.LunaticCultist;

internal class CultistPacificationNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public bool enraged = false;

    private int _timer = 0;
    private int _state = 0;

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
        if (_timer == 0)
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
        }

        _timer++;

        npc.TargetClosest();
        npc.spriteDirection = npc.direction;

        Player player = Main.player[npc.target];

        npc.damage = 0;
        npc.velocity += npc.DirectionTo(player.Center) * 0.4f;

        if (npc.velocity.LengthSquared() > 14 * 14)
            npc.velocity = Vector2.Normalize(npc.velocity) * 14;

        if (_state == 0)
        {
            if (_timer > 150 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 vel = npc.DirectionTo(player.Center) * 8;

                for (int i = 0; i < 3; ++i)
                {
                    Vector2 velocity = vel.RotatedByRandom(0.2f) * Main.rand.NextFloat(0.7f, 1f);
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, ProjectileID.CultistBossFireBall, 20, 2f, Main.myPlayer);
                }

                npc.velocity -= vel * 2;

                _timer = 0;
            }
            else if (_timer > 100)
            {
                npc.velocity *= 0.97f;

                for (int i = 0; i < 2; ++i)
                {
                    Vector2 vel = Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.8f, 1);
                    Dust.NewDust(HandPos(npc, i == 0), 1, 1, DustID.Torch, vel.X, vel.Y, Scale: Main.rand.NextFloat(2, 3) * ((_timer - 100) / 50f));
                }
            }
        }
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
                        Main.npc[cultist].GetGlobalNPC<CultistPacificationNPC>().enraged = true;
                }

                anyOneNearby = true;
            }
        }

        if (anyOneNearby)
            projectile.ai[0]--;
    }
}
