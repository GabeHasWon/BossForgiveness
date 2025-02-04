using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.LunaticCultist;

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

                if (_timer++ > 4 * 60)
                {
                    int cultist = NPC.FindFirstNPC(NPCID.CultistBoss);

                    if (cultist != -1)
                    {
                        Main.npc[cultist].GetGlobalNPC<CultistPacificationNPC>().enraged = true;
                        Main.npc[cultist].ai[0] = 0;
                        Main.npc[cultist].velocity = projectile.DirectionFrom(Main.npc[cultist].Center) * 6;
                    }

                    for (int i = 0; i < 60; ++i)
                    {
                        pos = projectile.position + new Vector2(-130 + Main.rand.Next(260), -130 + Main.rand.Next(260));
                        Dust.NewDustPerfect(pos, DustID.GoldFlame, new Vector2(0, Main.rand.NextFloat(-8, -4)), Scale: Main.rand.NextFloat(1.5f, 2f));
                    }

                    projectile.active = false;
                }

                anyOneNearby = true;
            }
        }

        if (anyOneNearby)
            projectile.ai[0]--;
    }
}
