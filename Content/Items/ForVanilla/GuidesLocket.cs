using BossForgiveness.Content.NPCs.Mechanics.MoonLord;
using BossForgiveness.Content.NPCs.Mechanics.WoF;
using BossForgiveness.Content.Systems.Syncing;
using BossForgiveness.Content.Tiles.Vanilla.MoonLord;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
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
        //int x = (int)(Main.MouseWorld.X / 16f);
        //int y = (int)(Main.MouseWorld.Y / 16f);
        //WorldGen.PlaceTile(x, y, ModContent.TileType<VolatileWatcher>());

        //Tile watcher = Main.tile[x, y];

        //if (watcher.HasTile && watcher.TileType == ModContent.TileType<VolatileWatcher>())
        //{
        //    PriorityQueue<VolatileWatcher.VolatileWatcherTE.Direction, float> queue = new();

        //    MoonLordPacificationSubworld.SetWatcherValues(queue, x, y);
        //}

        //return true;

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