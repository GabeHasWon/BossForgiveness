using BossForgiveness.Content.Systems.Misc;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class QueenBeeRequirements
{
    public readonly struct DisplayTileData(TileData data, LocalizedText display, int itemId, bool skipFrameCheck)
    {
        public readonly TileData Data = data;
        public readonly LocalizedText DisplayName = display;
        public readonly int ItemId = itemId;
        public readonly bool SkipFrameCheck = skipFrameCheck;

        public void SaveData(TagCompound tag)
        {
            TagCompound dataTag = [];
            Data.SaveData(dataTag);
            tag.Add(nameof(Data), dataTag);
            tag.Add(nameof(DisplayName), DisplayName.Key);
            tag.Add(nameof(ItemId), ItemId);
            tag.Add(nameof(SkipFrameCheck), SkipFrameCheck);
        }

        public static DisplayTileData LoadData(TagCompound tag)
        {
            var data = TileData.LoadData(tag.GetCompound(nameof(Data)));
            LocalizedText name = Language.GetText(tag.GetString(nameof(DisplayName)));
            int itemId = tag.GetInt(nameof(ItemId));
            bool skipFrameCheck = tag.GetBool(nameof(SkipFrameCheck));

            return new DisplayTileData(data, name, itemId, skipFrameCheck);
        }
    }

    public readonly List<DisplayTileData> desiredDatas = [];

    public QueenBeeRequirements(bool skipInit)
    {
        DisplayTileData[] datas = 
            [   new(new(TileID.Bottles, new Point(126, 0)), Lang.GetItemName(ItemID.HoneyCup), ItemID.HoneyCup, false), 
                new(new(TileID.Containers, new Point(1044, 0)), Lang.GetItemName(ItemID.HoneyChest), ItemID.HoneyChest, false), 
                new(new(TileID.HoneyDispenser, default), Lang.GetItemName(ItemID.HoneyDispenser), ItemID.HoneyDispenser, true),
                new(new(TileID.GoldCoinPile, default), Lang.GetItemName(ItemID.GoldCoin), ItemID.GoldCoin, true), 
                new(new(TileID.MetalBars, new Point(108, 0)), Lang.GetItemName(ItemID.GoldBar), ItemID.GoldBar, false)
            ];

        for (int i = 0; i < 5; ++i)
            desiredDatas.Add(Main.rand.Next(datas));
    }

    public bool RequirementsSatisfied(List<TileData> data)
    {
        bool hasTable = false;
        bool hasChair = false;
        bool hasHoneyLight = false;

        List<DisplayTileData> checkData = new(desiredDatas.Count);

        foreach (var item in desiredDatas)
            checkData.Add(item);

        foreach (var item in data)
        {
            if (!hasTable && item.Type == TileID.Tables && item.Frame.X == 1026)
                hasTable = true;

            if (!hasChair && item.Type == TileID.Chairs && item.Frame.Y == 880)
                hasChair = true;

            if (!hasHoneyLight && CheckDataIsHoneyLight(item))
                hasHoneyLight = true;

            if (checkData.Any(x => x.Data == item))
                checkData.Remove(checkData.First(x => x.Data == item));
        }

        return hasTable && hasChair && hasHoneyLight && checkData.Count == 0;
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
}