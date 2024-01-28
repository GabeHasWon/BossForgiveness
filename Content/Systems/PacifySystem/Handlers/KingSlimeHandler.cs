using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class KingSlimeHandler : PacifiedNPCHandler
{
    public override int Type => NPCID.KingSlime;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 2 * 60 * 60 && npc.velocity.LengthSquared() == 0;

    public override void OnPacify(NPC npc)
    {
        npc.playerInteraction[Main.myPlayer] = true;
        npc.NPCLoot();
        npc.Transform(ModContent.NPCType<KingSlimePacified>());
        npc.life = npc.lifeMax;
        npc.Center -= new Vector2(10, 2);
    }
}
