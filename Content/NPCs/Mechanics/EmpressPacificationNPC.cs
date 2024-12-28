using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class EmpressPacificationNPC : GlobalNPC, ICustomBarNPC
{
    public const int LightMax = 6000;

    public override bool InstancePerEntity => true;

    private int light = 0;

    public void AddLight() => light++;

    public override bool PreAI(NPC npc)
    {
        if (light >= LightMax && Main.netMode != NetmodeID.MultiplayerClient)
        {
            npc.Pacify<EmpressPacified>();
            return false;
        }

        npc.GetGlobalNPC<SpeedUpBehaviourNPC>().behaviourSpeed += light / (float)LightMax * 0.5f;
        return true;
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = light;
        barMax = LightMax;

        return npc.life >= npc.lifeMax;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) => binaryWriter.Write((short)light);
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) => light = binaryReader.ReadInt16();
}