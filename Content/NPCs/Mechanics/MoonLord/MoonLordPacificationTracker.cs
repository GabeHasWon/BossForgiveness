using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordPacificationTracker : ModSystem
{
    internal const int MaxTeleportTimer = 10 * 40;

    public static HashSet<int> LunarTowersPacified = [];
    public static int TeleportTimer = 0;
    public static int[] PlayerReadyTimer = new int[Main.maxPlayers];

    internal static bool SpawnedAlready = false;

    public override void Load()
    {
        On_LegacyPlayerRenderer.DrawPlayerFull += DrawPlayerFullMod;
        On_Main.Draw += DrawWhiteout;
        On_Main.Update += Timer;
    }

    private void Timer(On_Main.orig_Update orig, Main self, GameTime gameTime)
    {
        if (Main.mouseRight && Main.mouseMiddle)
            TeleportTimer = 1;

        if (TeleportTimer > 0 && SubworldSystem.Current is null)
        {
            TeleportTimer +=
#if DEBUG
                5;
#else
                1;
#endif
            if (TeleportTimer == 2)
            {
                foreach (Player plr in Main.ActivePlayers)
                {
                    if (plr.mount.Active)
                        plr.QuickMount();
                }
            }
        }

        orig(self, gameTime);

        TeleportTimer = Math.Clamp(TeleportTimer, 0, MaxTeleportTimer);
    }

    private void DrawPlayerFullMod(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Camera camera, Player drawPlayer)
    {
        if (TeleportTimer > 0 && SubworldSystem.Current is not MoonLordPacificationSubworld)
            return;

        orig(self, camera, drawPlayer);
    }

    private void DrawWhiteout(On_Main.orig_Draw orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);

        const float HalfMax = MaxTeleportTimer / 2f;

        if (TeleportTimer > HalfMax && !Main.gameMenu)
        {
            Main.spriteBatch.Begin();

            float adjTimer = (TeleportTimer - HalfMax) / HalfMax;
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Vector2(-10), new Rectangle(0, 0, 10000, 10000), Color.White * adjTimer);

            if (SubworldSystem.Current is MoonLordPacificationSubworld)
                MoonLordPacificationSubworld.DrawStatusText(false, adjTimer);

            Main.spriteBatch.End();
        }
    }

    public override void PreUpdateEntities()
    {
        if (SubworldSystem.Current is not MoonLordPacificationSubworld)
            return;

        bool valid = true;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (!plr.active)
                continue;
            
            ref int timer = ref PlayerReadyTimer[plr.whoAmI];

            if (plr.Center.Y / 16 < MoonLordPacificationSubworld.StillnessLayer)
                timer++;
            else
                timer = Math.Max(0, timer - 2);

            if (timer <= 1200)
                valid = false;
        }

        if (valid && Main.netMode != NetmodeID.MultiplayerClient && !SpawnedAlready)
        {
            SpawnedAlready = true;

            Vector2 center = new(0, 60000);

            foreach (Player plr in Main.ActivePlayers)
            {
                if (plr.Center.Y < center.Y)
                    center = plr.Center;
            }

            NPC.NewNPC(new EntitySource_SpawnNPC(), (int)center.X, (int)center.Y - 1200, NPCID.MoonLordCore);
        }
    }

    public override void SaveWorldData(TagCompound tag) => tag.Add("towers", LunarTowersPacified.ToArray());
    public override void LoadWorldData(TagCompound tag) => LunarTowersPacified = [.. tag.GetIntArray("towers")];

    public static void AddPacification(int type)
    {
        LunarTowersPacified.Add(type);

        if (LunarTowersPacified.Count == 4)
            TeleportTimer++;
    }
}

public class MoonLordEmptyPlayer : ModPlayer
{
    public override bool CanUseItem(Item item) => MoonLordPacificationTracker.TeleportTimer <= 0;

    public override void PreUpdateMovement()
    {
        if (MoonLordPacificationTracker.TeleportTimer > 0 && SubworldSystem.Current is not MoonLordPacificationSubworld)
            Player.velocity = Vector2.Zero;

        if (MoonLordPacificationTracker.TeleportTimer >= MoonLordPacificationTracker.MaxTeleportTimer && SubworldSystem.Current is null)
        {
            SubworldSystem.Enter<MoonLordPacificationSubworld>();
        }
        else if (SubworldSystem.Current is MoonLordPacificationSubworld)
        {
            MoonLordPacificationTracker.TeleportTimer -= 8;
        }
    }
}