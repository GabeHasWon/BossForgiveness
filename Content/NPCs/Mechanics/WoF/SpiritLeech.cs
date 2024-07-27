using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NPCUtils;
using System;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.WoF;

public class SpiritLeech : ModNPC
{
    private NPC Parent => Main.npc[(int)ParentWho];

    private ref float SegmentType => ref NPC.ai[0];
    private ref float ParentWho => ref NPC.ai[1];

    private bool Initialized
    {
        get => NPC.ai[2] == 1;
        set => NPC.ai[2] = value ? 1 : 0;
    }

    private bool TouchedPlayer
    {
        get => NPC.ai[3] == 1;
        set => NPC.ai[3] = value ? 1 : 0;
    }

    public bool isSpirit = false;
    public int timer = 0;

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.Size = new Vector2(18);
        NPC.lifeMax = 200;
        NPC.defense = 5;
        NPC.noGravity = true;
        NPC.dontCountMe = true;
        NPC.friendly = false;
        NPC.dontTakeDamageFromHostiles = true;
        NPC.noTileCollide = true;
        NPC.behindTiles = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "TheUnderworld");

    public override void AI()
    {
        timer++;

        if (SegmentType == 0)
        {
            const float MaxSpeed = 12;

            if (!Initialized)
            {
                const int Max = 8;

                int parent = NPC.whoAmI;

                for (int i = 0; i < Max; ++i)
                {
                    int segmentType = i == Max - 1 ? 2 : 1;
                    parent = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y, Type, NPC.whoAmI, segmentType, parent);
                    var spiritLeech = Main.npc[parent].ModNPC as SpiritLeech;
                    spiritLeech.isSpirit = isSpirit;
                }

                Initialized = true;

                if (!isSpirit)
                {
                    NPC.damage = 15;
                    NPC.defDamage = 50;
                }
                else
                {
                    NPC.Opacity = 0.25f;
                    int index = NPC.whoAmI + 1;

                    while (Main.npc[index].type == Type)
                        Main.npc[index++].Opacity = 0.25f;
                }
            }

            if (!TouchedPlayer)
            {
                NPC.TargetClosest();
                NPC.velocity += NPC.DirectionTo(Main.player[NPC.target].Center) * 0.65f;

                if (isSpirit)
                    CheckHitPlayers();
            }
            else
            {
                NPC.velocity += NPC.DirectionTo(Main.npc[Main.wofNPCIndex].Center) * 0.8f;
                CheckHitWoF();
            }

            if (NPC.velocity.LengthSquared() >= MaxSpeed * MaxSpeed)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * MaxSpeed;

            NPC.rotation = NPC.velocity.ToRotation() - MathHelper.Pi;

            if (timer == 300)
                NPC.active = false;
            else if (timer == 295)
            {
                NPC.position = NPC.Center;
                NPC.Size = new Vector2(140);
                NPC.position -= NPC.Size / 2;
                NPC.damage = 90;
                NPC.hide = true;

                int index = NPC.whoAmI + 1;

                while (Main.npc[index].type == Type)
                {
                    if (Main.netMode != NetmodeID.Server)
                        Gore.NewGore(Main.npc[index].GetSource_Death(), Main.npc[index].Center, Main.npc[index].velocity, 135);

                    Main.npc[index++].hide = true;
                }

                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode, NPC.position);

                for (int i = 0; i < 40; ++i)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(4f, 4f);
                    Dust.NewDust(NPC.Center, 4, 4, DustID.Torch, vel.X, vel.X);

                    if (i % 10 == 0 && Main.netMode != NetmodeID.Server)
                        Gore.NewGore(NPC.GetSource_Death(), NPC.Center, vel, GoreID.Smoke1 + Main.rand.Next(3));
                }
            }
        }
        else
        {
            if (ParentWho == -1 || !Parent.active || Parent.type != Type)
            {
                NPC.active = false;
                return;
            }

            if (Parent.DistanceSQ(NPC.Center) > 14 * 14)
                NPC.Center = Parent.Center + Parent.DirectionTo(NPC.Center) * 14;

            NPC.rotation = Parent.AngleFrom(NPC.Center);
        }
    }

    public override void OnKill() => Gore.NewGore(NPC.GetSource_Death(), NPC.Center, NPC.velocity, 135);

    private void CheckHitWoF()
    {
        if (Main.npc[Main.wofNPCIndex].Hitbox.Intersects(NPC.Hitbox))
        {
            Main.npc[Main.wofNPCIndex].GetGlobalNPC<WoFPacificationNPC>().AddPacification(Main.npc[Main.wofNPCIndex]);
            NPC.active = false;

            SoundEngine.PlaySound(SoundID.NPCHit54, NPC.Center);

            int index = NPC.whoAmI;

            while (Main.npc[index].type == Type)
            {
                if (Main.netMode != NetmodeID.Server)
                    Gore.NewGore(Main.npc[index].GetSource_Death(), Main.npc[index].Center, Vector2.Zero, GoreID.Smoke1);
                Main.npc[index++].hide = true;
            }
        }
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write(isSpirit);
    public override void ReceiveExtraAI(BinaryReader reader) => isSpirit = reader.ReadBoolean();

    private void CheckHitPlayers()
    {
        foreach (var plr in Main.ActivePlayers)
        {
            if (plr.Hitbox.Intersects(NPC.Hitbox))
            {
                TouchedPlayer = true;
                return;
            }
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = TextureAssets.Npc[Type].Value;
        Rectangle frame = SegmentType switch
        {
            0 => new Rectangle(0, 0, 20, 18),
            1 => new Rectangle(20, 0, 20, 18),
            _ => new Rectangle(40, 0, 22, 18)
        };

        SpriteEffects effect = SegmentType == 2 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
         
        if (isSpirit)
            frame.Y += 20;

        if (NPC.IsABestiaryIconDummy)
            drawColor = Color.White;

        float scale = MathF.Sin(timer * 0.3f + NPC.whoAmI * MathHelper.PiOver4) * 0.2f;
        spriteBatch.Draw(tex, NPC.Center - screenPos, frame, drawColor * NPC.Opacity, NPC.rotation, frame.Size() / 2f, NPC.scale + 0.2f + scale, effect, 0);

        return false;
    }
}
