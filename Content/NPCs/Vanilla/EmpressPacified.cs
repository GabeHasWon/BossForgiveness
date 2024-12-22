using BossForgiveness.Content.NPCs.Mechanics;
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
public class EmpressPacified : ModNPC
{
    public const float MaxTeleportTime = 180;

    public override string Texture => $"Terraria/Images/NPC_{NPCID.HallowBoss}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_37";

    private ref float Timer => ref NPC.ai[0];
    private ref float TeleportTimer => ref NPC.ai[1];
    private ref float XSpeed => ref NPC.ai[2];

    NPC _drawNPCDummy = null;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 16;

        NPCID.Sets.IsTownPet[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.HallowBoss);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.netAlways = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = NPCID.HallowBoss;
    }

    public override bool PreAI()
    {
        if (_drawNPCDummy is not null && _drawNPCDummy.ai[0] == 0)
            _drawNPCDummy.VanillaAI();

        if (TeleportTimer > 0)
        {
            const float HalfMax = MaxTeleportTime / 2f;

            TeleportTimer--;

            if (NPC.homeless)
            {
                TeleportTimer = 0;
                NPC.Opacity = 1f;
                return false;
            }

            NPC.Opacity = Math.Abs((TeleportTimer - HalfMax) / HalfMax);

            if (TeleportTimer == HalfMax)
                NPC.Center = new Vector2(NPC.homeTileX, NPC.homeTileY) * 16;

            return false;
        }

        Timer++;
        NPC.Opacity = 1f;

        if (!NPC.IsBeingTalkedTo())
        {
            int floor = NPC.GetFloor(40, true);
            int y = (!NPC.homeless ? NPC.homeTileY : floor) * 16;
            float dist = y - NPC.Center.Y;

            if (dist < 16 * 10)
                NPC.velocity.Y -= 0.35f;
            else if (dist > 16 * 15)
                NPC.velocity.Y += 0.35f;
            else
                NPC.velocity.Y *= 0.9f;

            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -6, 4);
        }
        else
            NPC.velocity *= 0.8f;

        if (NPC.homeless)
            IdleBehaviour();
        else
        {
            float homeX = NPC.homeTileX * 16;

            if (NPC.DistanceSQ(new Vector2(homeX, NPC.homeTileY * 16)) > 2400 * 2400)
                TeleportTimer = MaxTeleportTime;

            IdleBehaviour();
        }

        return false;
    }

    private void IdleBehaviour()
    {
        if (!NPC.IsBeingTalkedTo())
        {
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, XSpeed, 0.05f);

            if (Timer % 60 == 0)
            {
                XSpeed = Main.rand.NextFloat(-3, 3f);
                NPC.netUpdate = true;
            }
        }
    }

    public override void FindFrame(int frameHeight)
    {
        if (_drawNPCDummy is not null && _drawNPCDummy.ai[0] != 0)
            _drawNPCDummy.localAI[0]++;
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Empress." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
            return false;

        if (_drawNPCDummy is null)
            SetDummy();

        _drawNPCDummy.Center = NPC.Center;
        _drawNPCDummy.velocity = NPC.velocity;
        _drawNPCDummy.Opacity = NPC.Opacity;
        _drawNPCDummy.FindFrame();
        Main.instance.DrawNPCDirect(Main.spriteBatch, _drawNPCDummy, false, screenPos);
        return false;
    }

    private void SetDummy()
    {
        _drawNPCDummy = new();
        _drawNPCDummy.SetDefaults(NPCID.HallowBoss);
    }

    public override void ModifyHoverBoundingBox(ref Rectangle boundingBox)
    {
        boundingBox.Y -= NPC.height;
        boundingBox.Height = NPC.height * 2;
    }
}
