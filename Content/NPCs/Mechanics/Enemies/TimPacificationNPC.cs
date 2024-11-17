using BossForgiveness.Content.NPCs.Vanilla.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Enemies;

internal class TimPacificationNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Tim;

    public int runesCaught = 0;

    public override void SetDefaults(NPC entity) => entity.damage = 0;

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            for (int i = 0; i < 3; ++i)
                Projectile.NewProjectile(source, npc.Center, Vector2.Zero, ModContent.ProjectileType<TimRune>(), 0, 0, Main.myPlayer, npc.whoAmI);
        }
    }

    public override bool PreAI(NPC npc)
    {
        if (npc.life < npc.lifeMax)
            return true;

        if (runesCaught > 2)
        {
            npc.Pacify<TimPacified>();
            return false;
        }

        return true;
    }

    public class TimRune : ModProjectile
    {
        private NPC Tim => Main.npc[(int)Projectile.ai[0]];

        // Emphasis on TIMer
        private ref float Timer => ref Projectile.ai[1];

        public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

        public override void SetDefaults()
        {
            Projectile.Size = new Vector2(22, 20);
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (!Tim.active || Tim.type != NPCID.Tim || Tim.life < Tim.lifeMax)
            {
                Projectile.Kill();
                return;
            }

            Projectile.timeLeft++;
            Projectile.velocity.Y = MathF.Sin(Timer * 0.02f) * 0.3f;

            if (Main.rand.NextBool(40))
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);

            if (Tim.ai[0] == 1f)
                Teleport();

            if (Timer == 5)
                Projectile.frame = Main.rand.Next(3);
            else if (Timer % 6 == 0)
                Projectile.frame = (Projectile.frame + 1) % 3;

            Timer++;

            DustAndHitboxChecks();
        }

        private void DustAndHitboxChecks()
        {
            Vector2 pos = Projectile.Center;
            Vector2 vel = Projectile.DirectionTo(Tim.Center);
            bool fieldActive = Timer % 300 < 150;

            if (Timer % 8 == 0 && fieldActive)
            {
                while (pos.DistanceSQ(Tim.Center) > 15 * 15)
                {
                    float dist = pos.Distance(Projectile.Center);
                    float mod = 1f;

                    if (dist < 120f)
                    {
                        mod = dist / 120f;
                    }
                    else
                    {
                        dist = pos.Distance(Tim.Center);

                        if (dist < 120f)
                        {
                            mod = dist / 120f;
                        }
                    }

                    Dust.NewDustPerfect(pos, DustID.Shadowflame, Main.rand.NextVector2Circular(0.5f, 0.5f), 0, default, Main.rand.NextFloat(0.8f, 1.2f) * mod);

                    pos += vel * 15;
                }
            }

            foreach (Player plr in Main.ActivePlayers)
            {
                if (plr.Hitbox.Intersects(Projectile.Hitbox))
                {
                    Dust.NewDustPerfect(Projectile.position, DustID.Shadowflame, Main.rand.NextVector2CircularEdge(8, 8) * Main.rand.NextFloat(0.7f, 1f));

                    Tim.GetGlobalNPC<TimPacificationNPC>().runesCaught++;
                    Projectile.Kill();
                    break;
                }

                if (fieldActive && Collision.CheckAABBvLineCollision(plr.position, plr.Size, Projectile.Center + vel * 60, Tim.Center))
                {
                    plr.Hurt(PlayerDeathReason.ByCustomReason(Language.GetTextValue("Mods.BossForgiveness.TimLaserHurtReason")), 20, Projectile.Center.X > plr.Center.X ? -1 : 1);
                }
            }
        }

        private void Teleport()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Vector2 pos;

            do
            {
                pos = Tim.Center + Main.rand.NextVector2Circular(500, 500);
            } while (Collision.SolidCollision(pos, 22, 20));

            Projectile.position = pos;
            Projectile.netUpdate = true;

            for (int i = 0; i < 10; ++i)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Shadowflame);
            }
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            for (int i = 0; i < 3; ++i)
            {
                var frame = new Rectangle(0, (Projectile.frame + i) % 3 * 22, 22, 20);
                float sine = MathF.Sin(Timer * 0.05f + i * MathHelper.PiOver4) * 0.1f;
                Color color = Color.White * ((i + 1) / 3f);
                float scale = 1.1f + sine;
                Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, color, Projectile.rotation, frame.Size() / 2f, scale, SpriteEffects.None);
            }

            return false;
        }
    }
}
