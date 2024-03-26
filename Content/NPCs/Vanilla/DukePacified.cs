using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class DukePacified : ModNPC
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.DukeFishron}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_4";

    private ref float Timer => ref NPC.ai[0];
    private ref float NetTimer => ref NPC.ai[2];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 8;
        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.BrainofCthulhu);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = frameHeight * (int)(NPC.frameCounter / 6f % 6);
    }

    public override bool PreAI()
    {
        NPC.breath = NPC.breathMax;
        Timer++;

        if (NetTimer++ > 600) // Sync occasionally to be sure
        {
            NPC.netUpdate = true;
            NetTimer = 0;
        }

        if (NPC.homeless)
        {
            int floor = NPC.GetFloor(out bool water, 40);
            bool dontLevel = false;

            if (water)
            {
                int x = (int)NPC.Center.X / 16;
                int bottomPool = floor;

                while (!WorldGen.SolidTile(x, ++bottomPool)) { }

                int dif = bottomPool - floor;

                if (dif > 12)
                {
                    NPC.velocity.X *= 0.95f;
                    NPC.velocity.Y = (bottomPool - 12) * 16 - NPC.Center.Y;
                    NPC.velocity.Y *= 0.02f;

                    dontLevel = true;
                }
                else
                    NPC.velocity.Y -= 0.2f;

                NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, 0.05f);
            }
            else
            {
                NPC.velocity.X += NPC.Center.X < Main.maxTilesX * 8 ? -0.5f : 0.5f;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -8, 8);
                RotateToVelocityAngle();
            }

            if (!dontLevel)
            {
                NPC.velocity.Y += (((NPC.GetFloor(80, true) - 10) * 16) - NPC.Center.Y) / 500f;
                NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -4, 4);
            }
        }
        else
        {
            if (!NPC.IsBeingTalkedTo())
            {
                NPC.velocity.X += NPC.Center.X / 16f < NPC.homeTileX ? 0.1f : -0.1f;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -8, 8);
                NPC.velocity.Y += (((NPC.homeTileY - 10) * 16) - NPC.Center.Y) / 500f;
                NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -4, 4);
                RotateToVelocityAngle();
            }
            else
            {
                NPC.velocity.X *= 0.98f;
                NPC.velocity.Y *= 0.9f;
                NPC.rotation = Utils.AngleLerp(NPC.rotation, 0, 0.05f);
            }
        }

        return false;
    }

    private void RotateToVelocityAngle()
    {
        float angle = NPC.velocity.ToRotation();

        if (NPC.velocity.X < 0)
        {
            NPC.spriteDirection = 1;
            angle -= MathHelper.Pi;
        }
        else
            NPC.spriteDirection = -1;

        NPC.rotation = Utils.AngleLerp(NPC.rotation, angle, 0.1f);
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Duke." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
