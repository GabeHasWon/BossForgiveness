using Terraria.ID;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class CooledEmberTile : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

        Main.tileBrick[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        AddMapEntry(new Color(174, 75, 85));
        
        DustType = DustID.RedStarfish;
    }

    public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
        if (Main.rand.NextBool(30) && !WorldGen.SolidOrSlopedTile(i, j + 1))
        {
            Dust.NewDustPerfect(new Vector2(i, j).ToWorldCoordinates(), ModContent.DustType<EmberDrip>(), new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), 1), 150);
        }
    }
}
