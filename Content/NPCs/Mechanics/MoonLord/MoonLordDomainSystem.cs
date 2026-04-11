using SubworldLibrary;
using System.Collections.Generic;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordDomainSystem : GlobalNPC
{
    public override void EditSpawnPool(IDictionary<int, float> pool, NPCSpawnInfo spawnInfo)
    {
        if (SubworldSystem.Current is not MoonLordPacificationSubworld)
            return;

        pool.Clear();

        if (PacificationTracker.HasBoss(NPCID.KingSlime) && spawnInfo.SpawnTileY > MoonLordPacificationSubworld.EmberLayer)
        {
            pool.Add(NPCID.Shimmerfly, 0.6f);
            pool.Add(NPCID.ShimmerSlime, 0.1f);
        }
    }

    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        if (SubworldSystem.Current is not MoonLordPacificationSubworld)
            return;

        maxSpawns *= 2;
        spawnRate /= 4;
    }
}
