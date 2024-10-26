using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.WorldBuilding;

namespace BossForgiveness.Content.NPCs.Mechanics.Mech;

internal class MechBossPacificationNPC : GlobalNPC, ICustomBarNPC
{
    internal class Modifiers
    {
        public static readonly Modifiers Default = new();
        
        public float Speed = 0;
        public float Damage = 1;
    }

    public const int MaxStun = 12;

    public override bool InstancePerEntity => true;

    private readonly Modifiers _modifiers = Modifiers.Default;

    public int stunCount = 0;
    public int stunCooldown = 0;
    public bool electrified = false;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => IsValidEntity(entity);

    public override bool PreAI(NPC npc)
    {
        NPC parent = GetParent(npc);

        if (parent is null || !parent.TryGetGlobalNPC<MechBossPacificationNPC>(out var pacParent))
            return true;

        npc.GetGlobalNPC<SpeedUpBehaviourNPC>().behaviourSpeed += pacParent._modifiers.Speed;
        return true;
    }

    public override void PostAI(NPC npc)
    {
        stunCooldown--;

        NPC parent = GetParent(npc);

        if (parent is not null && parent.TryGetGlobalNPC<MechBossPacificationNPC>(out var pacParent))
        {
            if (pacParent.electrified && Main.rand.NextBool(parent.type == NPCID.TheDestroyer ? 120 : 10))
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);
        }
        else
            npc.active = false;
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (GetParent(npc).GetGlobalNPC<MechBossPacificationNPC>().electrified)
            target.AddBuff(BuffID.Electrified, 4 * 60);
    }

    public static bool IsValidEntity(NPC entity)
    {
        int[] types = [NPCID.Spazmatism, NPCID.Retinazer, NPCID.SkeletronPrime, NPCID.PrimeCannon, NPCID.PrimeLaser, NPCID.PrimeSaw, NPCID.PrimeVice,
            NPCID.TheDestroyer, NPCID.TheDestroyerBody, NPCID.TheDestroyerTail];

        return types.Contains(entity.type);
    }

    public static NPC GetParent(NPC npc)
    {
        if (npc.type is NPCID.Spazmatism or NPCID.Retinazer or NPCID.SkeletronPrime or NPCID.TheDestroyer)
            return npc;

        if (npc.type is NPCID.PrimeCannon or NPCID.PrimeLaser or NPCID.PrimeSaw or NPCID.PrimeVice)
        {
            for (int i = npc.whoAmI; i >= 0; i--)
            {
                if (Main.npc[i].active && Main.npc[i].type == NPCID.SkeletronPrime)
                    return Main.npc[i];
            }

            return null;
        }

        // Only remaining options are the Destroyer segments
        for (int i = npc.whoAmI; i >= 0; i--)
        {
            if (Main.npc[i].active && Main.npc[i].type == NPCID.TheDestroyer)
                return Main.npc[i];
        }

        return null;
    }

    public static void ModifyModifiers(NPC npc, float speedMod, float damageMod)
    {
        NPC parent = GetParent(npc);

        if (!parent.TryGetGlobalNPC<MechBossPacificationNPC>(out var pac))
            return;

        if (pac.stunCooldown > 0)
            return;

        for (int i = 0; i < 20; ++i)
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Electric);

        if (!Main.rand.NextBool(8))
        {
            Modifiers modifiers = pac._modifiers;
            modifiers.Speed += speedMod;
            modifiers.Damage += damageMod;

            if (modifiers.Speed < 0.5f)
                modifiers.Speed = 0.5f;

            if (modifiers.Damage < 0.1f)
                modifiers.Damage = 0.1f;
        }
        else
        {
            pac.electrified = true;
        }

        pac.stunCooldown = 60;
        pac.stunCount++;
        parent.netUpdate = true;

        if (parent.whoAmI != npc.whoAmI)
        {
            if (npc.type is NPCID.TheDestroyerTail or NPCID.TheDestroyerBody)
            {
                for (int i = parent.whoAmI; i < Main.maxNPCs; ++i)
                {
                    NPC segment = Main.npc[i];

                    if (segment.type != NPCID.TheDestroyerBody || segment.type != NPCID.TheDestroyerTail || !segment.TryGetGlobalNPC<MechBossPacificationNPC>(out var seg))
                        break;

                    CopyPacifiedValues(pac, seg);
                    segment.netUpdate = true;
                }
            }
            else if (npc.TryGetGlobalNPC<MechBossPacificationNPC>(out var selfPac))
                CopyPacifiedValues(pac, selfPac);
        }
    }

    private static void CopyPacifiedValues(MechBossPacificationNPC parent, MechBossPacificationNPC self)
    {
        self._modifiers.Speed = parent._modifiers.Speed;
        self._modifiers.Damage = parent._modifiers.Damage;
        self.electrified = parent.electrified;
        self.stunCooldown = parent.stunCooldown;
        self.stunCount = parent.stunCount;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)stunCount);
        binaryWriter.Write((byte)stunCooldown);
        binaryWriter.Write((Half)_modifiers.Speed);
        binaryWriter.Write((Half)_modifiers.Damage);
        bitWriter.WriteBit(electrified);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        stunCount = binaryReader.ReadByte();
        stunCooldown = binaryReader.ReadByte();
        _modifiers.Speed = (float)binaryReader.ReadHalf();
        _modifiers.Damage = (float)binaryReader.ReadHalf();
        electrified = bitReader.ReadBit();
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = stunCount;
        barMax = npc.type is NPCID.TheDestroyer or NPCID.TheDestroyerBody or NPCID.TheDestroyerTail ? MaxStun * 5 : MaxStun;
        return true;
    }
}
