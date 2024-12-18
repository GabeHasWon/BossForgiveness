using Microsoft.Xna.Framework;
using System.Security.Permissions;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Vanilla.Enemies;

[AutoloadHead]
public class SkeleMinerPacified : ModNPC
{
    private bool hasShine = true;
    private bool hasMine = true;

    private int shineTime = 0;
    private int mineTime = 0;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.OldMan];
        NPCID.Sets.NoTownNPCHappiness[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Tim);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.netAlways = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = NPCID.Steampunker;
    }

    public override bool PreAI()
    {
        shineTime--;
        mineTime--;

        if (shineTime == 0)
            hasShine = true;

        if (mineTime == 0)
            hasMine = true;

        return true;
    }

    public override void SetChatButtons(ref string button, ref string button2)
    {
        bool isGold = Main.GameUpdateCount % 360 > 180;
        int itemId = isGold ? ItemID.GoldOre : ItemID.PlatinumOre;
        string name = Lang.GetItemNameValue(itemId);
        Color color = isGold ? Color.Gold : Color.Silver;

        button = hasShine ? $"Shine (15 [c/{color.Hex3()}:{name}])" : "";
        button2 = hasMine ? $"Mine (15 [c/{color.Hex3()}:{name}])" : "";
    }

    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        int id = ItemID.GoldOre;
        int count = Main.LocalPlayer.CountItem(id);

        if (count < 5)
        {
            id = ItemID.PlatinumOre;
            count = Main.LocalPlayer.CountItem(id);
        }

        if (count >= 5)
        {
            Main.npcChatText = Language.GetTextValue("Mods.BossForgiveness.Dialogue.SkeleMiner." + (firstButton ? "Shine." : "Mine.") + Main.rand.Next(4));
            Main.LocalPlayer.AddBuff(firstButton ? BuffID.Shine : BuffID.Mining, 5 * 60 * 60);

            for (int i = 0; i < 15; ++i)
            {
                Main.LocalPlayer.ConsumeItem(id);
            }

            (firstButton ? ref hasShine : ref hasMine) = false;
            (firstButton ? ref shineTime : ref mineTime) = 60 * 60 * 10;
        }
        else
        {
            Main.npcChatText = Language.GetTextValue("Mods.BossForgiveness.Dialogue.SkeleMiner.NoOres." + Main.rand.Next(4));
        }
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.SkeleMiner." + (!hasShine && !hasMine ? "NoBuffs." : "") + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public override void SaveData(TagCompound tag)
    {
        tag.Add("mineTime", mineTime);
        tag.Add("shineTime", shineTime);
    }

    public override void LoadData(TagCompound tag)
    {
        mineTime = tag.GetInt("mineTime");
        shineTime = tag.GetInt("shineTime");
    }
}
