using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class PacifiedDeerclops : ModNPC
{
    private ref float Timer => ref NPC.ai[0];
    private ref float WaitTime => ref NPC.ai[1];
    private ref float Target => ref NPC.ai[2];

    public override void SetStaticDefaults() => NPCID.Sets.IsTownPet[Type] = true;

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Deerclops);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.noTileCollide = false;
        NPC.noGravity = false;
        NPC.netAlways = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;

        int frame = 0;

        if (NPC.velocity.X != 0)
        {
            int ResetTime = 10;
            int AnimationSpeed = 15;

            NPC.frameCounter += Math.Abs(NPC.velocity.X);
            if (NPC.frameCounter >= (double)(ResetTime * AnimationSpeed) || NPC.frameCounter < 0.0)
                NPC.frameCounter = 0.0;

            frame = 2 + (int)(NPC.frameCounter / (double)AnimationSpeed);
        }

        NPC.frame.Y = frame;
        NPC.frame.Width = 218;
        NPC.frame.Height = 240;
    }

    public override bool PreAI()
    {
        bool discussing = NPC.IsBeingTalkedTo();

        if (!discussing)
            Timer++;

        if (Timer > WaitTime)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Target = NPC.Center.X - (Main.rand.NextFloat(300, 400) * (Main.rand.NextBool() ? -1 : 1));
                WaitTime = Main.rand.Next(180, 360);
            }

            Timer = 0;
            NPC.netUpdate = true;
        }

        Vector2 home = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates();

        if (!Main.dayTime && NPC.DistanceSQ(home) > 3000 * 3000) // Teleport code
        {
            int closest = Player.FindClosest(NPC.position, NPC.width, NPC.height);
            Player closestPlayer = Main.player[closest];
            bool notNearMeRightNow = !closestPlayer.active || closestPlayer.DistanceSQ(NPC.Center) >= 2000 * 2000;

            closest = Player.FindClosest(home, NPC.width, NPC.height);
            closestPlayer = Main.player[closest];
            bool notNearMeThen = !closestPlayer.active || closestPlayer.DistanceSQ(home) >= 2000 * 2000;

            if (notNearMeRightNow && notNearMeThen)
            {
                NPC.Center = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates();

                while (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    NPC.position.Y -= 16;

                NPC.netUpdate = true;
            }
        }

        if (Math.Abs(Target - NPC.Center.X) > 16 && !discussing)
            NPC.velocity.X = Math.Sign(Target - NPC.Center.X) * 2.5f;
        else
            NPC.velocity.X = 0;

        if ((Collision.SolidCollision(NPC.Left, 6, 32) && Target < NPC.Center.X) || (Collision.SolidCollision(NPC.Right, 6, 32) && Target > NPC.Center.X))
            Target = NPC.Center.X;

        Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY);

        if (NPC.velocity.X != 0)
            NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);

        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = TextureAssets.Npc[Type].Value;
        Rectangle frame = tex.Frame(5, 5, NPC.frame.Y / 5, NPC.frame.Y % 5, 2, 2);
        SpriteEffects effect = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        spriteBatch.Draw(tex, NPC.Bottom - screenPos + new Vector2(0, 6), frame, drawColor, NPC.rotation, frame.Size() * new Vector2(0.5f, 1f), 1f, effect, 0);

        return false;
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Deerclops." + Main.rand.Next(5));
    public override ITownNPCProfile TownNPCProfile() => new QueenBeeProfile();

    public class QueenBeeProfile : ITownNPCProfile
    {
        public int RollVariation() => 0;
        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => TextureAssets.Npc[NPCID.Deerclops];
        public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("BossForgiveness/Content/NPCs/Vanilla/PacifiedDeerclops_Head");
    }
}
