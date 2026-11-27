using BossForgiveness.Content.NPCs.Mechanics.MoonLord;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.UI;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar;

internal class PortalAccessLayer : ModSystem
{
    public static Asset<Texture2D> PortalTex = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/Lunar/Portal");

    private static bool _lastMouse = false;

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (SubworldSystem.Current is MoonLordPacificationSubworld || !Main.playerInventory)
            return;

        int layer = layers.FindIndex(x => x.Name == "Vanilla: Inventory");

        if (layer != -1)
        {
            layers.Insert(layer, new LegacyGameInterfaceLayer("Pacifist Route Pacification UI", () =>
            {
                DrawPortal();
                return true;
            }));
        }
    }

    private static void DrawPortal()
    {
        int x = 20;
        int y = 270;

        bool click = Main.mouseLeft && !_lastMouse;
        _lastMouse = Main.mouseLeft;

        Color color = Color.Lerp(Color.White, Color.White * 0.5f, MathF.Sin((float)Main.timeForVisualEffects * 0.02f) * 0.5f + 0.5f);
        Main.spriteBatch.Draw(PortalTex.Value, new Vector2(x, y), null, color);

        if (new Rectangle(x, y, PortalTex.Width(), PortalTex.Height()).Contains(Main.MouseScreen.ToPoint()))
        {
            Main.LocalPlayer.mouseInterface = true;

            if (click)
            {
                MoonLordPacificationTracker.TeleportTimer = 1;
                SoundEngine.PlaySound(SoundID.Thunder with { Volume = 0.7f, PitchRange = (-0.2f, 0.2f) }, Main.LocalPlayer.Center);

                for (int i = 0; i < 3; ++i)
                {
                    ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.PaladinsHammer, new()
                    {
                        MovementVector = Main.rand.NextVector2Circular(3, 3),
                        PositionInWorld = Main.LocalPlayer.Center,
                        IndexOfPlayerWhoInvokedThis = (byte)Main.myPlayer,
                    });

                    ParticleOrchestrator.SpawnParticlesDirect(ParticleOrchestraType.ShimmerArrow, new()
                    {
                        MovementVector = Main.rand.NextVector2Circular(3, 3),
                        PositionInWorld = Main.LocalPlayer.Center,
                        IndexOfPlayerWhoInvokedThis = (byte)Main.myPlayer,
                    });
                }
            }
        }
    }
}
