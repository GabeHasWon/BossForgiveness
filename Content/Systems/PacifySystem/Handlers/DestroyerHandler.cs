using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class DestroyerHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.TheDestroyer;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 60 * 60 && !NPC.AnyNPCs(ModContent.NPCType<DestroyerPacified>());

    public override void OnPacify(NPC npc)
    {
        int count = NPC.GetDestroyerSegmentsCount() + 1;
        Span<Vector2> positions = stackalloc Vector2[count];
        int slot = 0;

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC worm = Main.npc[i];

            if (worm.active && (worm.type == NPCID.TheDestroyerBody || worm.type == NPCID.TheDestroyerTail) && worm.ai[3] == npc.whoAmI)
            {
                worm.active = false;
                positions[slot++] = worm.Center;
            }
        }

        TransformInto<DestroyerPacified>(npc);
        var list = (npc.ModNPC as DestroyerPacified).segments;
        DestroyerPacified.Segment parent = null;

        for (int i = 0; i < positions.Length; ++i)
        {
            var segment = new DestroyerPacified.Segment(positions[i], parent ?? (Entity)npc, i == positions.Length - 1);
            list.Add(segment);
            parent = segment;
        }
    }
}
