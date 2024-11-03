using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.UI.Chat;

namespace BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;

internal class CustomBarEdit : ILoadable
{
    private static int CurrentBarNPC = -1;

    public static string OverrideText = "";
    public static int TextOffsetX = 0;
    public static Asset<Texture2D> PacificationSymbol = null;

    public void Load(Mod mod)
    {
        On_CommonBossBigProgressBar.Draw += DrawBar;
        On_BigProgressBarHelper.DrawFancyBar_SpriteBatch_float_float_Texture2D_Rectangle += HijackBar;
        On_BrainOfCthuluBigProgressBar.Draw += DrawBarBoC;
        On_DeerclopsBigProgressBar.Draw += DrawBarDeerclops;
        On_GolemHeadProgressBar.Draw += On_GolemHeadProgressBar_Draw;
        On_EaterOfWorldsProgressBar.Draw += On_EaterOfWorldsProgressBar_Draw;
        On_TwinsBigProgressBar.Draw += DrawBarTwins;

        PacificationSymbol = mod.Assets.Request<Texture2D>("Content/Systems/PacifySystem/BossBarEdits/PacificationSymbol");
    }

    private void On_EaterOfWorldsProgressBar_Draw(On_EaterOfWorldsProgressBar.orig_Draw orig, EaterOfWorldsProgressBar self, ref BigProgressBarInfo info, SpriteBatch spriteBatch)
    {
        NPC npc = Main.npc[info.npcIndexToAimAt];

        CurrentBarNPC = npc.whoAmI;
        orig(self, ref info, spriteBatch);
        CurrentBarNPC = -1;
    }

    private void On_GolemHeadProgressBar_Draw(On_GolemHeadProgressBar.orig_Draw orig, GolemHeadProgressBar self, ref BigProgressBarInfo info, SpriteBatch spriteBatch)
    {
        int freeHead = NPC.FindFirstNPC(NPCID.GolemHead);

        if (freeHead == -1)
            freeHead = NPC.FindFirstNPC(NPCID.GolemHeadFree);

        if (freeHead == -1)
            freeHead = NPC.FindFirstNPC(NPCID.Golem);

        if (freeHead == -1)
            freeHead = info.npcIndexToAimAt;

        NPC npc = Main.npc[freeHead];

        CurrentBarNPC = npc.whoAmI;
        orig(self, ref info, spriteBatch);
        CurrentBarNPC = -1;
    }

    private void DrawBarTwins(On_TwinsBigProgressBar.orig_Draw orig, TwinsBigProgressBar self, ref BigProgressBarInfo info, SpriteBatch spriteBatch)
    {
        int who = NPC.FindFirstNPC(NPCID.Spazmatism);

        if (who == -1)
            who = info.npcIndexToAimAt;

        NPC npc = Main.npc[who];

        CurrentBarNPC = npc.whoAmI;
        orig(self, ref info, spriteBatch);
        CurrentBarNPC = -1;
    }

    private void DrawBarDeerclops(On_DeerclopsBigProgressBar.orig_Draw orig, DeerclopsBigProgressBar self, ref BigProgressBarInfo info, SpriteBatch spriteBatch)
    {
        NPC npc = Main.npc[info.npcIndexToAimAt];

        CurrentBarNPC = npc.whoAmI;
        orig(self, ref info, spriteBatch);
        CurrentBarNPC = -1;
    }

    private void DrawBarBoC(On_BrainOfCthuluBigProgressBar.orig_Draw orig, BrainOfCthuluBigProgressBar self, ref BigProgressBarInfo info, SpriteBatch spriteBatch)
    {
        NPC npc = Main.npc[info.npcIndexToAimAt];

        CurrentBarNPC = npc.whoAmI;
        orig(self, ref info, spriteBatch);
        CurrentBarNPC = -1;
    }

    private void HijackBar(On_BigProgressBarHelper.orig_DrawFancyBar_SpriteBatch_float_float_Texture2D_Rectangle orig, SpriteBatch spriteBatch, float lifeAmount, float lifeMax, 
        Texture2D barIconTexture, Rectangle barIconFrame)
    {
        if (CurrentBarNPC != -1)
        {
            NPC npc = Main.npc[CurrentBarNPC];

            foreach (var gNPC in npc.Globals)
            {
                if (gNPC is ICustomBarNPC barNPC && barNPC.ShowOverlay(npc, out _, out _))
                {
                    Vector2 barCenter = Main.ScreenSize.ToVector2() * new Vector2(0.5f, 1f) + new Vector2(-1f, -90f);
                    spriteBatch.Draw(PacificationSymbol.Value, barCenter, null, Color.White, 0f, PacificationSymbol.Size() / 2f, 1f, SpriteEffects.None, 0);
                }
            }
        }

        orig(spriteBatch, lifeAmount, lifeMax, barIconTexture, barIconFrame);

        if (CurrentBarNPC != -1)
        {
            NPC npc = Main.npc[CurrentBarNPC];

            foreach (var gNPC in npc.Globals)
            {
                if (gNPC is ICustomBarNPC barNPC && barNPC.ShowOverlay(npc, out float prog, out float max))
                    DrawOverlay(prog, max);
            }
        }
    }

    private static void DrawOverlay(float pac, float maxPac)
    {
        Texture2D value = Main.Assets.Request<Texture2D>("Images/UI/UI_BossBar").Value;

        var p = new Point(456, 22);
        var p2 = new Point(32, 24);
        Rectangle frame = value.Frame(1, 6, 0, 3);
        Color color = Color.Green * 1f;
        Rectangle rectangle = Utils.CenteredRectangle(Main.ScreenSize.ToVector2() * new Vector2(0.5f, 1f) + new Vector2(0f, -50f), p.ToVector2());
        Vector2 vector = rectangle.TopLeft() - p2.ToVector2();

        Main.spriteBatch.Draw(value, vector, new Rectangle(frame.X, frame.Y, frame.Width, frame.Height), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(value, vector, new Rectangle(frame.X, frame.Y, (int)(frame.Width * (pac / maxPac)), frame.Height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        if (OverrideText != null && OverrideText != string.Empty)
        {
            var font = FontAssets.ItemStack.Value;
            Vector2 textSize = font.MeasureString(OverrideText);
            Vector2 position = rectangle.Center.ToVector2() - textSize * 0.5f + new Vector2(TextOffsetX, 0);
            ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, OverrideText, position, Color.White, 0f, Vector2.Zero, Vector2.One);
        }
        else if (BigProgressBarSystem.ShowText)
            BigProgressBarHelper.DrawHealthText(Main.spriteBatch, rectangle, Vector2.Zero, pac, maxPac);

        OverrideText = string.Empty;
        TextOffsetX = 0;
    }

    private void DrawBar(On_CommonBossBigProgressBar.orig_Draw orig, CommonBossBigProgressBar self, ref BigProgressBarInfo info, SpriteBatch spriteBatch)
    {
        NPC npc = Main.npc[info.npcIndexToAimAt];

        CurrentBarNPC = npc.whoAmI;
        orig(self, ref info, spriteBatch);
        CurrentBarNPC = -1;
    }

    public void Unload() { }
}
