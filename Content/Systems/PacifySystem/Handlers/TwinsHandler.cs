using BossForgiveness.Content.NPCs.Mechanics.Mech;
using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class SpazmatismHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.Spazmatism;

    public override bool CanPacify(NPC npc)
    {
        const int PacifyTime = 1 * 60 * 60;

        bool retinazer = false;

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC other = Main.npc[i];

            if (other.active && other.type == NPCID.Retinazer)
            {
                if (other.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > PacifyTime && other.life > other.lifeMax / 2f)
                    retinazer = true;

                break;
            }
        }

        return npc.GetGlobalNPC<MechBossPacificationNPC>().stunCount >= MechBossPacificationNPC.MaxStun && npc.life > npc.lifeMax / 2f 
            && !NPC.AnyNPCs(ModContent.NPCType<SpazmatismPacified>()) && retinazer;
    }

    public override void OnPacify(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int ret = NPC.FindFirstNPC(NPCID.Retinazer);
        Main.npc[ret].active = false;

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, ret);

        TransformInto<SpazmatismPacified>(npc, new Vector2(0, 16));

        (npc.ModNPC as SpazmatismPacified).InitRetinazer(ret);
        npc.netUpdate = true;
    }
}
