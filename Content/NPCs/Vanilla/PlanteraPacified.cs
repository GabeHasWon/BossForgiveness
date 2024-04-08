using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class PlanteraPacified : ModNPC
{
    /// <summary>
    /// Wrapper to handle controlled Plantera Hook enemies.
    /// </summary>
    /// <param name="npc">The NPC to control.</param>
    internal class PlanteraHook(NPC npc)
    {
        internal readonly NPC dummy = npc;

        internal void Update(int plantera, int hookSlot)
        {
            dummy.active = true;
            int oldBoss = NPC.plantBoss;
            NPC.plantBoss = plantera; // Needed for the hook AI to work properly
            int oldNetmode = Main.netMode;
            Main.netMode = NetmodeID.SinglePlayer; // Workaround for net issues

            NPC boss = Main.npc[plantera];
            int life = boss.life;
            boss.life = boss.lifeMax;

            dummy.UpdateNPC(0);
            dummy.alpha = boss.alpha;
            dummy.localAI[0] -= 0.4f;

            Player plr = Main.player[boss.target];

            if ((!boss.homeless || !plr.active || plr.dead || plr.DistanceSQ(boss.Center) > 800 * 800) && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (dummy.localAI[0] == 10f)
                {
                    int reps = 0;

                    while (true)
                    {
                        reps++;

                        if (reps > 500)
                        {
                            boss.velocity.Y += 0.6f;
                            dummy.ai[0] = boss.Center.X / 16 + Main.rand.Next(-20, 20);
                            dummy.ai[1] = boss.Center.Y / 16 + Main.rand.Next(15, 30);
                            break;
                        }

                        dummy.ai[0] = boss.Center.X / 16 + Main.rand.Next(-20, 20);
                        dummy.ai[1] = boss.Center.Y / 16 + Main.rand.Next(-20, 20);
                        dummy.localAI[0] = Main.rand.Next(300, 600);

                        if (Collision.SolidCollision(new Vector2(dummy.ai[0], dummy.ai[1]) * 16 + dummy.Size / 2f, 8, 8))
                            break;
                    }

                    dummy.ai[0] = MathHelper.Clamp(dummy.ai[0], Main.offLimitBorderTiles, Main.maxTilesX - Main.offLimitBorderTiles);
                    dummy.ai[1] = MathHelper.Clamp(dummy.ai[1], Main.offLimitBorderTiles, Main.maxTilesY - Main.offLimitBorderTiles);
                    dummy.netUpdate = true;
                }
            }

            Main.netMode = oldNetmode;
            NPC.plantBoss = oldBoss;
            boss.life = life;

            if (dummy.netUpdate)
            {
                dummy.netUpdate = false;
                new SyncPlanteraHookModule(plantera, hookSlot, new Vector2(dummy.ai[0], dummy.ai[1])).Send();
            }
        }

        internal void Draw(int plantera)
        {
            Vector2 drawPos = dummy.Center;
            float num15 = Main.npc[plantera].Center.X - drawPos.X;
            float num16 = Main.npc[plantera].Center.Y - drawPos.Y;
            float rotation2 = (float)Math.Atan2(num16, num15) - 1.57f;
            bool flag3 = true;

            while (flag3)
            {
                int num17 = 16;
                int num2 = 32;
                float num3 = (float)Math.Sqrt(num15 * num15 + num16 * num16);

                if (num3 < (float)num2)
                {
                    num17 = (int)num3 - num2 + num17;
                    flag3 = false;
                }

                num3 = num17 / num3;
                num15 *= num3;
                num16 *= num3;
                drawPos.X += num15;
                drawPos.Y += num16;
                num15 = Main.npc[plantera].Center.X - drawPos.X + Main.npc[plantera].netOffset.X;
                num16 = Main.npc[plantera].Center.Y - drawPos.Y + Main.npc[plantera].netOffset.Y;

                Color col = Lighting.GetColor(drawPos.ToTileCoordinates());
                var src = new Rectangle(0, 0, TextureAssets.Chain26.Width(), num17);
                Main.spriteBatch.Draw(TextureAssets.Chain26.Value, drawPos - Main.screenPosition, src, col, rotation2, TextureAssets.Chain26.Size() / 2f, 1f, 0, 0f);
            }

            Main.instance.DrawNPCDirect(Main.spriteBatch, dummy, false, Main.screenPosition);
        }
    }
    public override string Texture => $"Terraria/Images/NPC_{NPCID.Plantera}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_11";

    private ref float Timer => ref NPC.ai[0];
    private ref float RotSpeed => ref NPC.ai[1];
    private ref float RotTime => ref NPC.ai[2];

    internal List<PlanteraHook> planteraHooks = [];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 8;

        NPCID.Sets.MustAlwaysDraw[Type] = true;
        NPCID.Sets.IsTownPet[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.EyeofCthulhu);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.noTileCollide = true;
        NPC.netAlways = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = frameHeight * (int)(NPC.frameCounter / 10f % 4);
    }

    public override bool PreAI()
    {
        const float FocusDistance = 600;

        Timer++;
        Lighting.AddLight(NPC.Center, new Vector3(0.3f, 0.1f, 0.1f));
        NPC.TargetClosest();
        NPC.netUpdate = true;

        for (int i = 0; i < planteraHooks.Count; i++)
        {
            PlanteraHook item = planteraHooks[i];
            item.Update(NPC.whoAmI, i);
        }

        if (planteraHooks.Count == 0)
            AddHooks(3);

        bool awayFromHooks = planteraHooks.All(x => x.dummy.DistanceSQ(NPC.Center) > FocusDistance * FocusDistance);
        Vector2 average = NPC.Center;

        if (awayFromHooks)
        {
            average = Vector2.Zero;

            foreach (var hook in planteraHooks)
                average += hook.dummy.Center;

            average /= planteraHooks.Count;
        }

        var plr = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];

        if (NPC.homeless)
        {
            if (!NPC.IsBeingTalkedTo())
            {
                if (plr.active && !plr.dead && plr.DistanceSQ(NPC.Center) < FocusDistance * FocusDistance)
                {
                    NPC.velocity += NPC.DirectionTo(plr.Center + NPC.DirectionFrom(plr.Center) * 200) * 0.12f;
                    NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.AngleTo(plr.Center) + MathHelper.PiOver2, 0.1f);
                }
                else
                {
                    NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);
                    NPC.velocity *= 0.99f;
                    NPC.velocity = NPC.velocity.RotatedBy(RotSpeed);

                    if (NPC.velocity.LengthSquared() < 2 * 2)
                        NPC.velocity = NPC.velocity.SafeNormalize(Vector2.Zero) * 2f;

                    if (Timer % RotTime == 0)
                    {
                        RotTime = Main.rand.Next(120, 420);
                        RotSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
                        NPC.netUpdate = true;
                    }
                }

                if (awayFromHooks)
                    NPC.velocity += NPC.DirectionTo(average) * 1.2f;
            }
            else
                NPC.velocity *= 0.9f;
        }
        else
        {
            NPC.rotation = plr.active && !plr.dead && plr.DistanceSQ(NPC.Center) < FocusDistance * FocusDistance
                ? Utils.AngleLerp(NPC.rotation, NPC.AngleTo(plr.Center) + MathHelper.PiOver2, 0.1f)
                : Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() + MathHelper.PiOver2, 0.1f);

            Vector2 home = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates();
            NPC.velocity += NPC.DirectionTo(home) * 0.05f;
            NPC.velocity = NPC.velocity.RotatedBy(RotSpeed);

            if (Timer % RotTime == 0)
            {
                RotTime = Main.rand.Next(120, 420);
                RotSpeed = Main.rand.NextFloat(-0.03f, 0.03f);
                NPC.netUpdate = true;
            }
        }

        if (NPC.velocity.LengthSquared() > 8 * 8)
            NPC.velocity = Vector2.Normalize(NPC.velocity) * 8;
        return false;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        foreach (var item in planteraHooks)
            item.Draw(NPC.whoAmI);

        return true;
    }

    private void AddHooks(int count)
    {
        for (int i = 0; i < count; ++i)
        {
            NPC newNPC = new() { position = NPC.position, velocity = new Vector2(0, Main.rand.NextFloat(1, 4)).RotatedByRandom(MathF.Tau) };
            newNPC.SetDefaults(NPCID.PlanterasHook);
            newNPC.ai[0] = newNPC.Center.X / 16f;
            newNPC.ai[1] = newNPC.Center.Y / 16f;
            planteraHooks.Add(new PlanteraHook(newNPC));
        }
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override bool CheckConditions(int left, int right, int top, int bottom) => bottom > Main.worldSurface;
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Plantera." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
