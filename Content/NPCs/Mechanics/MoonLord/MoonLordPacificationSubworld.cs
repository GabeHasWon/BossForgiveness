using BossForgiveness.Common;
using BossForgiveness.Content.Tiles.Vanilla.MoonLord;
using BossForgiveness.Content.Walls;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.Utilities;
using Terraria.WorldBuilding;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordPacificationSubworld : Subworld
{
    private static int OffburnLayer => Main.maxTilesY - 300;
    private static int FalloutLayer => Main.maxTilesY - 600;

    private static ref UnifiedRandom Random => ref Main._rand;

    public override int Width => 1500;
    public override int Height => 1200;

    private static Dictionary<int, int> LowYByX = [];

    public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("FalloutLanding", FalloutStep)];


    private static readonly List<string> StatusTexts = [];

    private void FalloutStep(GenerationProgress progress, GameConfiguration configuration)
    {
        FastNoiseLite noise = new(Random.Next());
        noise.SetFrequency(0.006f);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
        noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
        noise.SetDomainWarpAmp(-500);

        progress.Message = "(Can you see this?)\nGenerating burnlayer";

        float noiseAmp = 20;
        int tileId = TileID.ShimmerBlock;
        int currentPillarX = 0;
        int currentPillarY = 0;

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                Tile tile = Main.tile[x, y];

                float warpedX = x;
                float warpedY = y;
                noise.DomainWarp(ref warpedX, ref warpedY);

                int floorY = (int)(FalloutLayer + (noise is null ? 0 : noise.GetNoise(warpedX, 0) * noiseAmp));

                if (y <= floorY)
                    continue; // Stop tiles from being placed above the floor

                float warpedFloor = floorY + 80 * noise.GetNoise(warpedX + 300, warpedY + 300);

                if (y > floorY + 1)
                {
                    if (y < warpedFloor || y < floorY + 120 * noise.GetNoise(x + 9000, y) || y < floorY + 5)
                    {
                        tile.HasTile = true;
                        tile.TileType = (ushort)tileId;

                        float value = noise.GetNoise(warpedX, warpedY);

                        if (value < 0f)
                        {
                            tile.WallType = noise.GetNoise(warpedX * 1.4f + 3000, warpedY * 1.4f) < 0f ? WallID.ShimmerBlockWall : OffburnWallId();
                        }
                        else
                        {
                            tile.WallType = WallID.None;
                        }
                    }
                    else if (y < warpedFloor + 5 || y < floorY + 160 * noise.GetNoise(x + 12000, y) || y < floorY + 15 + Math.Abs(noise.GetNoise(x, y + 3000)) * 5)
                    {
                        tile.HasTile = true;
                        tile.TileType = (ushort)ModContent.TileType<CooledOffburnTile>();
                    }
                }
            }

            int dif = Math.Abs(x - currentPillarX);

            if (dif > 5 && !WorldGen.genRand.NextBool(30))
            {
                if (dif == 6) // needs to be changed, breaking through solid tiles atm
                    TryExpandPillar(x - 1, LowYByX[x - 1] - 1, false, false);

                continue;
            }

            int startY = LowYByX[x] - 1;
            bool firstOf = false;

            if (currentPillarX == 0)
            {
                currentPillarX = x;
                firstOf = true;
            }
            else if (Math.Abs(startY - currentPillarY) > 4)
            {
                currentPillarX = 0;
                continue;
            }

            currentPillarY = startY;

            while (!WorldGen.SolidTile(x, startY))
            {
                Tile tile = Main.tile[x, startY];
                tile.WallType = OffburnWallId();

                startY--;
            }

            if (firstOf)
            {
                TryExpandPillar(x, LowYByX[x] - 1, false, true);
            }
        }

        CleanWallsAboveBurnlayer();
    }

    private static void CleanWallsAboveBurnlayer()
    {
        for (int i = 0; i < Main.maxTilesX; ++i)
        {
            for (int j = 0; j < Main.maxTilesY; ++j)
            {
                Tile tile = Main.tile[i, j];
                tile.WallType = WallID.None;

                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    break;
                }
            }
        }
    }

    private static void TryExpandPillar(int x, int y, bool places, bool left)
    {
        bool hasPlaced = false;
        int origY = y;

        if (x < 10 || x > Main.maxTilesX - 10)
            return;

        if (y < 10)
            return;

        while (!WorldGen.SolidTile(x, y) && y > 10)
        {
            if (places)
            {
                Tile tile = Main.tile[x, y];
                tile.WallType = OffburnWallId();
            }

            y--;

            if (Math.Abs(y - origY) > 10 && WorldGen.genRand.NextBool(8) && !hasPlaced)
            {
                TryExpandPillar(x + (left ? -1 : 1), y, true, left);
                hasPlaced = true;
            }
        }
    }

    protected void ResetStep(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGenerator.CurrentGenerationProgress = progress;
        GenVars.structures = new();

        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = Height - 100;

        Main.worldSurface = Main.maxTilesY - 5;
        Main.rockLayer = Main.maxTilesY - 2;

        Random = new UnifiedRandom((int)DateTime.Now.ToBinary());
        FastNoiseLite noise = new(Random.Next());
        noise.SetFrequency(0.02f);
        noise.SetDomainWarpAmp(195f);
        noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2Reduced);

        progress.Message = Language.GetTextValue("Mods.BossForgiveness.Generation.Basics");

        for (int i = 0; i < Main.maxTilesX; ++i)
        {
            for (int j = (int)(Main.spawnTileY + 4 + noise.GetNoise(i, 30) * 12); j < Main.maxTilesY; ++j)
            {
                Tile tile = Main.tile[i, j];
                tile.TileType = (ushort)ModContent.TileType<OffburnTile>();
                tile.HasTile = true;
            }

            progress.Set(i / (double)Main.maxTilesX);
        }

        Dictionary<int, int> tilesPlacedByX = [];
        Dictionary<int, int> lowYByX = [];

        for (int i = 0; i < Main.maxTilesX; ++i)
        {
            for (int j = 4; j < Main.maxTilesY - 4; ++j)
            {
                float x = i;
                float y = j;
                noise.DomainWarp(ref x, ref y);
                Tile tile = Main.tile[i, j];
                float value = noise.GetNoise(x, y);

                if (y > OffburnLayer)
                {
                    bool canTile = true;

                    if (value > 0.3f)
                    {
                        if (value > 0.8f)
                        {
                            tile.TileType = (ushort)ModContent.TileType<CooledOffburnTile>();
                            tile.HasTile = true;
                        }
                        else
                            tile.WallType = OffburnWallId();
                    }
                    else if (noise.GetNoise(x + 500, y + 500) < -0.6f)
                        tile.WallType = OffburnWallId();
                    else
                        canTile = false;

                    if (canTile && (!tilesPlacedByX.TryGetValue(i, out int count) || count < 5))
                    {
                        tile.HasTile = true;
                        tile.TileType = (ushort)ModContent.TileType<OffburnTile>();
                        tilesPlacedByX.TryAdd(i, 0);
                        tilesPlacedByX[i]++;

                        lowYByX.TryAdd(i, j);
                        lowYByX[i] = Math.Min(lowYByX[i], j);
                    }
                }
            }

            progress.Set(i / (double)Main.maxTilesX);
        }

        LowYByX = lowYByX;

        progress.Message = Language.GetTextValue("Mods.BossForgiveness.Generation.Wait");

        for (int i = 1; i < Main.maxTilesX - 1; ++i)
        {
            for (int j = 1; j < Main.maxTilesY - 1; ++j)
            {
                if (!Random.NextBool(3))
                    Tile.SmoothSlope(i, j, false);
            }

            progress.Set(i / (double)Main.maxTilesX);
        }

        HashSet<Point16> vines = [];

        for (int i = 0; i < Main.maxTilesX; ++i)
        {
            for (int j = OffburnLayer; j < Main.maxTilesY - 20; ++j)
            {
                Tile tile = Main.tile[i, j];
                bool isOffburn = tile.TileType == ModContent.TileType<OffburnTile>() || tile.TileType == ModContent.TileType<CooledOffburnTile>();

                if (tile.HasTile && isOffburn)
                {
                    if (Random.NextBool(12) && !WorldGen.SolidTile(i, j + 1))
                    {
                        int height = Random.Next(2, 13);

                        for (int y = j + 1; y < j + height; ++y)
                        {
                            Tile vine = Main.tile[i, y];
                            vine.HasTile = true;
                            vine.TileType = (ushort)ModContent.TileType<MoltenOffburn>();
                            vines.Add(new Point16(i, y));
                        }
                    }
                    else if (Random.NextBool(6) && !WorldGen.SolidTile(i, j - 1))
                    {
                        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<OddPlants>(), true, style: Main.rand.Next(3));
                    }
                }
            }
        }

        foreach (Point16 pos in vines)
        {
            WorldGen.TileFrame(pos.X, pos.Y, true);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static ushort OffburnWallId() => (ushort)(Random.NextBool(12) ? ModContent.WallType<EmberOffburnWall>() : ModContent.WallType<OffburnWall>());

    public override void DrawMenu(GameTime gameTime)
    {
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-20, -20, Main.screenWidth + 40, Main.screenHeight + 40), Color.White);
        DrawStatusText(true);
    }

    internal static void DrawStatusText(bool addStatusText, float baseOpacity = 1f)
    {
        GenerationProgress progress = WorldGenerator.CurrentGenerationProgress;
        string status = Main.statusText;

        if (WorldGen.gen && progress is not null)
        {
            status = progress.Message;
        }

        foreach (string line in status.Split('\n'))
        {
            if (addStatusText && !StatusTexts.Contains(line))
            {
                StatusTexts.Add(line);

                if (StatusTexts.Count > 24)
                    StatusTexts.RemoveAt(0);
            }
        }

        float count = 0;
        float opacity = 0.05f;

        foreach (string text in StatusTexts)
        {
            var pos = new Vector2(20, 20 + count * 30);
            float scale = 0.5f;
            Color color = Color.Black * MathF.Min(opacity += 0.05f, 1) * baseOpacity;

            if (text.StartsWith('('))
            {
                scale = 0.45f;
                color *= 0.2f;
            }

            Main.spriteBatch.DrawString(FontAssets.DeathText.Value, text, pos, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0);

            count += scale * 2;
        }
    }
}
