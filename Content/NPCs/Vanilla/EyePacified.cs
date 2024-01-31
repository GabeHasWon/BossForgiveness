using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class EyePacified : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 6;

        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.EyeofCthulhu);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.noTileCollide = false;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = 166 * (int)(NPC.frameCounter / 10f % 3);
    }

    public override bool PreAI()
    {
        NPC.rotation = NPC.velocity.ToRotation() - MathHelper.PiOver2;
        NPC.ai[0]++;

        int y = (!NPC.homeless ? NPC.homeTileY : GetFloor()) * 16;
        float dist = y - NPC.Center.Y;

        if (dist < 16 * 15)
            NPC.velocity.Y -= 0.05f;
        else if (dist > 16 * 5)
            NPC.velocity.Y += 0.05f;

        if (NPC.homeless)
        {
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -6, 6);

            if (AnyNearbyPlayer(500, out var plrPos))
            {
                NPC.velocity.X += (plrPos.X - NPC.Center.X) / 1000f;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5, 5);
            }
            else
            {
                NPC.velocity.X *= 0.99f;

                if (NPC.ai[0] % 120 == 0)
                {
                    NPC.velocity.X = Main.rand.NextFloat(-5f, 5f);
                    NPC.netUpdate = true;
                }
            }
        }
        else
        {
            float homeX = NPC.homeTileX * 16;
            float homeXDist = homeX - NPC.Center.Y;

            NPC.velocity.X += (homeX - NPC.Center.X) / 1000f;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5, 5);
        }

        return false;
    }

    private bool AnyNearbyPlayer(int distance, out Vector2 playerPos)
    {
        playerPos = Vector2.Zero;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && !plr.dead && plr.DistanceSQ(NPC.Center) < distance * distance)
            {
                playerPos = plr.Center;
                return true;
            }
        }

        return false;
    }

    private int GetFloor()
    {
        int x = (int)((NPC.Center.X + NPC.velocity.X) / 16f);
        int y = (int)(NPC.Center.Y / 16f);

        while (!WorldGen.SolidTile(x, y))
            y++;

        return y;
    }

    public override bool CheckConditions(int left, int right, int top, int bottom) => bottom < Main.worldSurface;

    public override List<string> SetNPCNameList() => [Lang.GetNPCName(NPCID.EyeofCthulhu).Value];
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.EoC." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => new EoCProfile();

    public class EoCProfile : ITownNPCProfile
    {
        public int RollVariation() => 0;
        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => TextureAssets.Npc[NPCID.EyeofCthulhu];
        public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("BossForgiveness/Content/NPCs/Vanilla/EyePacified_Head");
    }
}
