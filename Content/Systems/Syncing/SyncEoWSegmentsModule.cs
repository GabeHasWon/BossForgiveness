using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using NetEasy;
using System;
using Terraria;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncEoWSegmentsModule(int eoWHead, Vector2[] segments) : Module
{
    private readonly int _eoWHead = eoWHead;
    private readonly Vector2[] _eowSegments = segments;

    protected override void Receive()
    {
        (Main.npc[_eoWHead].ModNPC as PacifiedEoW).SpawnBody([.._eowSegments]);
    }
}
