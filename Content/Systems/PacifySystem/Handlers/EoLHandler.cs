using BossForgiveness.Content.NPCs.Vanilla;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class EoLHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.HallowBoss;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 60 * 60 && !NPC.AnyNPCs(ModContent.NPCType<EoLPacified>());
    public override void OnPacify(NPC npc) => TransformInto<EoLPacified>(npc);
}
