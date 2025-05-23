using BossForgiveness.Content.NPCs.Mechanics.Lunar.Stardust;
using Microsoft.Xna.Framework;
using NetEasy;
using System;
using Terraria;
using Terraria.DataStructures;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SendComponentModule(int whoAmI, Point slot, bool placed, CompRotation rotation) : Module
{
    private readonly int _whoAmI = whoAmI;
    private readonly Point _slot = slot;
    private readonly bool _placed = placed;
    private readonly CompRotation _placedRotation = rotation;

    protected override void Receive()
    {
        if (!Main.npc[_whoAmI].TryGetGlobalNPC<StardustPillarPacificationNPC>(out var pillar))
            return;

        Point16 slot = new(_slot.X, _slot.Y);
        pillar.components[slot].Placed = _placed;
        pillar.components[slot].PlacedRotation = _placedRotation;
        Main.npc[_whoAmI].netUpdate = true;
    }
}
