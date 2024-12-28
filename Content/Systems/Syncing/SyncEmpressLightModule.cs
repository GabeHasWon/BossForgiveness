using BossForgiveness.Content.NPCs.Mechanics;
using NetEasy;
using System;
using Terraria;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncEmpressLightModule(int whoAmI) : Module
{
    private readonly int _whoAmI = whoAmI;

    protected override void Receive()
    {
        if (Main.npc[_whoAmI].TryGetGlobalNPC<EmpressPacificationNPC>(out var empress))
            empress.AddLight();

        Main.npc[_whoAmI].netUpdate = true;
    }
}
