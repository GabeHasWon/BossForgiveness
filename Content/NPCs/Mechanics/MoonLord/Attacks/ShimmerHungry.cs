using NPCUtils;
using System;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Attacks;

internal class ShimmerHungry : ModNPC
{
    private ref float Timer => ref NPC.ai[0];

    private Vector2 TargetPosition
    {
        get => new(NPC.ai[1], NPC.ai[2]);
        set => (NPC.ai[1], NPC.ai[2]) = (value.X, value.Y);
    }

    private Player Target => Main.player[NPC.target];

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 2;

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.TheHungryII);
        NPC.aiStyle = -1;
        NPC.noTileCollide = true;
        NPC.defense = 0;

        AIType = NPCID.None;
    }

    public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
    {
        NPC.lifeMax = 500;
        NPC.damage = 150;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "");

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = frameHeight * ((int)(NPC.frameCounter * 0.1f) % 2);
    }

    public override void AI()
    {
        Timer++;

        if (Timer < 5)
            NPC.TargetClosest();

        if (Timer < 30)
        {
            NPC.velocity = NPC.DirectionTo(Target.Center) * (Timer + 1) / 30f * 0.2f;
        }
        else
        {
            if (Timer < 90)
                TargetPosition = Target.Center;

            NPC.velocity = NPC.DirectionTo(TargetPosition) * (MathF.Min((Timer - 30) / 3f, 40) + 0.2f);

            if (NPC.DistanceSQ(TargetPosition) < 45 * 45)
            {
                Point16 npcPos = NPC.position.ToTileCoordinates16();

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = npcPos.X - 1; i < npcPos.X + 3; ++i)
                    {
                        for (int j = npcPos.Y - 1; j < npcPos.Y + 3; ++j)
                        {
                            if (i == npcPos.X - 1 || i == npcPos.X + 2 || j == npcPos.Y - 1 || j == npcPos.Y + 2)
                            {
                                if (Main.rand.NextBool(2))
                                    continue;
                            }

                            WorldGen.PlaceTile(i, j, TileID.ShimmerBlock, true, true);
                        }
                    }
                }

                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendTileSquare(-1, npcPos.X, npcPos.Y, 2, 2);
                else
                {
                    SoundEngine.PlaySound(SoundID.Shatter with { Volume = 0.5f, PitchRange = (-0.5f, 0) });
                    SoundEngine.PlaySound(SoundID.Dig with { PitchRange = (-0.3f, 0.3f) });
                }

                NPC.active = false;
                NPC.netUpdate = true;

                for (int i = 0; i < 12; ++i)
                    SpawnDust(2f);
            }    
        }

        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.Pi;

        if (Main.rand.NextFloat() < NPC.velocity.Length() / 40f)
            SpawnDust();
    }

    private void SpawnDust(float scale = 1f)
    {
        Dust dust = Main.dust[Dust.NewDust(NPC.position, NPC.width, NPC.height, !Main.rand.NextBool(3) ? DustID.ShimmerSplash : DustID.ShimmerTorch)];
        dust.noGravity = true;
        dust.scale = Main.rand.NextFloat(1.5f, 3f) * scale;
        dust.velocity = Main.rand.NextVector2Circular(5, 5) * scale;
    }
}
