using System;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class KingSlimeHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.KingSlime;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 60 * 60;

    public override void OnPacify(NPC npc)
    {

    }
}
