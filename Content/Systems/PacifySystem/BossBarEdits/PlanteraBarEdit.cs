using BossForgiveness.Content.NPCs.Mechanics.Plantera;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;

internal class PlanteraBarEdit : ILoadable
{
    private static int PlanteraBar = -1;

    public static Asset<Texture2D> PacificationSymbol = null;

    public void Load(Mod mod)
    {
        On_CommonBossBigProgressBar.Draw += DrawBar;
        On_BigProgressBarHelper.DrawFancyBar_SpriteBatch_float_float_Texture2D_Rectangle += HijackBar;

        PacificationSymbol = mod.Assets.Request<Texture2D>("Content/Systems/PacifySystem/BossBarEdits/PacificationSymbol");
    }

    private void HijackBar(On_BigProgressBarHelper.orig_DrawFancyBar_SpriteBatch_float_float_Texture2D_Rectangle orig, SpriteBatch spriteBatch, float lifeAmount, float lifeMax, 
        Texture2D barIconTexture, Rectangle barIconFrame)
    {
        if (PlanteraBar != -1)
        {
            NPC npc = Main.npc[PlanteraBar];

            if (PlanteraBar != -1 && npc.TryGetGlobalNPC<PlanteraPacificationNPC>(out _) && PlanteraPacificationNPC.CanPacify(npc))
            {
                Vector2 barCenter = Main.ScreenSize.ToVector2() * new Vector2(0.5f, 1f) + new Vector2(-1f, -90f);
                spriteBatch.Draw(PacificationSymbol.Value, barCenter, null, Color.White, 0f, PacificationSymbol.Size() / 2f, 1f, SpriteEffects.None, 0);
            }
        }

        orig(spriteBatch, lifeAmount, lifeMax, barIconTexture, barIconFrame);

        if (PlanteraBar != -1)
        {
            NPC npc = Main.npc[PlanteraBar];

            if (npc.TryGetGlobalNPC<PlanteraPacificationNPC>(out _) && PlanteraPacificationNPC.CanPacify(npc))
                DrawOverlay(npc, barIconFrame, barIconTexture);
        }
    }

    private void DrawOverlay(NPC npc, Rectangle barIconFrame, Texture2D barIconTexture)
    {
        Texture2D value = Main.Assets.Request<Texture2D>("Images/UI/UI_BossBar").Value;

        int pac = npc.GetGlobalNPC<PlanteraPacificationNPC>().pacification;
        float maxPac = PlanteraPacificationNPC.MaxPacificationsNeeded;

        Point p = new Point(456, 22);
        Point p2 = new Point(32, 24);
        Rectangle frame = value.Frame(1, 6, 0, 3);
        Color color = Color.Green * 1f;
        Rectangle rectangle = Utils.CenteredRectangle(Main.ScreenSize.ToVector2() * new Vector2(0.5f, 1f) + new Vector2(0f, -50f), p.ToVector2());
        Vector2 vector = rectangle.TopLeft() - p2.ToVector2();

        Main.spriteBatch.Draw(value, vector, new Rectangle(frame.X, frame.Y, frame.Width, frame.Height), Color.Red, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
        Main.spriteBatch.Draw(value, vector, new Rectangle(frame.X, frame.Y, (int)(frame.Width * (pac / maxPac)), frame.Height), color, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

        if (BigProgressBarSystem.ShowText)
            BigProgressBarHelper.DrawHealthText(Main.spriteBatch, rectangle, Vector2.Zero, pac, maxPac);
    }

    private void DrawBar(On_CommonBossBigProgressBar.orig_Draw orig, CommonBossBigProgressBar self, ref BigProgressBarInfo info, SpriteBatch spriteBatch)
    {
        NPC npc = Main.npc[info.npcIndexToAimAt];

        PlanteraBar = npc.whoAmI;
        orig(self, ref info, spriteBatch);
        PlanteraBar = -1;
    }

    public void Unload() { }
}
