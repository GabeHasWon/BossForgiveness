using BossForgiveness.Content.NPCs.Mechanics.WoF;
using NetEasy;
using System;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncSpawnAngryWoFModule(int playerWho) : Module
{
    private readonly int _playerWho = playerWho;

    protected override void Receive()
    {
        if (Main.netMode == NetmodeID.Server)
        {
            NPC.SpawnWOF(Main.player[_playerWho].Center);
            NPC wof = Main.npc[NPC.FindFirstNPC(NPCID.WallofFlesh)];
            wof.GetGlobalNPC<WoFPacificationNPC>().isAngry = true;
        }
    }
}
