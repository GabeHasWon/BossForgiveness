using BossForgiveness.Content.NPCs.Vanilla.Enemies;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Enemies;

internal class SkeleMinerPacificationNPC : GlobalNPC
{
    private static readonly HashSet<int> ValidGrabItems = [ItemID.GoldBar, ItemID.PlatinumBar];

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.UndeadMiner;

    public override bool PreAI(NPC npc)
    {
        foreach (var item in Main.ActiveItems)
        {
            if (npc.Hitbox.Intersects(item.Hitbox) && ValidGrabItems.Contains(item.type))
            {
                npc.Pacify<SkeleMinerPacified>();
                item.active = false;
                return false;
            }
        }

        return true;
    }
}
