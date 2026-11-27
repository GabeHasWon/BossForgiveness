using BossForgiveness.Content.Tiles.Vanilla.MoonLord;
using Terraria.ID;
using Terraria.ObjectData;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class PacificationGeneration
{
    public static void Generation(float progress)
    {
        for (int i = 10; i < Main.maxTilesX - 10; ++i)
        {
            for (int j = MoonLordPacificationSubworld.StillnessLayer - 20; j < MoonLordPacificationSubworld.StillnessLayer + 20; ++j)
            {
                if (Main.rand.NextBool((int)MathHelper.Lerp(50, 30, progress)))
                {
                    PlaceObject(i, j, ModContent.TileType<PeaceTree>(), true, progress switch
                    {
                        < 0.33f => 0,
                        < 0.67f => 2,
                        _ => 4,
                    } + Main.rand.Next(2));
                }

                if (Main.rand.NextFloat() < progress * 0.5f)
                    PlaceObject(i, j, ModContent.TileType<PeaceGrass>(), true, Main.rand.Next(6));

                if (Main.rand.NextFloat() < progress * 0.5f - 0.25f)
                    PlaceObject(i, j, ModContent.TileType<PeaceTallgrass>(), true, Main.rand.Next(5));
            }
        }
    }

    public static void PlaceObject(int x, int y, int type, bool mute = false, int style = 0, int alternate = 0, int random = -1, int direction = -1)
    {
        Point size;
        Point sync = new(x, y);

        if (TileObjectData.GetTileData(type, 0, 0) is TileObjectData data)
        {
            size = new Point(data.Width, data.Height);
            sync.X -= data.Origin.X;
            sync.Y -= data.Origin.Y;
        }
        else
            size = new Point(1, 1);

        WorldGen.PlaceObject(x, y, type, mute, style, alternate, random, direction);

        if (Main.netMode == NetmodeID.Server)
            NetMessage.SendTileSquare(-1, sync.X, sync.Y, size.X, size.Y, TileChangeType.None);
    }
}
