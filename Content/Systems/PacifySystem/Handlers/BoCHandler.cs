﻿using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class BoCHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.BrainofCthulhu;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2f * 2 * 60 && !NPC.AnyNPCs(ModContent.NPCType<BoCPacified>());

    public override void OnPacify(NPC npc)
    {
        TransformInto<BoCPacified>(npc);
        (npc.ModNPC as BoCPacified).BuildCreepers();
    }
}
