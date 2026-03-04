using BossForgiveness.Common;
using Terraria.ID;
using Terraria.ObjectData;
using Terraria.Utilities;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

#nullable enable

internal class SkewTree : ModTile
{
    public static void Grow(int x, int y, int height = 20, UnifiedRandom? random = null)
    {
        random ??= WorldGen.genRand;
        int direction = random.NextBool() ? 1 : -1;

        for (int j = y; j > y - height; j--)
        {
            Tile tile = Main.tile[x, j];

            if (j == y - height + 1)
            {
                int left = random.Next(11, 21);
                int right = random.Next(11, 21);

                for (int i = x - left; i < x + right; ++i)
                {
                    tile = Main.tile[i, j];
                    PlaceSkewTreeTile(random, i == x ? 72 : 90, tile);
                }

                return;
            }

            if (random.NextBool(2))
                direction *= -1;

            PlaceSkewTreeTile(random, direction == 1 ? 0 : 54, tile);

            x += direction;

            tile = Main.tile[x, j];
            PlaceSkewTreeTile(random, direction == 1 ? 18 : 36, tile);
        }
    }

    private static void PlaceSkewTreeTile(UnifiedRandom random, int frameX, Tile tile)
    {
        if (tile.HasTile)
            return;

        tile.HasTile = true;
        tile.TileType = (ushort)ModContent.TileType<SkewTree>();
        tile.TileFrameX = (short)frameX;
        tile.TileFrameY = (short)(random.Next(3) * 18);
    }

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileSolid[Type] = true;

        TileID.Sets.DrawsWalls[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(135, 83, 109));
    }
}
