using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class LunaticCultistHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.CultistBoss;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 60 * 60 && !NPC.AnyNPCs(ModContent.NPCType<CultistPacified>());
    public override void OnPacify(NPC npc) => TransformInto<CultistPacified>(npc);
}
