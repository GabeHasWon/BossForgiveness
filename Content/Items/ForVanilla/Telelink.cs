using BossForgiveness.Content.NPCs.Mechanics.Lunar.Nebula;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Gamepad;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class Telelink : ModItem
{
    private static Asset<Texture2D> Icon = null;

    public override void Load()
    {
        On_Main.DrawCursor += AddIcon;

        Icon = ModContent.Request<Texture2D>(Texture + "Icon");
    }

    private void AddIcon(On_Main.orig_DrawCursor orig, Vector2 bonus, bool smart)
    {
        orig(bonus, smart);

        if (Main.gameMenu)
            return;

        NebulaLinkPlayer linkPlayer = Main.LocalPlayer.GetModPlayer<NebulaLinkPlayer>();
        bool flag = UILinkPointNavigator.Available && !PlayerInput.InBuildingMode;

        if (linkPlayer.lastCanLink && linkPlayer.hovering)
        {
            Vector2 t = Main.screenPosition;
            Vector2 smartPointerPos = Vector2.Zero;
            bool smartCursor = Main.SmartCursorIsUsed;

            if (smartCursor)
            {
                PlayerInput.smartSelectPointer.UpdateCenter(Main.ScreenSize.ToVector2() / 2f);
                smartPointerPos = PlayerInput.smartSelectPointer.GetPointerPosition();
                if (Vector2.Distance(smartPointerPos, t) < 1f)
                    smartCursor = false;
                else
                    Utils.Swap(ref t, ref smartPointerPos);
            }

            float adjTransparency = 1f;
            bonus.Y -= 18;

            if (smartCursor)
            {
                adjTransparency = 0.3f;
                Color color = Color.White * Main.GamepadCursorAlpha;
                float rotation = (float)Math.PI / 2f * Main.GlobalTimeWrappedHourly;
                Main.spriteBatch.Draw(Icon.Value, smartPointerPos + bonus, null, color, rotation, Icon.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
            }

            if (smart && !flag)
            {
                Color color = Color.White * Main.GamepadCursorAlpha * adjTransparency;
                Main.spriteBatch.Draw(Icon.Value, t + bonus, null, color, 0f, Icon.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
            }
            else
            {
                Color color = Color.White;
                Main.spriteBatch.Draw(Icon.Value, Main.MouseScreen + bonus, null, color, 0f, Icon.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
            }
        }
    }

    public override void SetDefaults()
    {
        Item.Size = new(32, 26);
        Item.value = 0;
        Item.maxStack = 1;
        Item.rare = ItemRarityID.Lime;
    }

    public override void HoldItem(Player player) => player.GetModPlayer<NebulaLinkPlayer>().canLink = true;
    public override bool CanPickup(Player player) => !player.HasItem(Type) && player.trashItem.type != Type;
}
