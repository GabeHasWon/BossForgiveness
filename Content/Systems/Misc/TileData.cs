using Microsoft.Xna.Framework;
using System.Diagnostics.CodeAnalysis;
using Terraria;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.Systems.Misc;

public readonly struct TileData(int type, Point frame)
{
    public readonly int Type = type;
    public readonly Point Frame = frame;

    public TileData(Tile tile) : this(tile.TileType, new Point(tile.TileFrameX, tile.TileFrameY)) { }

    public void SaveData(TagCompound tag)
    {
        tag.Add(nameof(Type), Type);
        tag.Add(nameof(Frame), Frame);
    }

    public static TileData LoadData(TagCompound tag)
    {
        int type = tag.GetInt(nameof(Type));
        Point frame = tag.Get<Point>(nameof(Frame));

        return new(type, frame);
    }

    public override int GetHashCode() => Type.GetHashCode() + Frame.GetHashCode();
    public override bool Equals([NotNullWhen(true)] object obj) => obj is TileData data && data == this;
    public static bool operator ==(TileData data, TileData other) => data.Type == other.Type && data.Frame == other.Frame;
    public static bool operator !=(TileData data, TileData other) => data.Type != other.Type || data.Frame != other.Frame;
}