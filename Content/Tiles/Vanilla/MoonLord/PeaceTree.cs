using Terraria.ID;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class PeaceTree : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.Width = 4;
        TileObjectData.newTile.Height = 8;
        TileObjectData.newTile.CoordinateHeights = [16, 16, 16, 16, 16, 16, 16, 18];
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 2;
        TileObjectData.newTile.StyleWrapLimit = 2;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(235, 221, 245));
        
        DustType = DustID.WhiteTorch;
    }
}
