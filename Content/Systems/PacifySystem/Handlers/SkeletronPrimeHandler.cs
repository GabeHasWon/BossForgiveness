using BossForgiveness.Content.NPCs.Mechanics.Mech;
using BossForgiveness.Content.NPCs.Vanilla;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class SkeletronPrimeHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.SkeletronPrime;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<MechBossPacificationNPC>().stunCount >= MechBossPacificationNPC.MaxStun 
        && !NPC.AnyNPCs(ModContent.NPCType<SkelePrimePacified>()) && !Main.dayTime;
    public override void OnPacify(NPC npc) => TransformInto<SkelePrimePacified>(npc);
}
