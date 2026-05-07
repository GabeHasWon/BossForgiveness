using SubworldLibrary;
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

    List<CustomAttack> applicableAttacks = [];

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
