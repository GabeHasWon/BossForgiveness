using Microsoft.Xna.Framework;
using SubworldLibrary;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordSubworldSystem : ModSystem
{
    public static bool InSubworld => SubworldSystem.Current is MoonLordPacificationSubworld;

    public override void Load()
    {
        On_Main.DrawBlack += HijackDrawBlack;
        On_Lighting.AddLight_int_int_float_float_float += BlockLight;
        On_Lighting.AddLight_int_int_int_float += BlockLight_Torch;
    }

    private void BlockLight_Torch(On_Lighting.orig_AddLight_int_int_int_float orig, int i, int j, int torchID, float lightAmount)
    {
        if (InSubworld)
            return;

        orig(i, j, torchID, lightAmount);
    }

    private void BlockLight(On_Lighting.orig_AddLight_int_int_float_float_float orig, int i, int j, float r, float g, float b)
    {
        if (InSubworld)
            return;

        orig(i, j, r, g, b);
    }

    private void HijackDrawBlack(On_Main.orig_DrawBlack orig, Main self, bool force)
    {
        orig(self, force);
    }

    public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
    {
        if (!InSubworld)
            return;

        tileColor = Color.Black;
        backgroundColor = Color.White;
    }
}
