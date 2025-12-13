using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class OddPlants : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [28];
        TileObjectData.newTile.CoordinateWidth = 28;
        TileObjectData.newTile.AnchorInvalidTiles = [127];
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorAlternateTiles = [Type];
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(43, 86, 196));
        
        DustType = DustID.Lead;
    }

    public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
        if (Main.rand.NextBool(30))
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

        Dust.NewDustPerfect(position, ModContent.DustType<OffburnDust>(), velocity);
    }

    private static float RandomizeAxis(float x) => x == 0 ? Main.rand.NextFloat(-1, 1) : x * Main.rand.NextFloat(2, 6);
}
