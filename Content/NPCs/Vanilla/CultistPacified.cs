using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class CultistPacified : ModNPC
{
    private static float drawResetTimer = 0;

    public override string Texture => $"Terraria/Images/NPC_{NPCID.CultistBoss}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_24";

    private ref float Timer => ref NPC.ai[0];
    private ref float NetTimer => ref NPC.ai[1];

    private bool MovingHome
    {
        get => NPC.ai[2] == 1;
        set => NPC.ai[2] = value ? 1 : 0;
    }

    private ref float AnimSpeed => ref NPC.ai[3];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 16;
        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.CultistBoss);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;
        NPC.Opacity = 1f;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        AnimSpeed = MathHelper.Lerp(AnimSpeed, NPC.IsBeingTalkedTo() ? 0.8f : 1.1f, 0.2f);
        NPC.frameCounter += AnimSpeed;
        NPC.frame.Y = 64 * (int)(NPC.frameCounter / 8f % 4 + 3);
    }

    public override bool PreAI()
    {
        if (NetTimer++ > 600) // Sync occasionally to be sure
        {
            NPC.netUpdate = true;
            NetTimer = 0;
        }

        Timer++;

        if (NPC.homeless)
        {
            NPC.TargetClosest(false);
            NPC.FaceTarget();
            NPC.spriteDirection = NPC.direction;
            float sin = MathF.Sin(Timer * 0.025f) * 35;

            if (NPC.IsBeingTalkedTo())
            {
                sin *= 0.25f;
                sin += 6;
            }

            float yVel = ((NPC.GetFloor(80, true) - 10 + sin) * 16 - NPC.Center.Y) / 300f;
            NPC.velocity = new Vector2(0, yVel);
        }
        else
        {
            Vector2 home = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates();

            if (NPC.DistanceSQ(home) > 1200 * 1200 && !MovingHome)
                MovingHome = true;

            if (NPC.DistanceSQ(home) < 40 * 40 && MovingHome)
                MovingHome = false;

            if (MovingHome)
                NPC.Center = Vector2.Lerp(NPC.Center, home, 0.05f);
            else
            {
                NPC.TargetClosest(false);
                NPC.FaceTarget();
                NPC.spriteDirection = NPC.direction;

                float sin = MathF.Sin(Timer * 0.025f) * 35;

                if (NPC.IsBeingTalkedTo())
                {
                    sin *= 0.25f;
                    sin += 6;
                }

                float yVel = ((NPC.homeTileY - 10 + sin) * 16 - NPC.Center.Y) / 300f;
                NPC.velocity = new Vector2(0, yVel);
            }
        }

        return false;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Cultist." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        drawResetTimer = Timer;
        Timer = 0;
        return true;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => Timer = drawResetTimer;
}
