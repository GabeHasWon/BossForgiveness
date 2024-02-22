using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
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

        bool valid = npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 1 * 60 * 60 && selfLength < 8 && selfLength > 1 && !NPC.AnyNPCs(ModContent.NPCType<PacifiedEoW>());

        return valid;
    }

    public override void OnPacify(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        int count = _worm.Count;
        Span<Vector2> positions = stackalloc Vector2[count];

        for (int i = 0; i < count; ++i)
        {
            var worm = Main.npc[_worm.ElementAt(i)];
            worm.active = false;

            positions[i] = worm.position;

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, worm.whoAmI);
        }

        TransformInto<PacifiedEoW>(npc);
        (npc.ModNPC as PacifiedEoW).SpawnBody(positions);
        
        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
    }
}
