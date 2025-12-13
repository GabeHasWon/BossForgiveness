using BossForgiveness.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel.DataAnnotations;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class MoltenOffburn : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [16, 16];
        TileObjectData.newTile.AnchorInvalidTiles = [127];
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorAlternateTiles = [Type];
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(43, 86, 196));
        
        DustType = DustID.Lead;
    }

    public override void PlaceInWorld(int i, int j, Item item)
    {
        bool _ = true;
        TileFrame(i, j, ref _, ref _);
    }

    public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
    {
        Tile tile = Main.tile[i, j];
        Tile below = Main.tile[i, j + 1];

        if ((!below.HasTile || !Main.tileSolid[below.TileType]) && below.TileType != Type)
            tile.TileFrameX = 18;
        else
            tile.TileFrameX = 0;

        tile.TileFrameY = (short)(Main.rand.Next(3) * 18);
        return false;
    }

    public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
        if (Main.rand.NextBool(70))
        {
            TrySpawnDust(i, j - 1, new Vector2(0, -1), true);
            TrySpawnDust(i, j + 1, new Vector2(0, 1), true);
            TrySpawnDust(i + 1, j, new Vector2(1, 0), false);
            TrySpawnDust(i - 1, j, new Vector2(-1, 0), false);
        }
    }

    private static void TrySpawnDust(int i, int j, Vector2 direction, bool isUp)
    {
        if (WorldGen.SolidOrSlopedTile(i, j))
            return;

        float range = Main.rand.NextFloat(16);
        Vector2 position = new Vector2(i, j).ToWorldCoordinates(isUp ? range : -direction.X * 8, !isUp ? range : -direction.Y * 8);
        Vector2 velocity = new(RandomizeAxis(direction.X), RandomizeAxis(direction.Y));

        Dust.NewDustPerfect(position, ModContent.DustType<OffburnTile.OffburnDust>(), velocity);
    }

    private static float RandomizeAxis(float x) => x == 0 ? Main.rand.NextFloat(-1, 1) : x * Main.rand.NextFloat(2, 6);

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        float xMod = 1f;

        for (int y = j - 1; y > j - 4; --y)
        {
            Tile tile = Main.tile[i, y];

            if (!tile.HasTile || tile.TileType != Type)
            {
                int dif = j - y;
                xMod = (dif - 1) / 3f;
                break;
            }
        }

        Vector2 pos = TileExtensions.DrawPosition(i, j);
        pos.X += StaticNoise.GetNoise(i, j * 2 + StaticNoise.GetNoise(i, j * 8 + 2500) + Main.GameUpdateCount * 0.5f, StaticNoise.NoiseType.WebCellular) * 16 * xMod;

        Rectangle src = TileExtensions.BasicFrame(i, j);
        spriteBatch.Draw(TextureAssets.Tile[Type].Value, pos, src, TileExtensions.FadeLight(i, j));
        return false;
    }
}
