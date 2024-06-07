using BossForgiveness.Content.NPCs.Mechanics.BoC;
using NetEasy;
using System;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncSleepyBoCModule(int brainWho) : Module
{
    private readonly int _brainWho = brainWho;

    protected override void Receive()
    {
        NPC npc = Main.npc[_brainWho];
        npc.GetGlobalNPC<BoCPacificationNPC>().sleepyness++;

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}

[Serializable]
public class SyncSpikedCreeperModule(int creeperWho) : Module
{
    private readonly int _creeperWho = creeperWho;

    protected override void Receive()
    {
        NPC npc = Main.npc[_creeperWho];
        npc.GetGlobalNPC<CreeperPacificationNPC>().rage++;

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}