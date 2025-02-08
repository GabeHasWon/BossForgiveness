using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs;

internal class PrioritizePreAINPC : ModSystem
{
    public static bool RunningPreAI = false;
    public static Dictionary<int, Func<NPC, bool>> PreAIHooks = [];

    private static Hook NPCAIHook = null;

    public override void Load()
    {
        MethodInfo info = typeof(NPCLoader).GetMethod(nameof(NPCLoader.NPCAI));
        NPCAIHook = new Hook(info, PriorityPreAI);
    }

    public static void PriorityPreAI(Action<NPC> orig, NPC npc)
    {
        RunningPreAI = true;

        if (PreAIHooks.TryGetValue(npc.type, out Func<NPC, bool> value) && !value(npc))
        {
            NPCLoader.PostAI(npc);
            return;
        }

        RunningPreAI = false;

        orig(npc);
    }
}
