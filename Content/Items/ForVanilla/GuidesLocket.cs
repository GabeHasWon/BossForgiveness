using BossForgiveness.Content.NPCs.Mechanics.WoF;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class GuidesLocket : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 0;

    public override void SetDefaults()
    {
        Item.Size = new(30, 26);
        Item.noUseGraphic = true;
        Item.rare = ItemRarityID.Lime;
        Item.useTime = 30;
        Item.useAnimation = 30;
        Item.noMelee = true;
        Item.useStyle = ItemUseStyleID.HoldUp;
    }

    public override bool? UseItem(Player player)
    {
        if (NPC.AnyNPCs(NPCID.WallofFlesh) || !player.ZoneUnderworldHeight)
            return false;

        if (Main.netMode == NetmodeID.SinglePlayer)
        {
            NPC.SpawnWOF(player.Center);
            NPC wof = Main.npc[NPC.FindFirstNPC(NPCID.WallofFlesh)];
            wof.GetGlobalNPC<WoFPacificationNPC>().isAngry = true;
        }
        else if (Main.myPlayer == player.whoAmI)
            new SyncSpawnAngryWoFModule(Main.myPlayer).Send();

        return true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        ModContent.GetInstance<GuideLocketSystem>().locketGiven = false;
        Item.active = false;

        for (int i = 0; i < 20; ++i)
        {
            Vector2 speed = Main.rand.NextVector2Circular(4, 4);
            Dust.NewDust(Item.Center, 1, 1, DustID.Gold, speed.X, speed.Y);

            speed = Main.rand.NextVector2Circular(4, 4);
            Dust.NewDust(Item.Center, 1, 1, DustID.HeartCrystal, speed.X, speed.Y);
        }
    }

    public override void GrabRange(Player player, ref int grabRange) => grabRange += 400;
}