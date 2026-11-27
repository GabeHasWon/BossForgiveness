using BossForgiveness.Content.NPCs.Mechanics.MoonLord;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

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
        bool hover = new Rectangle(x, y, PortalTex.Width(), PortalTex.Height()).Contains(Main.MouseScreen.ToPoint());
        _lastMouse = Main.mouseLeft;

        Color baseColor = hover ? new Color(200, 200, 200) : Color.White;
        Color color = Color.Lerp(baseColor, baseColor * 0.5f, MathF.Sin((float)Main.timeForVisualEffects * 0.02f) * 0.5f + 0.5f);
        Main.spriteBatch.Draw(PortalTex.Value, new Vector2(x, y), null, color);

        if (hover)
        {
            Main.LocalPlayer.mouseInterface = true;

            ReLogic.Graphics.DynamicSpriteFont font = FontAssets.ItemStack.Value;
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, "Flee", Main.MouseScreen + new Vector2(0, 30), Color.White, 0, Vector2.Zero, Vector2.One);

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
