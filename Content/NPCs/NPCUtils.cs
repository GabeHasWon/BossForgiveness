using Terraria;

namespace BossForgiveness.Content.NPCs;

public static class NPCUtils
{
    public static int GetFloor(NPC npc)
    {
        int x = (int)((npc.Center.X + npc.velocity.X) / 16f);
        int y = (int)(npc.Center.Y / 16f);

        while (!WorldGen.SolidTile(x, y))
            y++;

        return y;
    }
}
