using Microsoft.Xna.Framework;
using Terraria;

namespace BossForgiveness.Content.NPCs;

public static class NPCUtils
{
    public static int GetFloor(this NPC npc, int maxDist = 40)
    {
        int x = (int)((npc.Center.X + npc.velocity.X) / 16f);
        int y = (int)(npc.Center.Y / 16f);
        int startY = y;

        while (!WorldGen.SolidTile(x, y))
        {
            y++;

            if (y - startY > maxDist)
                return y;
        }

        return y;
    }

    public static bool IsBeingTalkedTo(this NPC npc)
    {
        for (int j = 0; j < 255; j++)
            if (Main.player[j].active && Main.player[j].talkNPC == npc.whoAmI)
                return true;

        return false;
    }
}
