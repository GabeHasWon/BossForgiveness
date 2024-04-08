using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using NetEasy;
using System;
using Terraria;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncPlanteraHookModule(int plant, int hookId, Vector2 position) : Module
{
    private readonly short _plant = (short)plant;
    private readonly byte _hookId = (byte)hookId;
    private readonly Vector2 _position = position;

    protected override void Receive()
    {
        var hook = (Main.npc[_plant].ModNPC as PlanteraPacified).planteraHooks[_hookId];
        hook.dummy.ai[0] = _position.X;
        hook.dummy.ai[1] = _position.Y;
    }
}
