using BossForgiveness.Common;
using BossForgiveness.Content.Tiles.Vanilla.MoonLord;
using BossForgiveness.Content.Walls;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
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
    internal static int OffburnLayer => Main.maxTilesY - 300;
    internal static int EmberLayer => Main.maxTilesY - 600;
    internal static int StillnessLayer => Main.maxTilesY - 1300;

    public override int Width => 1500;
    public override int Height => 1900;

    internal bool HasSpawnedBoss = false;

    private static Dictionary<int, int> LowYByX = [];
    private static ref UnifiedRandom Random => ref Main._rand;

    public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep), new PassLegacy("Fallout", FalloutStep), new PassLegacy("Stillness", StillnessStep)];

    private static readonly List<string> StatusTexts = [];

    public override void CopyMainWorldData()
    {
        var bosses = ModContent.GetInstance<PacificationTracker>().PacifiedBosses;
        int[] ids = [.. bosses.Select(x => x.Key)];
        byte[] types = [.. bosses.Select(x => (byte)x.Value)];

        SubworldSystem.CopyWorldData("pacIDs", ids);
        SubworldSystem.CopyWorldData("pacTypes", types);
    }

    public override void ReadCopiedMainWorldData()
    {
        int[] ids = SubworldSystem.ReadCopiedWorldData<int[]>("pacIDs");
        byte[] types = SubworldSystem.ReadCopiedWorldData<byte[]>("pacTypes");

        ModContent.GetInstance<PacificationTracker>().PacifiedBosses.Clear();
        
        for (int i = 0; i < ids.Length; ++i)
            ModContent.GetInstance<PacificationTracker>().PacifiedBosses.Add(ids[i], (PacificationTracker.PacificationType)types[i]);
    }

    private void StillnessStep(GenerationProgress progress, GameConfiguration configuration)
    {
        const int StillnessHeight = 30;

        HasSpawnedBoss = false;

        FastNoiseLite noise = new(Random.Next());
        noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);

        FastNoiseLite oreNoise = new(Random.Next());
        oreNoise.SetFrequency(0.02f);

        FastNoiseLite oreNoiseType = new(Random.Next());

        int lastFlowerX = 0;

        for (int i = 2; i < Main.maxTilesX - 2; ++i)
        {
            int sineHeight = (int)(noise.GetNoise(i * 1.25f, 0) * 75) + StillnessHeight + 100;

            for (int j = StillnessLayer; j < StillnessLayer + sineHeight; ++j)
            {
                Tile tile = Main.tile[i, j];
                tile.HasTile = true;

                float factor = MathHelper.Clamp(1 - Utils.GetLerpValue(StillnessLayer, StillnessLayer + sineHeight, j, true) + Random.NextFloat(-0.05f, 0.05f), 0, 1);
                float ore = oreNoise.GetNoise(i, j);

                tile.TileType = factor switch
                {
                    < 0.33f => TileID.Dirt,
                    < 0.5f => ore <= 0.4f ? TileID.Stone : oreNoiseType.GetNoise(i, j) switch
                    {
                        < -0.5f => TileID.Copper,
                        < 0 => TileID.Iron,
                        < 0.5f => TileID.Lead,
                        _ => TileID.Tin
                    },
                    < 0.75f => TileID.Ash,
                    _ => TileID.ShimmerBlock
                };

                if (j == StillnessLayer && PacificationTracker.HasBoss(NPCID.CultistBoss) && Main.rand.NextBool(8) && i > lastFlowerX + 3)
                {
                    WorldGen.PlaceTile(i, j - 1, ModContent.TileType<CondolenceFlowers>(), true, style: Random.Next(3));
                    lastFlowerX = i;
                }
            }
        }
    }

    private void FalloutStep(GenerationProgress progress, GameConfiguration configuration)
    {
        FastNoiseLite noise = new((int)DateTime.Now.ToBinary());
        noise.SetFrequency(0.006f);
        noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
        noise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
        noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
        noise.SetDomainWarpAmp(-500);
        
        progress.Message = "Generating burnlayer";

        float noiseAmp = 20;
        int tileId = ModContent.TileType<EmberTile>();
        int currentPillarX = 0;
        int currentPillarY = 0;

        Dictionary<int, int> floorYAtX = [];

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                Tile tile = Main.tile[x, y];

                float warpedX = x;
                float warpedY = y;
                noise.DomainWarp(ref warpedX, ref warpedY);

                int floorY = (int)(EmberLayer + noise.GetNoise(warpedX, 0) * noiseAmp);

                if (y <= floorY)
                    continue; // Stop tiles from being placed above the floor

                float warpedFloor = floorY + 80 * noise.GetNoise(warpedX + 300, warpedY + 300);

                if (y > floorY + 1)
                {
                    if (y < warpedFloor || y < floorY + 120 * noise.GetNoise(x + 9000, y) || y < floorY + 5)
                    {
                        floorYAtX.TryAdd(x, y);
                        floorYAtX[x] = Math.Min(floorYAtX[x], y);

                        tile.HasTile = true;
                        tile.TileType = (ushort)tileId;

                        float value = noise.GetNoise(warpedX, warpedY);

                        if (value < 0f)
                        {
                            tile.WallType = noise.GetNoise(warpedX * 1.4f + 3000, warpedY * 1.4f) < 0f ? (ushort)ModContent.WallType<EmberWall>() : OffburnWallId();
                        }
                        else
                        {
                            tile.WallType = WallID.None;
                        }
                    }
                    else if (y < warpedFloor + 5 || y < floorY + 160 * noise.GetNoise(x + 12000, y) || y < floorY + 15 + Math.Abs(noise.GetNoise(x, y + 3000)) * 5)
                    {
                        tile.HasTile = true;
                        tile.TileType = (ushort)ModContent.TileType<CooledEmberTile>();
                    }
                }
            }

            int dif = Math.Abs(x - currentPillarX);

            if (dif > 5 && !WorldGen.genRand.NextBool(30))
            {
                if (dif == 6) // needs to be changed, breaking through solid tiles atm - update: idk bandaided lol
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
        DecorateOffburn();

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = 0; y < Main.maxTilesY; y++)
            {
                progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                Tile tile = Main.tile[x, y];

                float warpedX = x;
                float warpedY = y;
                noise.DomainWarp(ref warpedX, ref warpedY);

                int floorY = (int)(EmberLayer + (noise is null ? 0 : noise.GetNoise(warpedX, 30000) * noiseAmp));

                if (y <= floorY)
                    continue; // Stop tiles from being placed above the floor

                float warpedFloor = floorY + 80 * noise.GetNoise(warpedX + 300, warpedY + 30300);

                if (y > floorY + 1 && y < warpedFloor || y < floorY + 120 * noise.GetNoise(x * 1.75f + 9000, y * 1.75f + 30000) || y < floorY + 5)
                {
                    float value = noise.GetNoise(warpedX, warpedY);

                    if (value < 0f)
                    {
                        tile.WallType = (ushort)ModContent.WallType<EmberWall>();
                    }
                }
            }
        }

        FastNoiseLite flameNoise = new();
        flameNoise.SetNoiseType(FastNoiseLite.NoiseType.ValueCubic);
        flameNoise.SetFrequency(0.025f);
        flameNoise.SetFractalType(FastNoiseLite.FractalType.PingPong);
        flameNoise.SetFractalOctaves(2);
        flameNoise.SetFractalLacunarity(1.73f);
        flameNoise.SetFractalGain(-0.660f);
        flameNoise.SetFractalWeightedStrength(5.750f);
        flameNoise.SetFractalPingPongStrength(2f);

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            if (!floorYAtX.TryGetValue(x, out int bottomY))
                continue;

            int y = (int)(StillnessLayer + flameNoise.GetNoise(x, 0) * 30 + 120);

            for ( ; y < bottomY; y++)
            {
                progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));
                Tile tile = Main.tile[x, y];

                float v = flameNoise.GetNoise(x, y);

                if (v > -0.2f)
                {
                    tile.WallType = (ushort)ModContent.WallType<EmberWall>();
                }

                if (v > 0.3f)
                {
                    tile.TileType = (ushort)tileId;
                    tile.HasTile = true;
                }
            }
        }

        DecorateEmbers(progress);
    }

    private void DecorateEmbers(GenerationProgress progress)
    {
        PriorityQueue<VolatileWatcher.VolatileWatcherTE.Direction, float> queue = new();

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = OffburnLayer - 1; y >= StillnessLayer; y--)
            {
                progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));

                Tile tile = Main.tile[x, y];

                if (tile.HasTile && !WorldGen.SolidTile(x, y - 1) 
                    && (tile.TileType == ModContent.TileType<EmberTile>() || tile.TileType == ModContent.TileType<CooledEmberTile>() || SkewTreeTop(tile)))
                {
                    if (Random.NextBool(120))
                    {
                        WorldGen.PlaceTile(x, y - 1, ModContent.TileType<SunPlant>());

                        Tile sunPlant = Main.tile[x, y - 1];

                        if (sunPlant.HasTile && sunPlant.TileType == ModContent.TileType<SunPlant>())
                        {
                            int te = ModContent.GetInstance<SunPlant.SunPlantTE>().Place(x, y - 1);
                            (TileEntity.ByID[te] as SunPlant.SunPlantTE).Friendly = false;
                        }
                    }
                    else if (PacificationTracker.HasBoss(NPCID.EyeofCthulhu) && Random.NextBool(80))
                    {
                        WorldGen.PlaceTile(x, y - 1, ModContent.TileType<VolatileWatcher>());

                        Tile watcher = Main.tile[x, y - 1];

                        if (watcher.HasTile && watcher.TileType == ModContent.TileType<VolatileWatcher>())
                        {
                            queue.Clear();
                            SetWatcherValues(queue, x, y - 1);
                        }
                    }
                    else if (Random.NextBool(140) && x > 50 && x <= Width - 50)
                        SkewTree.Grow(x, y - 1, Random.Next(15, 30), Random);
                    else if (Random.NextBool(170))
                        GrowKelp(x, y);
                    else if (Random.NextBool(3) && !Main.tile[x, y - 1].HasTile)
                        WorldGen.PlaceObject(x, y - 1, Random.NextBool(3) ? ModContent.TileType<FlamegrassTall>() : ModContent.TileType<Flamegrass>(), true, Random.Next(6));
                }
                else if (tile.HasTile && !WorldGen.SolidTile(x, y + 1) && (tile.TileType == ModContent.TileType<EmberTile>() || tile.TileType == ModContent.TileType<CooledEmberTile>()) &&
                    PacificationTracker.HasBoss(NPCID.EyeofCthulhu))
                {
                    WorldGen.PlaceTile(x, y - 1, ModContent.TileType<VolatileWatcher>());

                    Tile watcher = Main.tile[x, y - 1];

                    if (watcher.HasTile && watcher.TileType == ModContent.TileType<VolatileWatcher>())
                    {
                        queue.Clear();
                        SetWatcherValues(queue, x, y - 1);
                    }
                }
            }
        }

        for (int x = 0; x < Main.maxTilesX; x++)
        {
            for (int y = OffburnLayer - 1; y >= StillnessLayer; y--)
            {
                progress.Set((y + x * Main.maxTilesY) / (float)(Main.maxTilesX * Main.maxTilesY));

                Tile tile = Main.tile[x, y];

                if (SkewTreeTop(tile))
                    GrowKelp(x, y);
            }
        }
    }

    internal static void SetWatcherValues(PriorityQueue<VolatileWatcher.VolatileWatcherTE.Direction, float> queue, int x, int y)
    {
        int id = ModContent.GetInstance<VolatileWatcher.VolatileWatcherTE>().Place(x, y);
        var ent = (VolatileWatcher.VolatileWatcherTE)TileEntity.ByID[id];
        queue.Clear();

        if (!WorldGen.SolidTile(x - 1, y))
            queue.Enqueue(VolatileWatcher.VolatileWatcherTE.Direction.Left, Random.NextFloat());
        
        if (!WorldGen.SolidTile(x + 1, y))
            queue.Enqueue(VolatileWatcher.VolatileWatcherTE.Direction.Right, Random.NextFloat());
        
        if (!WorldGen.SolidTile(x, y - 1))
            queue.Enqueue(VolatileWatcher.VolatileWatcherTE.Direction.Up, Random.NextFloat());
        
        if (!WorldGen.SolidTile(x, y + 1))
            queue.Enqueue(VolatileWatcher.VolatileWatcherTE.Direction.Down, Random.NextFloat());

        if (queue.Count == 0)
        {
            ModContent.GetInstance<VolatileWatcher.VolatileWatcherTE>().Kill(x, y);
            Tile tile = Main.tile[x, y];
            tile.HasTile = false;
            return;
        }

        ent.Dir = queue.Dequeue();
    }

    private static void GrowKelp(int x, int y)
    {
        int kelpHeight = Random.Next(2, 27);

        for (int k = y - 1; k > y - kelpHeight; k--)
        {
            Tile kelp = Main.tile[x, k];

            if (kelp.HasTile)
                continue;

            int frame = 2;

            if (k < y - kelpHeight * 0.75f)
                frame = 0;
            else if (k < y - kelpHeight * 0.33f)
                frame = 1;

            IKelpTile.Place<ClimbingEmbers>(x, k, frame);
        }
    }

    private static bool SkewTreeTop(Tile tile) => tile.TileType == ModContent.TileType<SkewTree>() && tile.TileFrameX >= 90;

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

                if (y < EmberLayer + 180)
                {
                    float placement = 1 - Utils.GetLerpValue(EmberLayer + 100, EmberLayer + 180, y, true);

                    if (Random.NextFloat() < placement)
                    {
                        tile.WallType = (ushort)ModContent.WallType<EmberWall>();
                    }
                }
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
    }

    private static void DecorateOffburn()
    {
        HashSet<Point16> vines = [];

        for (int i = 0; i < Main.maxTilesX; ++i)
        {
            for (int j = EmberLayer; j < Main.maxTilesY - 20; ++j)
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

                            if (vine.HasTile)
                                break;

                            vine.HasTile = true;
                            vine.TileType = (ushort)ModContent.TileType<MoltenOffburn>();
                            vines.Add(new Point16(i, y));
                        }
                    }
                    else if (Random.NextBool(6) && !WorldGen.SolidTile(i, j - 1))
                    {
                        WorldGen.PlaceTile(i, j - 1, ModContent.TileType<OddPlants>(), true, style: Random.Next(3));
                    }
                }
                else if (tile.WallType == ModContent.WallType<OffburnWall>() || tile.WallType == ModContent.WallType<EmberOffburnWall>())
                {
                    if (Random.NextBool(140))
                    {
                        WorldGen.PlaceObject(i, j, ModContent.TileType<WallFlowers2x2>(), true, Random.Next(4));
                    }
                    else if (Random.NextBool(110))
                    {
                        WorldGen.PlaceObject(i, j, ModContent.TileType<WallFlowers1x1>(), true, Random.Next(8));
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

    public override void Update()
    {
        TileEntity.UpdateStart();

        foreach (TileEntity te in TileEntity.ByID.Values)
        {
            te.Update();
        }

        TileEntity.UpdateEnd();

        if (HasSpawnedBoss)
            return;

        bool valid = true;
        int[] timers = MoonLordPacificationTracker.PlayerReadyTimer;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!plr.active)
                continue;

            ref int timer = ref timers[plr.whoAmI];

            if (plr.Center.Y / 16 < StillnessLayer)
                timer++;
            else
                timer = Math.Max(0, timer - 2);

            if (timer <= 1200)
                valid = false;
        }

        if (valid && Main.netMode != NetmodeID.MultiplayerClient)
        {
            HasSpawnedBoss = true;

            Vector2 center = new(0, 60000);

            foreach (Player plr in Main.ActivePlayers)
            {
                if (plr.Center.Y < center.Y)
                    center = plr.Center;
            }

            NPC.NewNPC(new EntitySource_SpawnNPC(), (int)center.X, (int)center.Y - 1200, NPCID.MoonLordCore);
        }
    }

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
