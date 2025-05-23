using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Stardust;

internal class StardustPillarPlayer : ModPlayer
{
    private static bool lastMouseLeft = true;

    public override void UpdateEquips()
    {
        if (Main.myPlayer == Player.whoAmI)
        {
            StardustPillarPacificationNPC.CheckComponents(static (comp, npc) =>
            {
                if (comp.Placed && !comp.Finished && Main.mouseLeft && !lastMouseLeft)
                {
                    comp.PlacedRotation++;

                    if (comp.PlacedRotation > CompRotation.Down)
                        comp.PlacedRotation = CompRotation.Up;

                    if (Main.netMode == NetmodeID.SinglePlayer)
                        CheckCompletion(npc);
                    else
                        new CheckCompleteStardustModule(npc.whoAmI).Send();

                    if (Main.netMode == NetmodeID.MultiplayerClient)
                        new SendComponentModule(npc.whoAmI, comp.Position.ToPoint(), true, comp.PlacedRotation).Send();
                }
                
                return false;
            });

            lastMouseLeft = Main.mouseLeft;
        }
    }

    internal static void CheckCompletion(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        StardustPillarPacificationNPC pac = npc.GetGlobalNPC<StardustPillarPacificationNPC>();
        bool finished = true;

        foreach (Component comp in pac.components.Values)
        {
            if (!comp.Finished)
            {
                finished = false;
                break;
            }
        }

        if (finished)
        {
            for (int i = 0; i < 40; ++i)
            {
                Vector2 pos = npc.position + new Vector2(Main.rand.NextFloat(npc.width), Main.rand.NextFloat(npc.height));
                Dust.NewDustPerfect(pos, DustID.Wet, Main.rand.NextVector2Circular(6, 6), Scale: Main.rand.NextFloat(1, 3)).noGravity = true;
            }

            pac.won = true;
            npc.active = false;
            npc.netUpdate = true;

            foreach (var other in Main.ActiveNPCs)
            {
                if (npc.type is NPCID.StardustCellBig or NPCID.StardustCellSmall or NPCID.StardustJellyfishBig or NPCID.StardustJellyfishSmall or NPCID.StardustSoldier
                    or NPCID.StardustSpiderBig or NPCID.StardustSpiderSmall or NPCID.StardustWormBody or NPCID.StardustWormHead or NPCID.StardustWormTail)
                {
                    other.active = false;
                    other.netUpdate = true;

                    for (int i = 0; i < 12; ++i)
                    {
                        Vector2 pos = npc.position + new Vector2(Main.rand.NextFloat(npc.width), Main.rand.NextFloat(npc.height));
                        Dust.NewDustPerfect(pos, DustID.Wet, Main.rand.NextVector2Circular(6, 6), Scale: Main.rand.NextFloat(1, 3)).noGravity = true;
                    }
                }
            }
        }
    }
}
