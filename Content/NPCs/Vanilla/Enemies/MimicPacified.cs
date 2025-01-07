using BossForgiveness.Content.NPCs.Mechanics.Enemies;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla.Enemies;

[AutoloadHead]
public class MimicPacified : ModNPC
{
    public override string Texture => "Terraria/Images/NPC_" + NPCID.Mimic;
    public override string HeadTexture => (GetType().Namespace + "." + Name).Replace('.', '/') + "_Head";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Mimic];
        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Mimic);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override bool PreAI()
    {
        if (NPC.homeless) 
        {
            if (NPC.collideY)
            {
                NPC.ai[2]++;

                if (NPC.ai[2] >= 20)
                {
                    if (Main.rand.NextBool())
                        NPC.velocity.X = 2;
                    else
                        NPC.velocity.X = -2;

                    NPC.velocity.Y = -3.5f;
                    NPC.ai[2] = 0;

                    NPC.direction = Math.Sign(NPC.velocity.X);
                    NPC.spriteDirection = NPC.direction;
                    NPC.netUpdate = true;
                }
                else
                    NPC.velocity.X *= 0.9f;
            }
        }

        return false;
    }

    public override void FindFrame(int frameHeight)
    {
        int frameSpeed = 3;

        if (NPC.collideY)
            NPC.frameCounter--;
        else
            NPC.frameCounter++;

        if (NPC.frameCounter < 0.0)
            NPC.frameCounter = 0;

        if (NPC.frameCounter > frameSpeed * 4)
            NPC.frameCounter = frameSpeed * 4;

        if (NPC.frameCounter < frameSpeed)
            NPC.frame.Y = frameHeight;
        else if (NPC.frameCounter < frameSpeed * 2)
            NPC.frame.Y = frameHeight * 2;
        else if (NPC.frameCounter < frameSpeed * 3)
            NPC.frame.Y = frameHeight * 3;
        else if (NPC.frameCounter < frameSpeed * 4)
            NPC.frame.Y = frameHeight * 4;
        else if (NPC.frameCounter < frameSpeed * 5)
            NPC.frame.Y = frameHeight * 5;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Mimic." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
