using Terraria.ID;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class CooledOffburnTile : ModTile, IAutoloadTileItem
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

        Main.tileBrick[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        AddMapEntry(new Color(78, 89, 96));
        
        DustType = DustID.Lead;
    }
}
