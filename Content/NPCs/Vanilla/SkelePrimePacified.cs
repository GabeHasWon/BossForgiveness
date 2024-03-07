using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class SkelePrimePacified : ModNPC
{
    private const int ArmCount = 4;

    public override string Texture => $"Terraria/Images/NPC_{NPCID.SkeletronPrime}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_18";

    private ref float Timer => ref NPC.ai[0];

    private readonly List<SkelePrimeArm> _arms = new(ArmCount);

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 3;
        NPCID.Sets.IsTownPet[Type] = true;
        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.SkeletronPrime);
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
        Timer++;
        NPC.rotation = NPC.velocity.X / 16f;

        foreach (var item in _arms)
            item.Update();

        if (NPC.homeless)
        {
            int floor = NPC.GetFloor(40) * 16;
            NPC.velocity.Y += (floor - NPC.Center.Y - 420) / 200f;
            NPC.velocity.Y *= 0.99f;
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -6, 6);
            NPC.velocity.X = MathF.Sin(Timer * 0.03f) * 6;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -6, 6);
        }
        else
        {
            Vector2 home = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates() - new Vector2(0, 320).RotatedBy(MathF.Sin(Timer * 0.02f) * MathHelper.PiOver4);
            NPC.velocity += NPC.DirectionTo(home);
            float dist = (NPC.Distance(home) / 100f);

            if (NPC.velocity.LengthSquared() > dist * dist)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * dist;
        }


        return false;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (_arms.Count == 0)
        {
            for (int i = 0; i < ArmCount; ++i)
                _arms.Add(new SkelePrimeArm(NPC, i));
        }

        foreach (var item in _arms)
            item.Draw(screenPos);

        var tex = TextureAssets.Npc[Type].Value;
        var src = new Rectangle(0, 0, 140, 156);
        spriteBatch.Draw(tex, NPC.Center - screenPos, src, drawColor, NPC.rotation, src.Size() / 2f, 1f, 0, 0);

        return false;
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.SkeletronPrime." + Main.rand.Next(5));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public class SkelePrimeArm : Entity
    {
        private readonly NPC _head;
        private readonly int _handNum;

        public SkelePrimeArm(NPC head, int handNum)
        {
            _head = head;
            _handNum = handNum;

            Center = _head.Center;
            Size = ContentSamples.NpcsByNetId[GetArmType()].Size;
        }
        
        public void Update()
        {
            Vector2 off = GetArmType() switch
            {
                NPCID.PrimeCannon => new Vector2(120, -120),
                NPCID.PrimeSaw => new Vector2(-80, 200),
                NPCID.PrimeVice => new Vector2(80, 200),
                _ => new Vector2(-120, -120)
            };

            off *= MathF.Pow(MathF.Sin(_head.ai[0] * 0.015f + _handNum), 2) * 0.3f + 0.7f;

            if (position.Y > _head.position.Y + off.Y)
            {
                if (velocity.Y > 0f)
                    velocity.Y *= 0.96f;
                velocity.Y -= 0.07f;
                if (velocity.Y > 6f)
                    velocity.Y = 6f;
            }
            else if (position.Y < _head.position.Y + off.Y)
            {
                if (velocity.Y < 0f)
                    velocity.Y *= 0.96f;
                velocity.Y += 0.07f;
                if (velocity.Y < -6f)
                    velocity.Y = -6f;
            }

            if (Center.X > _head.Center.X + off.X)
            {
                if (velocity.X > 0f)
                    velocity.X *= 0.96f;
                velocity.X -= 0.18f;
                if (velocity.X > 8f)
                    velocity.X = 8f;
            }
            if (Center.X < _head.Center.X + off.X)
            {
                if (velocity.X < 0f)
                    velocity.X *= 0.96f;
                velocity.X += 0.18f;
                if (velocity.X < -8f)
                    velocity.X = -8f;
            }

            position += velocity;
        }

        public void Draw(Vector2 screenPos)
        {
            DrawArm(screenPos, out var rot);

            int armType = GetArmType();
            Main.instance.LoadNPC(armType);
            var armTex = TextureAssets.Npc[armType].Value;
            var src = new Rectangle(0, 0, armTex.Width, armTex.Height);
            var col = Lighting.GetColor(Center.ToTileCoordinates());

            if (armType == NPCID.PrimeSaw || armType == NPCID.PrimeVice)
                src = armTex.Frame(1, 2, 0, 0, 0, 0);

            Main.spriteBatch.Draw(TextureAssets.Npc[armType].Value, Center - screenPos, src, col, rot - MathHelper.Pi, src.Size() / 2f, 1f, SpriteEffects.None, 0f);
        }

        private void DrawArm(Vector2 screenPos, out float rot)
        {
            Vector2 basePosition = new(Center.X - 5f * _handNum, position.Y + 20f);
            rot = 0;

            for (int k = 0; k < 2; k++)
            {
                float offX = _head.Center.X - basePosition.X;
                float offY = _head.Center.Y - basePosition.Y;
                int side = GetArmSide();
                float length;

                if (k == 0)
                {
                    offX -= 200f * side;
                    offY += 130f;
                    length = (float)Math.Sqrt(offX * offX + offY * offY);
                    length = 92f / length;
                    basePosition.X += offX * length;
                    basePosition.Y += offY * length;
                }
                else
                {
                    offX -= 50f * side;
                    offY += 80f;
                    length = (float)Math.Sqrt(offX * offX + offY * offY);
                    length = 60f / length;
                    basePosition.X += offX * length;
                    basePosition.Y += offY * length;
                }

                float rotation = (float)Math.Atan2(offY, offX) - 1.57f;
                Color color7 = Lighting.GetColor((int)basePosition.X / 16, (int)(basePosition.Y / 16f));
                var src = new Rectangle(0, 0, TextureAssets.BoneArm.Width(), TextureAssets.BoneArm.Height());
                Main.spriteBatch.Draw(TextureAssets.BoneArm2.Value, basePosition - screenPos, src, color7, rotation, src.Size() / 2f, 1f, 0, 0f);
                
                if (k == 0)
                {
                    rot = rotation;
                    basePosition.X += offX * length / 2f;
                    basePosition.Y += offY * length / 2f;
                }
                else if (Main.instance.IsActive)
                {
                    basePosition.X += offX * length - 16f;
                    basePosition.Y += offY * length - 6f;
                }
            }
        }

        private int GetArmType()
        {
            return _handNum switch
            {
                0 => NPCID.PrimeLaser,
                1 => NPCID.PrimeVice,
                2 => NPCID.PrimeCannon,
                _ => NPCID.PrimeSaw
            };
        }

        private int GetArmSide()
        {
            return GetArmType() switch
            {
                NPCID.PrimeCannon or NPCID.PrimeVice => -1,
                _ => 1
            };
        }
    }
}
