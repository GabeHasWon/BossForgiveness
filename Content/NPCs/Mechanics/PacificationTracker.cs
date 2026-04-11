using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics;

public class PacificationTracker : ModSystem
{
    [Flags]
    public enum PacificationType : byte
    {
        None = 0,
        Boss = 1,
        Miniboss = 1 << 2,
        Enemy = 1 << 3,
        Event = 1 << 4,

        Other = 1 << 7
    }

    /// <summary>
    /// % of bosses pacified out of the vanilla 17 (non-event, non-Moon Lord, and BoC/Eow combined into one) bosses.
    /// </summary>
    public static float MoonLordBeatFactor => Count(PacificationType.Boss) / 16f;

    /// <summary>
    /// All current or former pacifications in the world.
    /// </summary>
    internal Dictionary<int, PacificationType> PacifiedBosses = [];

    /// <summary>
    /// Unloaded modded pacifications, kept to not lose data.
    /// </summary>
    internal List<string> UnloadedPacifications = [];

    /// <summary>
    /// Adds the boss to the pacification tracker.
    /// </summary>
    internal static bool AddBoss(int id, PacificationType pacification) => ModContent.GetInstance<PacificationTracker>().PacifiedBosses.TryAdd(id, pacification);

    internal static bool HasBoss(int id) => ModContent.GetInstance<PacificationTracker>().PacifiedBosses.ContainsKey(id);

    internal static int Count(PacificationType type)
    {
        int count = 0;

        foreach (var pair in ModContent.GetInstance<PacificationTracker>().PacifiedBosses)
            if (pair.Value.HasFlag(type))
                count++;

        return count;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        List<(string name, byte type)> bossesAll = [.. PacifiedBosses.Select(x => (x.Key < NPCID.Count ? "Terraria/" + x.Key : ModContent.GetModNPC(x.Key).FullName, (byte)x.Value))];
        List<string> bosses = [.. bossesAll.Select(x => x.name)];
        List<byte> types = [.. bossesAll.Select(x => x.type)];

        bosses.AddRange(UnloadedPacifications);

        tag.Add("bosses", (string[])[.. bosses]);
        tag.Add("types", (byte[])[.. types]);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        PacifiedBosses.Clear();
        string[] bosses = tag.Get<string[]>("bosses");
        byte[] types = tag.GetByteArray("types");

        for (int i = 0; i < bosses.Length; i++)
        {
            string boss = bosses[i];
            var type = (PacificationType)types[i];

            int id;

            if (boss.StartsWith("Terraria/"))
                id = int.Parse(boss.Replace("Terraria/", ""));
            else
            {
                if (ModContent.TryFind(boss, out ModNPC npc))
                    id = npc.Type;
                else
                {
                    UnloadedPacifications.Add(boss);
                    continue;
                }
            }

            PacifiedBosses.Add(id, type);
        }
    }

    public override void ClearWorld()
    {
        PacifiedBosses.Clear();
        UnloadedPacifications.Clear();
    }
}
