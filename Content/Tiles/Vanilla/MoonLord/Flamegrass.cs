using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class Flamegrass : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [22];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.AnchorInvalidTiles = [127];
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.DrawYOffset = -6;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 6;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(255, 255, 147));
        
        DustType = DustID.Torch;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) => spriteEffects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
}
