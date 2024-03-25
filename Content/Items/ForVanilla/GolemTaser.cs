using BossForgiveness.Content.NPCs;
using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class GolemTaser : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(22);
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 20;
        Item.shoot = ModContent.ProjectileType<GolemTaserProj>();
        Item.shootSpeed = 8;
        Item.channel = true;
        Item.rare = ItemRarityID.Purple;
    }

    public override bool CanUseItem(Player player) => player.ownedProjectileCounts[ModContent.ProjectileType<GolemTaserProj>()] < 1;

    private class GolemTaserProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.Size = new(18);
            Projectile.timeLeft = 12000;
            Projectile.aiStyle = -1;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            var owner = Main.player[Projectile.owner];

            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(8))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.GoldFlame, Projectile.velocity.X, Projectile.velocity.Y);

            if (Projectile.DistanceSQ(owner.Center) > 600 * 600)
                owner.channel = false;

            if (!owner.channel)
            {
                Projectile.velocity = Projectile.DirectionTo(owner.Center) * 12;

                if (Projectile.DistanceSQ(owner.Center) < 30 * 30)
                {
                    Projectile.Kill();
                    return;
                }
            }
            else
            {
                for (int i = 0; i < Main.maxNPCs; ++i)
                {
                    NPC npc = Main.npc[i];

                    if (npc.active && npc.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        if (npc.type == NPCID.GolemHead || npc.type == NPCID.GolemHeadFree || npc.type == NPCID.GolemFistLeft || npc.type == NPCID.GolemFistRight || npc.type == NPCID.Golem)
                            npc.GetGlobalNPC<GolemPacificationNPC>().taserCount++;

                        owner.channel = false;

                        SoundEngine.PlaySound(SoundID.Item94 with { PitchRange = (0.8f, 1f), Volume = 0.7f });
                        break;
                    }
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Main.player[Projectile.owner].channel = false;
            return false;
        }
    }

    public class GolemPacificationNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        internal int taserCount = 0;

        public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.GolemHead || entity.type == NPCID.GolemHeadFree
            || entity.type == NPCID.Golem || entity.type == NPCID.GolemFistLeft || entity.type == NPCID.GolemFistRight;

        public override bool PreAI(NPC npc)
        {
            taserCount = (int)MathHelper.Clamp(taserCount, 0, npc.type == NPCID.Golem ? 20 : 6);

            if (npc.type == NPCID.GolemHead)
            {
                if (taserCount > 1)
                    npc.ai[0] = 1f;

                if (taserCount > 5 || Main.npc[NPC.golemBoss].GetGlobalNPC<GolemPacificationNPC>().taserCount > 10)
                    npc.Transform(NPCID.GolemHeadFree);
            }
            else if (npc.type == NPCID.GolemHeadFree)
            {
                if (taserCount > 5)
                {
                    npc.damage = 0;
                    npc.velocity.Y += 0.2f;
                    npc.rotation += npc.velocity.X * 0.02f;

                    return false;
                }
            }
            else if (npc.type == NPCID.Golem)
            {
                if (taserCount == 20)
                {
                    if (Main.netMode != NetmodeID.Server)
                        SoundEngine.PlaySound(SoundID.Item14, npc.Center);

                    npc.SimpleStrikeNPC(1, 0, false, 0, null, false, 0, true);
                    npc.NPCLoot();
                    npc.active = false;

                    for (int i = 0; i < 80; ++i)
                        SpawnGoldFlames(npc);

                    for (int j = 0; j < 12; ++j)
                        SpawnSmoke(npc);
                }
            }

            npc.GetGlobalNPC<SpeedUpBehaviourNPC>().behaviourSpeed += taserCount * 0.1f;
            npc.defense = (int)(npc.defDefense * (1 + taserCount * 0.5f));

            return true;
        }

        public override void OnKill(NPC npc)
        {
            for (int i = npc.whoAmI; i < Main.maxNPCs; ++i)
            {
                NPC nextNPC = Main.npc[i];

                if (nextNPC.active && nextNPC.type == NPCID.GolemHeadFree && nextNPC.GetGlobalNPC<GolemPacificationNPC>().taserCount > 5)
                {
                    for (int j = 0; j < 60; ++j)
                        SpawnGoldFlames(nextNPC);

                    for (int j = 0; j < 12; ++j)
                        SpawnSmoke(nextNPC);

                    nextNPC.active = false;
                }
            }
        }

        public override void DrawEffects(NPC npc, ref Color drawColor)
        {
            if (taserCount == 0)
                return;

            if (Main.rand.NextBool(Math.Max(65 - (taserCount * 7), 2)))
                SpawnGoldFlames(npc);

            if (taserCount > 10 && Main.rand.NextBool(Math.Max(30 - ((taserCount - 10) * 2), 3)))
                SpawnSmoke(npc);
        }

        private static void SpawnSmoke(NPC npc)
        {
            var pos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
            Gore.NewGorePerfect(npc.GetSource_FromAI(), pos, new Vector2(0, Main.rand.NextFloat(2, 5)).RotatedByRandom(MathHelper.TwoPi), GoreID.Smoke1);
        }

        private static void SpawnGoldFlames(NPC npc)
        {
            int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GoldFlame, 0, 0);
            Main.dust[dust].fadeIn = 0.2f;
            Main.dust[dust].scale = Main.rand.NextFloat(1.25f, 1.75f);
            Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(2, 3)).RotatedByRandom(MathHelper.TwoPi);
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (npc.type == NPCID.GolemHeadFree && taserCount > 5)
            {
                var tex = TextureAssets.Npc[npc.type].Value;
                var col = npc.GetAlpha(Lighting.GetColor(npc.Center.ToTileCoordinates()));

                Main.EntitySpriteDraw(tex, npc.Center - screenPos, npc.frame, col, npc.rotation, npc.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
                return false;
            }

            return true;
        }
    }

    public class GolemClickPlayer : ModPlayer
    {
        public override void PostUpdate()
        {
            if (Main.myPlayer != Player.whoAmI)
                return;

            if (!Player.HasItem(ItemID.LihzahrdPowerCell))
                return;

            for (int i = 0; i < Main.maxNPCs; ++i)
            {
                NPC npc = Main.npc[i];

                if (npc.active && npc.type == NPCID.GolemHeadFree && npc.GetGlobalNPC<GolemPacificationNPC>().taserCount > 5)
                {
                    bool click = Main.mouseRight && Main.mouseRightRelease;

                    if (click && npc.Hitbox.Contains(Main.MouseWorld.ToPoint()))
                    {
                        Player.ConsumeItem(ItemID.LihzahrdPowerCell, true, true);
                        npc.Transform(ModContent.NPCType<GolemHeadPacified>());
                    }
                }
            }
        }
    }
}
