﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class SkeletronPacified : ModNPC
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.SkeletronHead}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_19";

    private SkeletronHand leftHand;
    private SkeletronHand rightHand;

    private ref float MoveSpeed => ref NPC.ai[0];
    private ref float Timer => ref NPC.ai[1];
    private ref float NetTimer => ref NPC.ai[2];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.SkeletronHead];

        NPCID.Sets.IsTownPet[Type] = true;
        NPCID.Sets.SpawnsWithCustomName[Type] = true;
        
        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.SkeletronHead);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;

        leftHand ??= new SkeletronHand(NPC, true);
        rightHand ??= new SkeletronHand(NPC, false);
    }

    public override bool PreAI()
    {
        if (++NetTimer % 360 == 0)
            NPC.netUpdate = true;

        leftHand.Update();
        rightHand.Update();

        bool discussing = NPC.IsBeingTalkedTo();
        var dungeon = Main.remixWorld ? new Vector2(Main.spawnTileX, Main.spawnTileY) : new Vector2(Main.dungeonX, Main.dungeonY);
        Vector2 home = (NPC.homeless ? dungeon : new Vector2(NPC.homeTileX, NPC.homeTileY)).ToWorldCoordinates();

        if (!discussing)
        {
            const float MaxDist = 250 * 250;

            float dist = NPC.DistanceSQ(home);

            if (dist > MaxDist)
                NPC.velocity += NPC.SafeDirectionTo(home) * 0.5f;
            else if (NPC.velocity.LengthSquared() < MoveSpeed * MoveSpeed)
            {
                if (NPC.velocity.LengthSquared() < 1)
                    NPC.velocity.SafeNormalize(Vector2.One);

                NPC.velocity *= 1.02f;
            }

            if (NPC.velocity == Vector2.Zero)
                NPC.velocity = new Vector2(0, -2);

            if (dist < MaxDist * 4 && !NPC.homeless)
                MoveSpeed = MathHelper.Lerp(MoveSpeed, 5, 0.05f);
            else if (dist < MaxDist * 40)
                MoveSpeed = MathHelper.Lerp(MoveSpeed, 9, 0.05f);
            else
                MoveSpeed = MathHelper.Lerp(MoveSpeed, 20, 0.05f);

            NPC.velocity = Vector2.Clamp(NPC.velocity, new Vector2(-MoveSpeed), new Vector2(MoveSpeed));
            Timer++;

            if (Timer % 360 > 320)
               NPC.velocity = NPC.velocity.RotatedBy(Timer % 700 > 350 ? -0.02f : 0.02f);
        }
        else
            NPC.velocity *= 0.95f;

        NPC.rotation = NPC.velocity.X * 0.05f;

        return false;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Skeletron." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
            return true;

        leftHand.Draw(screenPos);
        rightHand.Draw(screenPos);
        return true;
    }

    private class SkeletronHand : Entity
    {
        private int Side => !leftHand ? -1 : 1;

        readonly NPC parent = null;
        readonly bool leftHand = false;

        private float _rotation;

        public SkeletronHand(NPC parent, bool left)
        {
            this.parent = parent;
            leftHand = left;
            Center = parent.Center;
            Size = ContentSamples.NpcsByNetId[NPCID.SkeletronHand].Size;
        }

        public void Update()
        {
            float xOff = (leftHand ? -160 : 160) * (parent.IsBeingTalkedTo() ? 0.6f : 1);
            float yOff = 100 + (MathF.Sin(parent.ai[2] * 0.02f) * 50 * (parent.IsBeingTalkedTo() ? 1 : 0));
            var target = parent.Center + new Vector2(xOff, yOff);
            float dist = MathHelper.Clamp(Distance(target) / 40f, 0, parent.ai[0]);
            velocity = this.SafeDirectionTo(target) * dist;
            Center += velocity;

            if (DistanceSQ(parent.Center) > 12000 * 12000)
                Center = parent.Center + new Vector2(0, 20);
        }

        public void Draw(Vector2 screenPos)
        {
            Main.instance.LoadNPC(NPCID.SkeletronHand);
            var tex = TextureAssets.Npc[NPCID.SkeletronHand].Value;

            Main.spriteBatch.Draw(tex, Center - screenPos, null, Lighting.GetColor(Center.ToTileCoordinates()), _rotation, tex.Size() / 2f, 1f, 0, 0);
            DrawArm(screenPos);
        }

        // Heavily modified version of vanilla's skeletron arms code
        private void DrawArm(Vector2 screenPos)
        {
            Vector2 drawPos = new(Center.X - 5f * Side, position.Y + 20f);

            for (int j = 0; j < 2; j++)
            {
                Vector2 difference = parent.Center - drawPos;
                float diffDistance;

                if (j == 0)
                {
                    difference.X -= 200f * Side;
                    difference.Y += 130f;
                    diffDistance = 92f / difference.Length();
                }
                else
                {
                    difference.X -= 50f * Side;
                    difference.Y += 80f;
                    diffDistance = 60f / difference.Length();
                }

                drawPos += difference * diffDistance;

                float rotation = (float)Math.Atan2(difference.Y, difference.X) - 1.57f;
                Color color = Lighting.GetColor(drawPos.ToTileCoordinates());
                var texture = TextureAssets.BoneArm.Value;
                Main.spriteBatch.Draw(texture, drawPos - screenPos, null, color, rotation, texture.Size() / 2f, 1f, 0, 0f);

                if (j == 0)
                {
                    drawPos += difference * diffDistance / 2f;
                    _rotation = rotation + MathHelper.Pi;
                }
                else if (Main.instance.IsActive)
                {
                    drawPos.X += difference.X * diffDistance - 16f;
                    drawPos.Y += difference.Y * diffDistance - 6f;
                }
            }
        }
    }
}
