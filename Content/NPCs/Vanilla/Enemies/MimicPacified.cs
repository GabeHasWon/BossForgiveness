using BossForgiveness.Content.Items.ForVanilla;
using BossForgiveness.Content.NPCs.Mechanics.Enemies;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
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

    private ref float XVel => ref NPC.ai[0];

    private Projectile _bell = null;
    private byte? _hasBell = null;

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
        AIType = -1;
    }

    public override bool PreAI()
    {
        if (NPC.homeless)
            JumpAround();
        else
        {
            if (NPC.DistanceSQ(NPC.HomeTileWorld()) > 2000 * 2000)
            {
                NPC.velocity.Y -= 0.6f;
                NPC.velocity.X *= 0.9f;
                NPC.Opacity *= 0.9f;

                if (NPC.Opacity < 0.2f)
                {
                    NPC.Opacity = 1f;
                    NPC.Center = NPC.HomeTileWorld();

                    for (int i = 0; i < 20; ++i)
                        Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCoin);

                    SoundEngine.PlaySound(SoundID.CoinPickup, NPC.Center);
                }
            }
            else
                JumpAround();
        }

        return false;
    }

    private void JumpAround()
    {
        if (NPC.velocity.Y == 0)
        {
            if (!NPC.IsBeingTalkedTo() && Main.time % 1200 > 100)
                NPC.ai[2]++;

            if (NPC.ai[2] >= 20)
            {
                if (!JumpDirectionIsLeft(out float speed))
                    XVel = 2 * speed;
                else
                    XVel = -2 * speed;

                NPC.velocity.Y = -3.5f * speed;
                NPC.ai[2] = 0;
                NPC.netUpdate = true;
            }
            else
                NPC.velocity.X *= 0.9f;
        }
        else
            NPC.velocity.X = XVel;

        NPC.direction = Math.Sign(NPC.velocity.X);
        NPC.spriteDirection = NPC.direction;

        if (_hasBell.HasValue)
        {
            if (Main.player[_hasBell.Value].Hitbox.Intersects(NPC.Hitbox))
            {
                Main.player[_hasBell.Value].QuickSpawnItem(NPC.GetSource_GiftOrReward(), ModContent.ItemType<MimicBell>());

                NPC.netUpdate = true;
                _hasBell = null;
            }
        }
        else
        {
            if (_bell is not null && (!_bell.active || _bell.type != ModContent.ProjectileType<MimicBell.MimicBellProj>() || _bell.DistanceSQ(NPC.Center) > 800 * 800))
            {
                _bell = null;

                NPC.netUpdate = true;
                return;
            }

            if (_bell is not null && _bell.Hitbox.Intersects(NPC.Hitbox))
            {
                _bell.Kill();
                _hasBell = (byte)_bell.owner;

                NPC.netUpdate = true;
                SoundEngine.PlaySound(SoundID.CoinPickup);
            }
        }
    }

    private bool JumpDirectionIsLeft(out float speed)
    {
        Projectile closest = null;
        speed = 1;

        if (_hasBell.HasValue)
            return Main.player[_hasBell.Value].Center.X < NPC.Center.X;

        foreach (Projectile proj in Main.ActiveProjectiles)
        {
            float dist = proj.DistanceSQ(NPC.Center);

            if (proj.type == ModContent.ProjectileType<MimicBell.MimicBellProj>() && dist < 400 * 400 && (closest is null || closest.DistanceSQ(NPC.Center) > dist))
                closest = proj;
        }

        if (closest is not null)
        {
            speed = 1.5f;
            _bell = closest;
            return closest.Center.X < NPC.Center.X;
        }

        return Main.rand.NextBool();
    }

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(_hasBell ?? 255);
        writer.Write(_bell.identity);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        byte hasBell = reader.ReadByte();
        _hasBell = hasBell == 255 ? null : hasBell;

        short id = reader.ReadInt16();
        _bell = Main.projectile.FirstOrDefault(x => x.identity == id);
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
    public override bool? CanFallThroughPlatforms() => true;
}
