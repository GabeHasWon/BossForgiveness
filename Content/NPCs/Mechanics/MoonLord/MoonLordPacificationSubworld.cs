using BossForgiveness.Common;
using BossForgiveness.Content.Tiles.Vanilla.MoonLord;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.IO;
using Terraria.Localization;
using Terraria.WorldBuilding;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordPacificationSubworld : Subworld
{
    public override int Width => 1500;
    public override int Height => 1200;

    public override List<GenPass> Tasks => [new PassLegacy("Reset", ResetStep)];

    private static readonly List<string> StatusTexts = [];

    protected void ResetStep(GenerationProgress progress, GameConfiguration configuration)
    {
        WorldGenerator.CurrentGenerationProgress = progress;
        GenVars.structures = new();

        Main.spawnTileX = Main.maxTilesX / 2;
        Main.spawnTileY = Height - 100;

        Main.worldSurface = Main.maxTilesY - 5;
        Main.rockLayer = Main.maxTilesY - 2;
        
        Main.rand = new();
        FastNoiseLite noise = new(Main.rand.Next());
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

        for (int i = 0; i < Main.maxTilesX; ++i)
        {
            for (int j = 4; j < Main.maxTilesY - 4; ++j)
            {
                float x = i;
                float y = j;
                noise.DomainWarp(ref x, ref y);
                Tile tile = Main.tile[i, j];
                float value = noise.GetNoise(x, y);

                if (y > Main.maxTilesY - 300)
                {
                    bool canTile = true;

                    if (value > 0.3f)
                    {
                        if (value > 0.9f)
                        {
                            tile.TileType = TileID.ShimmerBrick;
                            tile.HasTile = true;
                        }
                        else
                            tile.WallType = WallID.ShimmerBlockWall;
                    }
                    else if (noise.GetNoise(x + 500, y + 500) < -0.6f)
                        tile.WallType = WallID.ShimmerBrickWall;
                    else
                        canTile = false;

                    if (canTile && (!tilesPlacedByX.TryGetValue(i, out int count) || count < 5))
                    {
                        tile.HasTile = true;
                        tile.TileType = (ushort)ModContent.TileType<OffburnTile>();
                        tilesPlacedByX.TryAdd(i, 0);
                        tilesPlacedByX[i]++;
                    }
                }
            }

            progress.Set(i / (double)Main.maxTilesX);
        }

        progress.Message = Language.GetTextValue("Mods.BossForgiveness.Generation.Wait");

        for (int i = 1; i < Main.maxTilesX - 1; ++i)
        {
            for (int j = 1; j < Main.maxTilesY - 1; ++j)
            {
                if (Main.rand.NextBool())
                    Tile.SmoothSlope(i, j, false);
            }

            progress.Set(i / (double)Main.maxTilesX);
        }
    }

    public override void DrawMenu(GameTime gameTime)
    {
        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-20, -20, Main.screenWidth + 40, Main.screenHeight + 40), Color.White);

        if (!StatusTexts.Contains(Main.statusText))
        {
            StatusTexts.Add(Main.statusText);

            if (StatusTexts.Count > 20)
                StatusTexts.RemoveAt(0);
        }

        float count = 0;
        float opacity = 0.05f;

        foreach (string text in StatusTexts)
        {
            var pos = new Vector2(20, 20 + count * 30);
            float scale = 0.5f;

            if (text.StartsWith('('))
            {
                scale = 0.8f;
            }

            Main.spriteBatch.DrawString(FontAssets.DeathText.Value, text, pos, Color.Black * MathF.Min(opacity += 0.05f, 1), 0f, Vector2.Zero, scale, SpriteEffects.None, 0);

            count += scale * 2;
        }
    }
}
