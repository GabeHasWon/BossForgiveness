using Microsoft.Xna.Framework;
using MonoMod.Cil;
using SubworldLibrary;
using Terraria.DataStructures;
using Terraria.GameContent;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordSubworldSystem : ModSystem
{
    public const float MaxFadeTime = 7 * 60;

    public static bool InSubworld => SubworldSystem.Current is MoonLordPacificationSubworld;

    private static float PlayerFadeEffect => MathHelper.Clamp(Main.LocalPlayer.GetModPlayer<MoonlordDomainPlayer>().FadeTimer / MaxFadeTime, 0, 1);

    public override void Load()
    {
        On_Lighting.AddLight_int_int_int_float += BlockLight_Torch;
        On_Player.QuickMount += HijackQuickMount;
        On_Main.DrawBG += DrawBG;
        On_DrawData.Draw_SpriteDrawBuffer += DrawSilhouette;

        IL_Main.DoDraw_Tiles_Solid += BlackenedTiles;
        IL_Main.DoDraw_WallsAndBlacks += HideWalls;
    }

    private void DrawSilhouette(On_DrawData.orig_Draw_SpriteDrawBuffer orig, ref DrawData self, SpriteDrawBuffer sb)
    {
        if (InSubworld)
        {
            self.color = Color.Lerp(self.color, Color.Black, PlayerFadeEffect);
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
        FadeTimer--;
        DomainTimer++;
    }

    public override bool CanUseItem(Item item) => FadeTimer <= 0;
}
