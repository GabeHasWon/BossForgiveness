using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordPacificationTracker : ModSystem
{
    internal const int MaxTeleportTimer = 10 * 40;

    public static HashSet<int> LunarTowersPacified = [];
    public static int TeleportTimer = 0;

    public override void Load()
    {
        On_LegacyPlayerRenderer.DrawPlayerFull += DrawPlayerFullMod;
        On_Main.Draw += DrawWhiteout;
        On_Main.Update += Timer;
    }

    private void Timer(On_Main.orig_Update orig, Main self, GameTime gameTime)
    {
        if (Main.mouseRight && Main.mouseMiddle && Main.mouseLeft)
            TeleportTimer = 1;

        if (TeleportTimer > 0 && SubworldSystem.Current is null)
        {
            TeleportTimer++;

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
    }

    private void DrawPlayerFullMod(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Camera camera, Player drawPlayer)
    {
        if (TeleportTimer > 0)
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

            Main.spriteBatch.End();
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
    public override void PreUpdateMovement()
    {
        if (MoonLordPacificationTracker.TeleportTimer > 0)
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