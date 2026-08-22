using Terraria.DataStructures;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

internal class EntitySource_Memory(Memory mem, string context = "") : IEntitySource
{
    string IEntitySource.Context => context;

    public readonly Memory Mem = mem;
}
