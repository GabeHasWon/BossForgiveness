using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class EoCHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.EyeofCthulhu;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 1 * 1 && npc.ai[0] <= 2;

    public override void OnPacify(NPC npc) => TransformInto<EyePacified>(npc);
}
