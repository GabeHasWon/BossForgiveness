using BossForgiveness.Content.NPCs.Vanilla.Enemies;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Enemies;

internal class PinkyPacificationNPC : GlobalNPC
{
    private static readonly HashSet<int> ValidGrabItems = [ItemID.LesserHealingPotion, ItemID.HealingPotion];

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.BlueSlime;

    public override bool PreAI(NPC npc)
    {
        if (npc.netID != NPCID.Pinky)
            return true;

        foreach (var item in Main.ActiveItems)
        {
            if (npc.Hitbox.Intersects(item.Hitbox) && (ValidGrabItems.Contains(item.type) || ItemID.Sets.IsFood[item.type]))
            {
                npc.Pacify<PinkyPacified>();
                item.active = false;
                return false;
            }
        }

        return true;
    }
}
