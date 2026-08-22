using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordAttacksNPC : GlobalNPC
{
    public abstract class CustomAttack : ModType
    {
        public static int AttackCount { get; private set; }

        public static readonly Dictionary<int, int[]> RequirementsById = [];

        private int InternalID;

        public abstract int[] PacifiedRequirements { get; }

        public static int Id<T>() where T : CustomAttack => ModContent.GetInstance<T>().InternalID;

        protected override void Register()
        {
            ModTypeLookup<CustomAttack>.Register(this);

            InternalID = AttackCount++;
        }

        public override void SetupContent()
        {
            SetStaticDefaults();

            RequirementsById.Add(InternalID, PacifiedRequirements);
        }

        public bool Available()
        {
            foreach (int type in RequirementsById[InternalID])
                if (!PacificationTracker.HasBoss(type))
                    return false;

            return true;
        }

        public CustomAttack Clone() => (CustomAttack)MemberwiseClone();

        public virtual void Update(NPC npc)
        {
        }
    }

    public override bool InstancePerEntity => true;

    private readonly List<CustomAttack> applicableAttacks = [];

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.MoonLordCore;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (SubworldSystem.Current is null)
            return;

        IEnumerable<CustomAttack> attacks = ModContent.GetContent<CustomAttack>();

        foreach (CustomAttack attack in attacks)
        {
            if (attack.Available())
                applicableAttacks.Add(attack.Clone());
        }
    }

    public override void AI(NPC npc)
    {
         foreach (var attack in applicableAttacks)
            attack.Update(npc);
    }
}

internal class MoonLordPacificationNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public float Progress => PacifiedBosses.Count / (float)PacificationTracker.VanillaBossesForMoonLord;

    public readonly HashSet<int> PacifiedBosses = [];

    private static List<int> UnpacifiedBosses = [];

    public static List<int> GetUnpacifiedBosses()
    {
        UnpacifiedBosses.Clear();

        foreach (int id in PacificationTracker.VanillaIdsForMoonLord)
            UnpacifiedBosses.Add(id);

        return UnpacifiedBosses;
    }

    public static MoonLordPacificationNPC GetPacificationNPC()
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.type == NPCID.MoonLordCore)
                return npc.GetGlobalNPC<MoonLordPacificationNPC>();
        }

        return null;
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type is NPCID.MoonLordCore or NPCID.MoonLordHand or NPCID.MoonLordHead or NPCID.MoonLordFreeEye;

    public override bool PreAI(NPC npc)
    {
        if (npc.life < npc.lifeMax || AnyHandHurt())
        {
            if (SubworldSystem.Current is MoonLordPacificationSubworld)
            {
                npc.active = false;
                return false;
            }
        }

        if (false)
        {
            bool isHeadOrHand = npc.type is NPCID.MoonLordHead or NPCID.MoonLordHand;

            if (isHeadOrHand)
            {
                npc.ai[0] = 0;
                npc.ai[1] = 0;
            }
            else
            {
                npc.velocity *= 0.9f;
            }

            return npc.type is NPCID.MoonLordHead or NPCID.MoonLordHand;
        }

        return true;
    }

    private bool AnyHandHurt()
    {
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (npc.type != NPCID.MoonLordHand)
                continue;

            if (npc.life != npc.lifeMax)
                return true;
        }

        return false;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (EnlightenedMoonlordTarget.DrawingSpecialML)
            return true;

        return false;
    }
}