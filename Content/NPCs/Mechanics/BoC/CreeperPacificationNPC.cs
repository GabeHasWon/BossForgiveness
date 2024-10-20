using System.IO;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.BoC;

internal class CreeperPacificationNPC : GlobalNPC
{
    private const int MaxRage = 4;

    public override bool InstancePerEntity => true;

    public int rage = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Creeper;

    public override bool PreAI(NPC npc)
    {
        if (NPC.crimsonBoss == -1)
            return true;

        if (rage > 100)
            rage = 100;

        NPC parent = Main.npc[NPC.crimsonBoss];
        bool sleepyParent = parent.active && parent.type == NPCID.BrainofCthulhu && parent.GetGlobalNPC<BoCPacificationNPC>().sleepyness >= BoCPacificationNPC.MaxSleepy;

        if (rage <= MaxRage)
            npc.position += npc.velocity * (rage * 0.25f);

        if (sleepyParent)
        {
            if (npc.noTileCollide && Collision.SolidCollision(npc.position, npc.width, npc.height))
                return true;

            npc.noTileCollide = true;
            npc.velocity.Y += 0.05f;
            return false;
        }

        return true;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) => binaryWriter.Write((byte)rage);
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) => rage = binaryReader.ReadByte();
}
