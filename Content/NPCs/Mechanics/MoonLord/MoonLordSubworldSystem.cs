using Microsoft.Xna.Framework.Graphics;
using MonoMod.Cil;
using SubworldLibrary;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Drawing;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordSubworldSystem : ModSystem
{
    public const float MaxFadeTime = 7 * 60;

    private delegate void hook_PushSprite(SpriteBatch spriteBatch, Texture2D texture, float sourceX, float sourceY, float sourceW, float sourceH, float destinationX, float destinationY, 
        float destinationW, float destinationH, Color color, float originX, float originY, float rotationSin, float rotationCos, float depth, byte effects);

    private static bool Blackout = false;

    public static bool InSubworld => SubworldSystem.Current is MoonLordPacificationSubworld;

    internal static float PlayerFadeEffect => MathHelper.Clamp(Main.LocalPlayer.GetModPlayer<MoonlordDomainPlayer>().FadeTimer / MaxFadeTime, 0, 1);

    public static Color Fade(Color color) => Color.Lerp(color, Color.Black, PlayerFadeEffect);

    public override void Load()
    {
        On_Lighting.AddLight_int_int_int_float += BlockLight_Torch;
        On_Player.QuickMount += HijackQuickMount;
        On_Main.DrawBG += DrawBG;
        On_Main.DrawStarsInBackground += On_Main_DrawStarsInBackground;
        On_DrawData.Draw_SpriteDrawBuffer += DrawSilhouette;
        On_TileDrawing.Draw += AddCheck;
        On_TileDrawing.PostDrawTiles += AddCheck;
        On_Main.DrawDust += AddCheck;

        MonoModHooks.Add(typeof(SpriteBatch).GetMethod("PushSprite", BindingFlags.Instance | BindingFlags.NonPublic), DetourPushSprite);

        IL_Main.DoDraw_Tiles_Solid += BlackenedTiles;
        IL_Main.DoDraw_WallsAndBlacks += HideWalls;
    }

    private void AddCheck(On_Main.orig_DrawDust orig, Main self)
    {
        Blackout = true;
        orig(self);
        Blackout = false;
    }

    private void AddCheck(On_TileDrawing.orig_PostDrawTiles orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets)
    {
        Blackout = true;
        orig(self, solidLayer, forRenderTargets, intoRenderTargets);
        Blackout = false;
    }

    private void AddCheck(On_TileDrawing.orig_Draw orig, TileDrawing self, bool solidLayer, bool forRenderTargets, bool intoRenderTargets, int waterStyleOverride)
    {
        Blackout = true;
        orig(self, solidLayer, forRenderTargets, intoRenderTargets, waterStyleOverride);
        Blackout = false;
    }

    private static void DetourPushSprite(hook_PushSprite orig, SpriteBatch spriteBatch, Texture2D texture, float sourceX, float sourceY, float sourceW, float sourceH, float destinationX, 
        float destinationY, float destinationW, float destinationH, Color color, float originX, float originY, float rotationSin, float rotationCos, float depth, byte effects)
    {
        if (Blackout)
            color = Fade(color);

        orig(spriteBatch, texture, sourceX, sourceY, sourceW, sourceH, destinationX, destinationY, destinationW, destinationH, color, originX, originY, rotationSin, rotationCos, depth, effects);
    }

    private void On_Main_DrawStarsInBackground(On_Main.orig_DrawStarsInBackground orig, Main self, Main.SceneArea sceneArea, bool artificial)
    {
        orig(self, sceneArea, artificial);

        if (InSubworld && false)
        {
            MoonlordBackground.Draw();
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-20, -20, Main.screenWidth + 40, Main.screenHeight + 40), Color.White * PlayerFadeEffect);
        }
    }

    private void DrawSilhouette(On_DrawData.orig_Draw_SpriteDrawBuffer orig, ref DrawData self, SpriteDrawBuffer sb)
    {
        if (InSubworld)
        {
            self.color = Fade(self.color);
        }

        orig(ref self, sb);
    }

    private void HideWalls(ILContext il)
    {
        ILCursor c = new(il);

        for (int i = 0; i < 2; ++i)
            if (!c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(Color).GetProperty(nameof(Color.White)).GetGetMethod())))
                return;

        c.EmitDelegate(ModifyTileDrawColor);
    }

    private void DrawBG(On_Main.orig_DrawBG orig, Main self)
    {
        orig(self);

        if (InSubworld)
        {
            MoonlordBackground.Draw();
            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-20, -20, Main.screenWidth + 40, Main.screenHeight + 40), Color.White * PlayerFadeEffect);
        }
    }

    private void BlackenedTiles(ILContext il)
    {
        ILCursor c = new(il);

        if (!c.TryGotoNext(MoveType.After, x => x.MatchCall(typeof(Color).GetProperty(nameof(Color.White)).GetGetMethod())))
            return;

        c.EmitDelegate(ModifyTileDrawColor);
    }

    public static Color ModifyTileDrawColor(Color white) => InSubworld ? Color.Lerp(white, Color.Black, PlayerFadeEffect) : white;

    private void HijackQuickMount(On_Player.orig_QuickMount orig, Player self)
    {
        if (MoonLordPacificationTracker.TeleportTimer > 2)
            return;

        orig(self);
    }

    private void BlockLight_Torch(On_Lighting.orig_AddLight_int_int_int_float orig, int i, int j, int torchID, float lightAmount)
    {
        if (InSubworld)
            return;

        orig(i, j, torchID, lightAmount);
    }
}

public class MoonlordDomainPlayer : ModPlayer
{
    internal int FadeTimer = 0;
    internal int DomainTimer = 0;

    public override void OnEnterWorld()
    {
        if (SubworldSystem.Current is not MoonLordPacificationSubworld)
        {
            return;
        }

        FadeTimer = (int)MoonLordSubworldSystem.MaxFadeTime * 3;
        MoonlordBackground.Elements.Clear();
        MoonlordBackground.ElementCountsByName.Clear();
    }

    public override void PreUpdate()
    {
#if DEBUG
        const int Speed = 5;
#else
        const int Speed = 1;
#endif

        FadeTimer -= Speed;
        DomainTimer += Speed;

        if (SubworldSystem.Current is MoonLordPacificationSubworld)
        {
            Main.shimmerAlpha = Utils.GetLerpValue(Main.spawnTileY, Main.maxTilesY - 300, Player.Center.Y / 16f, true);
        }
    }

    public override bool CanUseItem(Item item) => FadeTimer <= 0;
}
