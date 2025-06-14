using BossForgiveness.Content.NPCs.Mechanics.Lunar.Nebula;
using NetEasy;
using System;
using Terraria;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncNebulaLink(byte who, short npcWho) : Module
{
    protected override void Receive()
    {
        NPC npc = Main.npc[npcWho];

        if (npc.TryGetGlobalNPC(out NebulaLinkNPC neb))
            Main.player[who].GetModPlayer<NebulaLinkPlayer>().AddConnection(npc, neb);

        if (Main.dedServ)
            Send();
    }
}
