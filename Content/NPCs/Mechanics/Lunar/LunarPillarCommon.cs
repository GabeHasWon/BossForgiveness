using BossForgiveness.Content.NPCs.Mechanics.MoonLord;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar;

internal class LunarPillarCommon : ILoadable
{
    public static void SpamFragments(IEntitySource source, Rectangle hitbox, int fragType)
    {
        int count = Main.rand.Next(6, 9);

        for (int k = 0; k < count; ++k)
        {
            int subStack = Main.rand.Next(3, 7);
            Item.NewItem(source, hitbox, fragType, subStack);
        }
    }

    void ILoadable.Load(Mod mod)
    {
        On_WorldGen.StartImpendingDoom += ReplaceImpendingDoom;
    }

    private void ReplaceImpendingDoom(On_WorldGen.orig_StartImpendingDoom orig, int countdownTime)
    {
        if (MoonLordPacificationTracker.LunarTowersPacified.Count == 4)
        {
            NPC.LunarApocalypseIsUp = false;
            WorldGen.BroadcastText(NetworkText.FromKey("Mods.BossForgiveness.PortalAccess"), new Color(50, 255, 130));

            if (Main.netMode != NetmodeID.MultiplayerClient)
                WorldGen.GetRidOfCultists();

            ModContent.GetInstance<PortalAccessSystem>().HasPortal = true;
            return;
        }

        orig(countdownTime);
    }

    void ILoadable.Unload()
    {
    }
}
