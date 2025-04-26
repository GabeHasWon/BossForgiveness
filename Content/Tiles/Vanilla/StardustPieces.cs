using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace BossForgiveness.Content.Tiles.Vanilla;

internal class StardustPieces : ModTile
{
    public override bool IsLoadingEnabled(Mod mod) => false;

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoAttach[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.addTile(Type);

        LocalizedText name = CreateMapEntryName();
        AddMapEntry(new Color(138, 226, 255), name);

        //DustType = DustID.Star;
        HitSound = SoundID.Dig;
        AdjTiles = [TileID.Containers];
    }

    public override IEnumerable<Item> GetItemDrops(int i, int j)
    {
        yield return new Item(ItemID.RottenChunk, Main.rand.Next(3));
    }

    public override void NumDust(int i, int j, bool fail, ref int num) => num = 3;
}
