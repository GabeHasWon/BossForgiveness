using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class EoWHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.EaterofWorldsHead;

    private HashSet<int> _worm = [];

    public override bool CanPacify(NPC npc)
    {
        _worm = [];
        int selfLength = 0;

        for (int i = npc.whoAmI; i < Main.maxNPCs; ++i)
        {
            var chk = Main.npc[i];

            if (!chk.active)
                break;

            if (chk.type >= NPCID.EaterofWorldsHead && chk.type <= NPCID.EaterofWorldsTail)
            {
                selfLength++;

                if (chk.type != NPCID.EaterofWorldsHead)
                    _worm.Add(chk.whoAmI);
            }

            if (chk.type == NPCID.EaterofWorldsTail)
                break;
        }

        return npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 1 * 2 * 60 && selfLength < 8 && selfLength > 1 && !NPC.AnyNPCs(ModContent.NPCType<PacifiedEoW>());
    }

    public override void OnPacify(NPC npc)
    {
        int count = _worm.Count;
        Span<Vector2> positions = stackalloc Vector2[count];

        for (int i = 0; i < count; ++i)
        {
            var worm = Main.npc[_worm.ElementAt(i)];
            worm.active = false;
            positions[i] = worm.position;
        }

        TransformInto<PacifiedEoW>(npc);
        (npc.ModNPC as PacifiedEoW).SpawnBody(positions);
    }
}
