using BossForgiveness.Content.Systems.Misc;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class QueenBeeRequirements
{
    public readonly struct DisplayTileData(TileData data, int itemId, bool skipFrameCheck, bool skipEntirely = false)
    {
        public readonly TileData Data = data;
        public readonly int ItemId = itemId;
        public readonly bool SkipFrameCheck = skipFrameCheck;
        public readonly bool SkipEntirely = skipEntirely;

        public void SaveData(TagCompound tag)
        {
            TagCompound dataTag = [];
            Data.SaveData(dataTag);
            tag.Add(nameof(Data), dataTag);
            tag.Add(nameof(ItemId), ItemId);
            tag.Add(nameof(SkipFrameCheck), SkipFrameCheck);
            tag.Add(nameof(SkipEntirely), SkipEntirely);
        }

        public static DisplayTileData LoadData(TagCompound tag)
        {
            var data = TileData.LoadData(tag.GetCompound(nameof(Data)));
            int itemId = tag.GetInt(nameof(ItemId));
            bool skipFrameCheck = tag.GetBool(nameof(SkipFrameCheck));
            bool skipEntirely = tag.GetBool(nameof(SkipEntirely));

            return new DisplayTileData(data, itemId, skipFrameCheck, skipEntirely);
        }

        public void NetSend(BinaryWriter writer)
        {
            writer.Write(Data.Type);
            writer.Write((short)Data.Frame.X);
            writer.Write((short)Data.Frame.Y);
            writer.Write((short)ItemId);
            writer.Write(SkipFrameCheck);
            writer.Write(SkipEntirely);
        }

        public static DisplayTileData NetRecieve(BinaryReader reader)
        {
            int dataType = reader.ReadInt32();
            var dataFrame = new Point(reader.ReadInt16(), reader.ReadInt16());
            int itemId = reader.ReadInt16();
            bool skipFrameCheck = reader.ReadBoolean();
            bool skipEntirely = reader.ReadBoolean();

            return new DisplayTileData(new TileData(dataType, dataFrame), itemId, skipFrameCheck, skipEntirely);
        }

        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is not DisplayTileData other)
            {
                if (obj is TileData data)
                    return !SkipFrameCheck ? data == Data : data.Type == Data.Type;

                return false;
            }

            return other.SkipFrameCheck || SkipFrameCheck ? other.Data.Type == Data.Type : other.Data == Data;
        }
    }

    public readonly List<DisplayTileData> desiredDatas = [];

    public QueenBeeRequirements(bool skipInit)
    {
        if (skipInit)
            return;

        DisplayTileData[] datas = 
            [   new(new(TileID.Bottles, new Point(126, 0)), ItemID.HoneyCup, false), 
                new(new(TileID.Containers, new Point(1044, 0)), ItemID.HoneyChest, false), 
                new(new(TileID.HoneyDispenser, default), ItemID.HoneyDispenser, true),
                new(new(TileID.GoldCoinPile, default), ItemID.GoldCoin, true), 
                new(new(TileID.MetalBars, new Point(108, 0)), ItemID.GoldBar, false),
                new(new(TileID.Books, default), ItemID.Book, true)
            ];

        for (int i = 0; i < 6; ++i)
            desiredDatas.Add(Main.rand.Next(datas));

        desiredDatas.Add(new DisplayTileData(new(TileID.Tables, new Point(1026, 18)), ItemID.HoneyTable, false, true));
        desiredDatas.Add(new DisplayTileData(new(TileID.Chairs, new Point(880, 18)), ItemID.HoneyChair, false, true));
    }

    public bool RequirementsSatisfied(List<TileData> data)
    {
        bool hasTable = false;
        bool hasChair = false;

        List<DisplayTileData> checkData = new(desiredDatas.Count);

        foreach (var item in desiredDatas)
            checkData.Add(item);

        checkData.RemoveAll(x => x.SkipEntirely);

        foreach (var item in data)
        {
            if (!hasTable && item.Type == TileID.Tables && item.Frame.X == 1026)
                hasTable = true;

            if (!hasChair && item.Type == TileID.Chairs && item.Frame.Y == 880)
                hasChair = true;

            if (checkData.Any(x => x.Equals(item)))
                checkData.Remove(checkData.First(x => x.Equals(item)));
        }

        return hasTable && hasChair && checkData.Count == 0;
    }

    private static bool CheckDataIsHoneyLight(TileData tile)
    {
        if (tile.Type == TileID.Candles && tile.Frame.Y == 484)
            return true;

        if (tile.Type == TileID.Candelabras && tile.Frame.Y == 144)
            return true;

        return tile.Type == TileID.Chandeliers && tile.Frame.Y == 540 && tile.Frame.X < 110;
    }

    public void SaveData(TagCompound tag)
    {
        tag.Add("dataCount", desiredDatas.Count);

        for (int i = 0; i < desiredDatas.Count; ++i)
        {
            TagCompound data = [];
            desiredDatas[i].SaveData(data);
            tag.Add("data" + i, data);
        }
    }

    public static QueenBeeRequirements LoadData(TagCompound tag)
    {
        QueenBeeRequirements requirements = new(true);
        int count = tag.GetInt("dataCount");

        for (int i = 0; i < count; ++i)
        {
            TagCompound data = tag.GetCompound("data" + i);
            requirements.desiredDatas.Add(DisplayTileData.LoadData(data));
        }

        return requirements;
    }

    public void NetSend(BinaryWriter writer)
    {
        writer.Write((byte)desiredDatas.Count);

        for (int i = 0; i < desiredDatas.Count; ++i)
            desiredDatas[i].NetSend(writer);
    }

    public static QueenBeeRequirements NetRecieve(BinaryReader reader)
    {
        int count = reader.ReadByte();
        QueenBeeRequirements requirements = new(true);

        for (int i = 0; i < count; ++i)
            requirements.desiredDatas.Add(DisplayTileData.NetRecieve(reader));

        return requirements;
    }
}