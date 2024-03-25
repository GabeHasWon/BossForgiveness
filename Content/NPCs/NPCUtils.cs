﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs;

public static class NPCUtils
{
    public static int GetFloor(this NPC npc, int maxDist = 40, bool checkWater = false)
    {
        int x = (int)((npc.Center.X + npc.velocity.X) / 16f);
        int y = (int)(npc.Center.Y / 16f);
        int startY = y;

        if (y < 0)
            return 1;

        while (!WorldGen.SolidTile(x, y) && (!checkWater || Main.tile[x, y].LiquidAmount < 20))
        {
            if (y <= 0)
                return 1;

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

    public static ITownNPCProfile DefaultProfile(this ModNPC npc) => new Profiles.DefaultNPCProfile(npc.Texture, ModContent.GetModHeadSlot(npc.HeadTexture));

    public static void HideFromBestiary(this ModNPC npc)
    {
        NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new() { Hide = true };
        NPCID.Sets.NPCBestiaryDrawOffset.Add(npc.Type, drawModifiers);
    }

    public static bool CanTeleport(this NPC npc, Vector2 destination, float distanceCheck = 1500)
    {
        bool canTeleport = true;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player player = Main.player[i];

            if (player.active && (player.DistanceSQ(npc.Center) < distanceCheck * distanceCheck || player.DistanceSQ(destination) < distanceCheck * distanceCheck))
                canTeleport = false;
        }

        return canTeleport;
    }
}
