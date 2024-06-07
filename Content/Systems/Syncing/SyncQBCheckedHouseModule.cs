using BossForgiveness.Content.NPCs.Mechanics;
using NetEasy;
using System;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncQBCheckedHouseModule(int qbWhoAmI) : Module
{
    private readonly int _qbWhoAmI = qbWhoAmI;

    protected override void Receive()
    {
        QueenBeePacificationNPC.SetChecked(_qbWhoAmI);

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
