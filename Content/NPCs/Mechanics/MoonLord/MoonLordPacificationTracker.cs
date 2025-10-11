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
    internal const int MaxTeleportTimer = 10 * 60;

    public static HashSet<int> LunarTowersPacified = [];
    public static int TeleportTimer = 0;

    public override void Load()
    {
        On_LegacyPlayerRenderer.DrawPlayerFull += DrawPlayerFullMod;
        On_Main.Draw += DrawBlackout;
    }

    private void DrawPlayerFullMod(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Camera camera, Player drawPlayer)
    {
        if (TeleportTimer > 0)
            return;

        orig(self, camera, drawPlayer);
    }

    private void DrawBlackout(On_Main.orig_Draw orig, Main self, GameTime gameTime)
    {
        orig(self, gameTime);

        const float HalfMax = MaxTeleportTimer / 2f;

        if (TeleportTimer > HalfMax)
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

    public override void PreUpdatePlayers()
    {
        if (Main.mouseRight && Main.mouseMiddle && Main.mouseLeft)
            TeleportTimer = 1;

        if (TeleportTimer > 0)
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
            MoonLordPacificationTracker.TeleportTimer = 0;
            SubworldSystem.Enter<MoonLordPacificationSubworld>();
        }
    }
}