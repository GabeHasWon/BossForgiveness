using BossForgiveness.Content.NPCs.Mechanics.Lunar.Stardust;
using NetEasy;
using System;
using Terraria;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class CheckCompleteStardustModule(int who) : Module
{
    protected override void Receive() => StardustPillarPlayer.CheckCompletion(Main.npc[who]);
}
