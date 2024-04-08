using BossForgiveness.Content.NPCs.Mechanics;
using NetEasy;
using System;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncEoWMorselModule(int eowWhoAmI, int projToKill) : Module
{
    private readonly int _eowWhoAmI = eowWhoAmI;
    private readonly int _projToKill = projToKill;

    protected override void Receive()
    {
        WormPacificationNPC.AddFoodToHead(Main.npc[_eowWhoAmI]);
        Main.projectile[_projToKill].Kill();

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
