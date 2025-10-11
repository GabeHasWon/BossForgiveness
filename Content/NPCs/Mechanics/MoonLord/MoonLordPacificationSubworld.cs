using SubworldLibrary;
using System.Collections.Generic;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.WorldBuilding;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordPacificationSubworld : Subworld
{
    public override int Width => 500;
    public override int Height => 500;

    public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep)];

    protected void ResetStep(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGenerator.CurrentGenerationProgress = progress;
        Main.ActiveWorldFileData.SetSeedToRandom();
        GenVars.structures = new();

        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = Height - 100;

        Main.worldSurface = Main.spawnTileY + 5;
        Main.rockLayer = Main.spawnTileY + 15;

        for (int i = 0; i < Main.maxTilesX; ++i)
        {
            for (int j = Main.spawnTileY + 4; j < Main.maxTilesY; ++j)
            {
                Tile tile = Main.tile[i, j];
                tile.TileType = TileID.ShimmerBlock;
                tile.HasTile = true;
            }
        }
    }
}
