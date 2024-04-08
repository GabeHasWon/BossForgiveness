using NetEasy;
using System;
using Terraria;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncNPCTransformModule(int npcWho, int toType) : Module
{
    private readonly int _npcWho = npcWho;
    private readonly int _toType = toType;

    protected override void Receive()
    {
        NPC npc = Main.npc[_npcWho];
        npc.Transform(_toType);
        npc.netUpdate = true;
    }
}
