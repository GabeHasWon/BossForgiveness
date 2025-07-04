using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Vortex;

internal class VortexModPlayer : ModPlayer
{
    public bool Dummy = false;
    public List<CachedDummyData> OldInformation = [];
    public int Timer = 0;

    public override void ResetEffects()
    {
        Dummy = false;
        Timer++;

        if (Timer % 2 == 0)
            OldInformation.Add(new CachedDummyData(Player.Center, Player.velocity, Player.direction));

        if (OldInformation.Count > 180)
            OldInformation.RemoveAt(0);
    }

    public override void DrawEffects(PlayerDrawSet drawInfo, ref float r, ref float g, ref float b, ref float a, ref bool fullBright)
    {
        if (Dummy)
        {
            const float Transparency = 0.1f;

            r *= Transparency;
            g *= Transparency;
            b *= Transparency;
            a *= Transparency;
        }
    }
}
