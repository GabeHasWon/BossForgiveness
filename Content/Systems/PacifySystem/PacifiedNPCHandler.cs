using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem;

internal abstract class PacifiedNPCHandler : ILoadable
{
    public static Dictionary<int, PacifiedNPCHandler> Handlers = [];

    public static bool TransformingNPC = false;

    public abstract int Type { get; }

    public virtual void Load(Mod mod)
    {
        Handlers.Add(Type, this);
    }

    public void Unload() { }

    public abstract bool CanPacify(NPC npc);
    public abstract void OnPacify(NPC npc);

    public static void TransformInto<T>(NPC npc, Vector2? offset = null) where T : ModNPC
    {
        offset ??= Vector2.Zero;

        TransformingNPC = true;

        npc.playerInteraction[Main.myPlayer] = true;
        npc.NPCLoot();
        npc.Transform(ModContent.NPCType<T>());
        npc.GivenName = string.Empty;
        npc.life = npc.lifeMax;
        npc.Center -= offset.Value;

        TransformingNPC = false;
    }
}
