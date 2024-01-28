using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem;

internal abstract class PacifiedNPCHandler : ILoadable
{
    public static Dictionary<int, PacifiedNPCHandler> Handlers = [];

    public abstract int Type { get; }

    public void Load(Mod mod)
    {
        Handlers.Add(Type, this);
    }

    public void Unload() { }

    public abstract bool CanPacify(NPC npc);
    public abstract void OnPacify(NPC npc);
}
