using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BossForgiveness.Content.Tiles.Vanilla;

internal class RottenGrowth : ModTile
{
    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
        TileObjectData.newTile.Origin = new Point16(0, 1);
        TileObjectData.newTile.CoordinateHeights = [16, 18];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.RandomStyleRange = 3;
        TileObjectData.addTile(Type);

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Color(116, 94, 97), name);

        DustType = DustID.Corruption;
        HitSound = SoundID.Dig;
        AdjTiles = [TileID.Containers];
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        yield return new Item(ItemID.RottenChunk, Main.rand.Next(3));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;
}

public class RottenGrowthRandomUpdate : GlobalTile
{
    public override void RandomUpdate(int i, int j, int type)
    {
        if (type == TileID.Ebonstone && Main.rand.NextBool(120) && !Main.tile[i, j - 1].HasTile)
            WorldGen.PlaceObject(i, j - 1, ModContent.TileType<RottenGrowth>(), true, Main.rand.Next(3));
    }
}
