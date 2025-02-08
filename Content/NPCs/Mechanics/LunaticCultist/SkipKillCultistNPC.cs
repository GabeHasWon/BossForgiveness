using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.LunaticCultist;

internal class SkipKillCultistNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private int timer = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.CultistTablet;

    public override bool PreAI(NPC npc)
    {
        if (NPC.AnyNPCs(NPCID.CultistBoss))
            return true;

        if (timer >= 60 * 6)
        {
            foreach (NPC other in Main.ActiveNPCs)
            {
                if (other.type is NPCID.CultistArcherBlue or NPCID.CultistDevote)
                {
                    other.active = false;
                    other.netUpdate = true;

                    for (int i = 0; i < 15; ++i)
                    {
                        Vector2 pos = other.position + new Vector2(Main.rand.Next(other.width), Main.rand.Next(other.height));
                        Dust.NewDustPerfect(pos, DustID.GoldFlame, new Vector2(0, Main.rand.NextFloat(-8, -4)), Scale: Main.rand.NextFloat(1.5f, 2f));
                    }

                    for (int i = 0; i < 3; ++i)
                        Gore.NewGore(other.GetSource_Death(), other.Center, Vector2.Zero, GoreID.Smoke1 + Main.rand.Next(3));
                }
            }
        }

        foreach (var player in Main.ActivePlayers)
        {
            if (player.DistanceSQ(npc.Center + new Vector2(0, 60)) < 20 * 20)
            {
                Vector2 pos = player.position + new Vector2(Main.rand.Next(player.width), Main.rand.Next(player.height));
                Dust.NewDustPerfect(pos, DustID.GoldFlame, new Vector2(0, Main.rand.NextFloat(-8, -4)), Scale: Main.rand.NextFloat(1.5f, 2f));

                timer++;
                return true;
            }
        }

        return true;
    }
}
