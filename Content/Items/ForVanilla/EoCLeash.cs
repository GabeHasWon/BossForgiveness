﻿using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class EoCLeash : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(22);
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 20;
        Item.shoot = ModContent.ProjectileType<EoCLeashProj>();
        Item.shootSpeed = 12;
        Item.channel = true;
    }

    private class EoCLeashProj : ModProjectile
    {
        public override string Texture => base.Texture.Replace("Proj", "");

        public override void SetDefaults()
        {
            Projectile.Size = new(44);
            Projectile.timeLeft = 12000;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            var owner = Main.player[Projectile.owner];

            if (Projectile.DistanceSQ(owner.Center) > 1200 * 1200)
                owner.channel = false;

            if (!owner.channel)
            {
                Projectile.velocity = Projectile.DirectionTo(owner.Center) * 16;

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

                    if (npc.active && npc.type == ModContent.NPCType<EyePacified>() && npc.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        npc.ai[1] = 1;
                        npc.ai[2] = Projectile.owner;

                        owner.GetModPlayer<EoCLassoPlayer>().ridingEoC = i;
                        owner.GetModPlayer<EoCLassoPlayer>().eoCVelocity = npc.velocity;

                        if (owner.mount.Active)
                            owner.QuickMount();

                        Projectile.Kill();
                        return;
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

    public class EoCLassoPlayer : ModPlayer
    {
        internal static bool OverrideDraw = false;

        private NPC Steed => Main.npc[ridingEoC];
        private bool OnEoC => ridingEoC != -1;

        public int ridingEoC = -1;
        public Vector2 eoCVelocity = Vector2.Zero;

        public override void Load() => On_LegacyPlayerRenderer.DrawPlayerFull += HijackFull;

        // Weird workaround for not rendering full player
        private void HijackFull(On_LegacyPlayerRenderer.orig_DrawPlayerFull orig, LegacyPlayerRenderer self, Camera camera, Player drawPlayer)
        {
            var modPlayer = drawPlayer.GetModPlayer<EoCLassoPlayer>();

            if (modPlayer.OnEoC && !OverrideDraw)
                return;

            orig(self, camera, drawPlayer);
        }

        public override void ModifyScreenPosition()
        {
            if (OnEoC)
                Main.screenPosition = Steed.Center - Main.ScreenSize.ToVector2() / 2f;
        }

        public override void PreUpdateMovement()
        {
            if (OnEoC)
            {
                bool steedInvalid = !Steed.active || Steed.type != ModContent.NPCType<EyePacified>() || Steed.life <= 0;

                if (steedInvalid || Player.controlMount || Player.grappling[0] >= 0 || Player.controlJump)
                {
                    (Steed.ModNPC as EyePacified).Unmount();
                    ridingEoC = -1;
                    return;
                }

                if (Steed.collideX)
                    eoCVelocity.X = 0;

                if (Steed.collideY)
                    eoCVelocity.Y = 0;

                if (Player.controlUp)
                    eoCVelocity.Y -= 0.6f;
                
                if (Player.controlDown)
                    eoCVelocity.Y += 0.6f;
                
                if (Player.controlLeft)
                    eoCVelocity.X -= 0.6f;
                if (Player.controlRight)
                    eoCVelocity.X += 0.6f;
                
                if (!Player.controlDown && !Player.controlLeft && !Player.controlRight && !Player.controlUp)
                    eoCVelocity *= 0.99f;

                eoCVelocity = Vector2.Clamp(eoCVelocity, new Vector2(-12), new Vector2(12));

                Steed.velocity = eoCVelocity;
                Player.gravity = 0;
                Player.velocity = Vector2.Zero;
                Player.Center = (Steed.ModNPC as EyePacified).GetPlayerCenter() + Steed.velocity;
                Player.direction = 1;
            }
        }
    }
}
