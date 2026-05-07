using Terraria.ID;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class EldritchFleshTile : ModTile, IAutoloadTileItem
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

        Main.tileBrick[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = false;

        AddMapEntry(new Color(158, 15, 25));
        
        DustType = DustID.Torch;
    }
}
