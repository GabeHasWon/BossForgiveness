using BossForgiveness.Content.NPCs.Mechanics;
using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class EoWHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.EaterofWorldsHead;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<WormPacificationNPC>().foodCount > WormPacificationNPC.MaxFood && !NPC.AnyNPCs(ModContent.NPCType<PacifiedEoW>());

    public override void OnPacify(NPC npc)
    {
        if (Main.netMode == NetmodeID.MultiplayerClient)
            return;

        List<Vector2> positions = [];

        for (int i = npc.whoAmI; i < Main.maxNPCs; ++i)
        {
            var chk = Main.npc[i];

            if (!chk.active)
                break;

            if (chk.type is >= NPCID.EaterofWorldsHead and <= NPCID.EaterofWorldsTail)
            {
                if (chk.type != NPCID.EaterofWorldsHead)
                {
                    positions.Add(chk.position);
                    chk.active = false;

                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, chk.whoAmI);
                }
            }

            if (chk.type == NPCID.EaterofWorldsTail)
                break;
        }

        float oldScale = npc.scale;
        npc.boss = true;

        TransformInto<PacifiedEoW>(npc);

        npc.scale = oldScale;
        (npc.ModNPC as PacifiedEoW).SpawnBody(positions);

        if (Main.netMode == NetmodeID.Server)
        {
            NetMessage.SendData(MessageID.SyncNPC, -1, -1, null, npc.whoAmI);
            new SyncEoWSegmentsModule(npc.whoAmI, [..positions]).Send(-1, -1, false);
        }
    }

    public override void Load(Mod mod)
    {
        base.Load(mod);
        On_EaterOfWorldsProgressBar.ValidateAndCollectNecessaryInfo += StopPacifiedBar;
    }

    private bool StopPacifiedBar(On_EaterOfWorldsProgressBar.orig_ValidateAndCollectNecessaryInfo orig, EaterOfWorldsProgressBar self, ref BigProgressBarInfo info)
    {
        bool valid = orig(self, ref info);
        return (!valid || Main.npc[info.npcIndexToAimAt].type != ModContent.NPCType<PacifiedEoW>()) && valid;
    }
}
