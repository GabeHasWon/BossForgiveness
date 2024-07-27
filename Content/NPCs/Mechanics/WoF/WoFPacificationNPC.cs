using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.WoF;

internal class WoFPacificationNPC : GlobalNPC
{
    public const int MaxPetrify = 3;

    public override bool InstancePerEntity => true;

    public bool isAngry = false;
    public int leechTimer = 0;
    public int leechCount = 0;

    internal int petrifyCount = 0;

    private int _time = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.WallofFlesh;

    public override bool PreAI(NPC npc)
    {
        if (!isAngry)
            return true;

        float lifeFactor = petrifyCount / (float)MaxPetrify;
        npc.velocity.X *= MathHelper.Lerp(1.25f, 3.25f, lifeFactor);
        npc.scale = 1.1f + MathF.Sin(_time++ * 0.025f) * 0.1f;

        if (leechTimer++ > MathHelper.Lerp(400f, 200f, lifeFactor))
        {
            SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);

            leechCount++;
            leechTimer = 0;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int leech = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<SpiritLeech>());
                Main.npc[leech].velocity = npc.velocity;

                if (leechCount >= 2)
                {
                    leechCount = 0;
                    (Main.npc[leech].ModNPC as SpiritLeech).isSpirit = true;
                    Main.npc[leech].netUpdate = true;
                }
            }
        }

        if (Main.rand.NextBool((int)MathHelper.Lerp(400f, 50f, lifeFactor)))
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone);

        return true;
    }

    internal void AddPacification(NPC npc)
    {
        petrifyCount++;
        npc.netUpdate = true;
    }
    
    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        bitWriter.WriteBit(isAngry);
        binaryWriter.Write(petrifyCount);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        isAngry = bitReader.ReadBit();
        petrifyCount = binaryReader.ReadInt32();
    }
}
