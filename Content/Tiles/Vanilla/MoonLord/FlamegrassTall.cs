using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class FlamegrassTall : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        Main.tileFrameImportant[Type] = true;
        Main.tileLighted[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [40];
        TileObjectData.newTile.CoordinateWidth = 20;
        TileObjectData.newTile.AnchorInvalidTiles = [127];
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.DrawYOffset = -20;
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(255, 255, 147));
        
        DustType = DustID.Torch;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => (r, g, b) = (0.3f, 0.1f, 0.25f);
    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) => spriteEffects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
}
