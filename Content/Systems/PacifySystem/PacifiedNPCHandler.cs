using BossForgiveness.Content.NPCs;
using System.Collections.Generic;

namespace BossForgiveness.Content.Systems.PacifySystem;

internal abstract class PacifiedNPCHandler : ILoadable
{
    public static Dictionary<int, PacifiedNPCHandler> Handlers = [];

    /// <summary>
    /// Value used to track if an NPC is being pacified right now. Only used for pacified drops at the moment.
    /// </summary>
    public static bool TransformingNPC = false;

    public abstract int Type { get; }

    public virtual void Load(Mod mod) => Handlers.Add(Type, this);

    public void Unload() => Handlers.Remove(Type);

    public abstract bool CanPacify(NPC npc);
    public abstract void OnPacify(NPC npc);

    /// <summary>
    /// Legacy method that simply calls <see cref="NPCs.NPCUtilities.Pacify{T}(NPC)"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="npc"></param>
    /// <param name="offset"></param>
    public static void TransformInto<T>(NPC npc, Vector2? offset = null) where T : ModNPC
    {
        npc.Pacify<T>();
        return;

        //offset ??= Vector2.Zero;

        //TransformingNPC = true;

        //npc.playerInteraction[Main.myPlayer] = true;
        //npc.NPCLoot();
        //npc.Transform(ModContent.NPCType<T>());
        //npc.GivenName = string.Empty;
        //npc.life = npc.lifeMax;
        //npc.Center -= offset.Value;

        //TransformingNPC = false;
    }
}
