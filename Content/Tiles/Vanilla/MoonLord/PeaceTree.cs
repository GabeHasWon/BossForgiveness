using BossForgiveness.Content.Items.ForVanilla.MoonLord;
using System;
using Terraria.DataStructures;
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

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 1;

    public override void KillMultiTile(int i, int j, int frameX, int frameY)
    {
        int style = frameY / 146;
        int spam = style switch
        {
            0 => 1,
            1 => 3,
            _ => 6,
        };

        Range stack = style switch
        {
            0 => 1..1,
            1 => 1..2,
            _ => 2..3,
        };

        int adjI = i - frameX % 72 / 18;
        int adjJ = j - frameY % 146 / 18;

        for (int k = 0; k < spam; ++k) 
        {
            int subStack = Main.rand.Next(stack.Start.Value, stack.End.Value + 1);
            float xOff = Main.rand.NextFloat(4) * 16;
            float yOff = Main.rand.NextFloat(8) * 16;
            int item = Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(adjI, adjJ).ToWorldCoordinates(xOff, yOff), ModContent.ItemType<CreationShard>(), subStack);
            (Main.item[item].ModItem as CreationShard).Stackable = false;
        }
    }
}
