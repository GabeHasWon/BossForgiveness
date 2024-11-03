using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;

namespace BossForgiveness.Content.NPCs.Mechanics;

public class WormPacificationNPC : GlobalNPC, ICustomBarNPC
{
    public const int MaxFood = 30;

    public override bool InstancePerEntity => true;

    internal int foodCount = 0;
    internal int lastWormCount = 0;
    internal bool wasHarmed = false;

    public override bool AppliesToEntity(NPC n, bool lateInstantiation) => n.type == NPCID.EaterofWorldsHead;

    public override bool PreAI(NPC npc)
    {
        if (wasHarmed || NPC.CountNPCS(NPCID.EaterofWorldsHead) > 1)
        {
            wasHarmed = true;
            return true;
        }

        if (foodCount > 0)
        {
            if (lastWormCount < foodCount)
            {
                if (foodCount % 10 == 0)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int n = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.DevourerHead, npc.whoAmI);
                        Main.npc[n].velocity = npc.velocity.RotatedByRandom(0.2f);
                    }

                    if (Main.netMode != NetmodeID.Server)
                        SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
                }

                lastWormCount = foodCount;
            }
        }

        return true;
    }

    internal static void AddFoodToHead(NPC npc)
    {
        if (npc.type == NPCID.EaterofWorldsHead)
        {
            npc.GetGlobalNPC<WormPacificationNPC>().foodCount += 2;
            return;
        }

        for (int i = npc.whoAmI - 1; i >= 0; --i)
        {
            NPC cur = Main.npc[i];

            if (cur.type == NPCID.EaterofWorldsHead)
            {
                cur.GetGlobalNPC<WormPacificationNPC>().foodCount++;
                break;
            }
        }
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = foodCount;
        barMax = MaxFood;
        return npc.life == npc.lifeMax && !wasHarmed;
    }
}