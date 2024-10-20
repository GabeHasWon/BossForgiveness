using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

internal class PlanteraPacificationNPC : GlobalNPC
{
    public const int MaxPacificationsNeeded = 10;

    public override bool InstancePerEntity => true;

    public static bool CanPacify(NPC npc) => npc.life == npc.lifeMax || npc.GetGlobalNPC<PlanteraPacificationNPC>().midHealth && npc.life == npc.lifeMax / 2;

    public bool midHealth = false;

    internal byte pacification = 1;

    short _flowerTimer = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Plantera;

    public override bool PreAI(NPC npc)
    {
        if (!CanPacify(npc))
            return true;

        _flowerTimer++;

        if (pacification >= MaxPacificationsNeeded - 3)
        {
            npc.life = npc.lifeMax / 2;
            midHealth = true;
        }

        if (_flowerTimer > 360)
            ShootFlower(npc);

        return true;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write(_flowerTimer);
        binaryWriter.Write(pacification);
        bitWriter.WriteBit(midHealth);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        _flowerTimer = binaryReader.ReadInt16();
        pacification = binaryReader.ReadByte();
        midHealth = bitReader.ReadBit();
    }

    private void ShootFlower(NPC npc)
    {
        float reduction = MathHelper.Clamp(_flowerTimer - 360, 0, 100f) / 100f;
        npc.position -= npc.velocity * reduction;

        if (_flowerTimer == 460 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            int type = Main.rand.Next(4) switch // Main.rand.Next(2) switch
            {
                0 => ModContent.ProjectileType<LilyProjectile>(),
                1 => ModContent.ProjectileType<PoppyProjectile>(),
                2 => ModContent.ProjectileType<BlackPoppyProjectile>(),
                _ => ModContent.ProjectileType<RoseProjectile>(),
            };

            Vector2 velocity = npc.DirectionTo(Main.player[npc.target].Center) * 6;
            Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, velocity, type, 0, 0f, Main.myPlayer);
            _flowerTimer = 0;

            npc.velocity -= velocity;
            npc.netUpdate = true;

            for (int i = 0; i < 16; ++i)
                Dust.NewDustPerfect(npc.Center, DustID.JungleGrass, velocity.RotatedByRandom(0.2f) * 8);
        }
    }
}
