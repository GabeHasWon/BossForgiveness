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

        return npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > PacifyTime && npc.life > npc.lifeMax / 2f && !NPC.AnyNPCs(ModContent.NPCType<SpazmatismPacified>()) && retinazer;
    }

    public override void OnPacify(NPC npc)
    {
        TransformInto<SpazmatismPacified>(npc, new Vector2(0, 16));

        int ret = NPC.FindFirstNPC(NPCID.Retinazer);
        (npc.ModNPC as SpazmatismPacified).InitRetinazer(ret);
        Main.npc[ret].active = false;
    }
}
