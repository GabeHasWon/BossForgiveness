using BossForgiveness.Content.NPCs.Mechanics;
using NetEasy;
using System;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncGolemTaserModule(int golemWhoAmI, byte taserCount) : Module
{
    private readonly int _golemWhoAmI = golemWhoAmI;
    private readonly int _taserCount = taserCount;

    protected override void Receive()
    {
        Main.npc[_golemWhoAmI].GetGlobalNPC<GolemPacificationNPC>().taserCount = _taserCount;

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
