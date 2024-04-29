using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class EoLPacified : ModNPC
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.HallowBoss}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_37";

    private ref float Timer => ref NPC.ai[0];
    private ref float NetTimer => ref NPC.ai[1];
    private ref float State => ref NPC.ai[2];

    NPC _dummy = null;

    public override void SetStaticDefaults() => NPCID.Sets.IsTownPet[Type] = true;

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.HallowBoss);
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
        if (NetTimer++ > 600) // Sync occasionally to be sure
        {
            NPC.netUpdate = true;
            NetTimer = 0;
        }

        SetDummy();

        Timer++;

        if (NPC.homeless)
        {
            if (State == 0)
            {
                if (!NPC.IsBeingTalkedTo())
                    FloatMovement(null);
                else
                    NPC.velocity *= 0.7f;

                if (Timer >= 600f)
                {
                    TeleportFX();

                    State = 1;
                    Timer = 0;
                    _dummy.Opacity = 0;
                    NPC.velocity.Y = 0.1f;
                }
            }
            else if (State == 1)
            {
                if (Timer == 1)
                {
                    NPC.position.X -= Main.rand.Next(400, 600) * (Main.rand.NextBool(2) ? -1 : 1);

                    while (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                        NPC.position.Y -= 16;
                }
                else if (Timer == 2)
                {
                    TeleportFX();

                    State = 0;
                    Timer = 0;
                    _dummy.Opacity = 1f;
                }
            }
        }
        else
        {
            Vector2 home = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates();

            if (State == 0)
            {
                if (NPC.DistanceSQ(home) > 4000 * 4000)
                {
                    TeleportFX();

                    State = 1;
                    Timer = 0;
                    _dummy.Opacity = 0f;
                    NPC.velocity.Y = 0.1f;
                }

                if (!NPC.IsBeingTalkedTo())
                {
                    FloatMovement(NPC.homeTileY * 16);
                    NPC.velocity.X = MathF.Cos(Timer * 0.03f) * 10;
                }
                else
                    NPC.velocity *= 0.7f;
            }
            else if (State == 1)
            {
                if (Timer == 1)
                    NPC.Center = home - new Vector2(0, 200);
                else if (Timer == 2)
                {
                    TeleportFX();

                    State = 0;
                    Timer = 0;
                    _dummy.Opacity = 1f;
                }
            }
        }

        return false;
    }

    private void FloatMovement(float? minHeight)
    {
        NPC.TargetClosest(false);
        NPC.FaceTarget();
        NPC.spriteDirection = NPC.direction;
        float sin = MathF.Sin(Timer * 0.05f) * 20;

        if (NPC.IsBeingTalkedTo())
            sin *= 0.25f;

        float yVel = MathHelper.Min(((NPC.GetFloor(80, true) - 15 + sin) * 16 - NPC.Center.Y), minHeight ?? float.MaxValue) / 300f;
        NPC.velocity = new Vector2(0, yVel);
    }

    private void TeleportFX()
    {
        if (Main.netMode != NetmodeID.Server)
        {
            for (int i = 0; i < 150; ++i)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.HallowSpray);

            SoundEngine.PlaySound(SoundID.NPCHit5, NPC.Center);
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        SetDummy();

        _dummy.Center = NPC.Center;
        _dummy.velocity = NPC.velocity;
        _dummy.frameCounter++;
        _dummy.ai[0] = 1;
        _dummy.localAI[0]++;
        _dummy.FindFrame();
        Main.instance.DrawNPCDirect(Main.spriteBatch, _dummy, false, screenPos);
        return false;
    }

    private void SetDummy()
    {
        if (_dummy is null)
        {
            _dummy = new();
            _dummy.SetDefaults(NPCID.HallowBoss);
            _dummy.IsABestiaryIconDummy = NPC.IsABestiaryIconDummy;
            _dummy.Opacity = 1f;
        }
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.EoL." + Main.rand.Next(7));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
