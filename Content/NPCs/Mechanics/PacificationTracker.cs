using System;
using System.Collections.Generic;
using System.IO;
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
    internal Dictionary<int, PacificationType> PacifiedIDs = [];

    /// <summary>
    /// Unloaded modded pacifications, kept to not lose data.
    /// </summary>
    internal List<string> UnloadedPacifications = [];

    /// <summary>
    /// Adds the boss to the pacification tracker.
    /// </summary>
    internal static bool AddBoss(int id, PacificationType pacification) => ModContent.GetInstance<PacificationTracker>().PacifiedIDs.TryAdd(id, pacification);

    internal static bool HasBoss(int id) => ModContent.GetInstance<PacificationTracker>().PacifiedIDs.ContainsKey(id);

    internal static int Count(PacificationType type)
    {
        int count = 0;

        foreach (var pair in ModContent.GetInstance<PacificationTracker>().PacifiedIDs)
            if (pair.Value.HasFlag(type))
                count++;

        return count;
    }

    public override void SaveWorldData(TagCompound tag)
    {
        List<(string name, byte type)> bossesAll = [.. PacifiedIDs.Select(x => (x.Key < NPCID.Count ? "Terraria/" + x.Key : ModContent.GetModNPC(x.Key).FullName, (byte)x.Value))];
        List<string> bosses = [.. bossesAll.Select(x => x.name)];
        List<byte> types = [.. bossesAll.Select(x => x.type)];

        bosses.AddRange(UnloadedPacifications);

        tag.Add("bosses", (string[])[.. bosses]);
        tag.Add("types", (byte[])[.. types]);
    }

    public override void LoadWorldData(TagCompound tag)
    {
        PacifiedIDs.Clear();
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

            PacifiedIDs.Add(id, type);
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((short)PacifiedIDs.Count);

        foreach (var pair in PacifiedIDs)
        {
            writer.Write((short)pair.Key);
            writer.Write((byte)pair.Value);
        }
    }

    public override void NetReceive(BinaryReader reader)
    {
        PacifiedIDs.Clear();
        int count = reader.ReadInt16();

        for (int i = 0; i < count; ++i)
            PacifiedIDs.Add(reader.ReadInt16(), (PacificationType)reader.ReadByte());
    }

    public override void ClearWorld()
    {
        PacifiedIDs.Clear();
        UnloadedPacifications.Clear();
    }
}
