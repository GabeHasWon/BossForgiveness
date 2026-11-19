using BossForgiveness.Content.NPCs.Mechanics;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class PeaceGrass : ModTile
{
    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        // TileID.Sets.SwaysInWindBasic[Type] = true;

        Main.tileFrameImportant[Type] = true;

        // TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [18];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.AnchorInvalidTiles = [127];
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.RandomStyleRange = 6;
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(43, 86, 196));
        
        DustType = DustID.ShimmerSplash;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) => spriteEffects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        const int Height = 16;
        const int HeightMod = Height + 2;

        SpriteEffects flip = SpriteEffects.None;
        SetSpriteEffects(i, j, ref flip);
        float wind = Main.instance.TilesRenderer.GetWindGridPush(i, j, 30, 1) * 0.04f + MathF.Sin((float)Main.instance.TilesRenderer._grassWindCounter * 6 + i + j) * 0.14f;
        Vector2 pos = TileExtensions.DrawPosition(i, j) + new Vector2(0, Height + Math.Abs(wind * 3)) + new Vector2(0, 2);
        Rectangle src = TileExtensions.BasicFrame(i, j) with { Height = HeightMod };
        Vector2 origin = src.Size() * new Vector2(0.5f, 1f);
        spriteBatch.Draw(TextureAssets.Tile[Type].Value, pos, src, TileExtensions.FadeLight(i, j), wind, origin, 1f, flip, 0);

        float fac = PacificationTracker.MoonLordBeatFactor;
        if (fac > 0)
            spriteBatch.Draw(TextureAssets.Tile[Type].Value, pos, src with { Y = HeightMod + 2 }, TileExtensions.FadeLight(i, j) * fac, wind, origin, 1f, flip, 0);

        return false;
    }
}
