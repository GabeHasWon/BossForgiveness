using BossForgiveness.Content.NPCs;
using BossForgiveness.Content.NPCs.Mechanics;
using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

class CrystalFisher : ModItem
{
    public override void SetStaticDefaults() => Item.ResearchUnlockCount = 1;
    public override bool IsLoadingEnabled(Mod mod) => false;

    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.FiberglassFishingPole);
        Item.fishingPole = 35;
        Item.shootSpeed = 13f;
        Item.shoot = ModContent.ProjectileType<CrystalBobber>();
        Item.value = Item.buyPrice(0, 0, 30, 0);
    }

    public override void AddRecipes() => CreateRecipe().
        AddIngredient(ItemID.CrystalShard, 20).
        AddIngredient(ItemID.Cobweb, 5).
        AddTile(TileID.Anvils).
        Register();

    public class CrystalBobber : ModProjectile
    {
        private NPC Queen => Main.npc[(int)ConnectedQS];
        private Player Owner => Main.player[Projectile.owner];

        private bool Returning => Projectile.ai[0] == 1;
        public ref float ConnectedQS => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.BobberGolden);
            DrawOriginOffsetY = 0;
        }

        public override void OnSpawn(IEntitySource source) => ConnectedQS = -1;

        public override bool PreAI()
        {
            if (ConnectedQS != -1)
            {
                Queen.GetGlobalNPC<QueenSlimePacificationNPC>().crystalHooked = true;

                if (Main.myPlayer == Projectile.owner)
                {
                    Vector2 pos = Queen.GetGlobalNPC<QueenSlimePacificationNPC>().crystalPosition - new Vector2(0, 6);

                    Projectile.Center = pos;
                    Projectile.netUpdate = true;
                }

                if (Returning)
                    Queen.GetGlobalNPC<QueenSlimePacificationNPC>().crystalOffset += Projectile.DirectionTo(Owner.Center) * 0.1f;

                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var crystalBox = new Rectangle((int)Projectile.Center.X - 16, (int)Projectile.Center.Y - 16, 38, 40);
                    Rectangle hitbox = Queen.Hitbox;
                    hitbox.Inflate(14, 14);

                    if (!hitbox.Intersects(crystalBox))
                    {
                        Projectile.ai[0] = 1;
                        Projectile.ai[1] = ModContent.ItemType<CrystalCore>();
                        Projectile.netUpdate = true;
                        Queen.Pacify<QueenSlimePacified>();
                        Queen.netUpdate = true;
                        ConnectedQS = -1;
                    }
                }

                if (Owner.DistanceSQ(Projectile.Center) > 400 * 400)
                    Owner.velocity -= Owner.DirectionFrom(Projectile.Center) * 4;
            }
            else
            {
                foreach (var item in Main.ActiveNPCs)
                {
                    if (item.type == NPCID.QueenSlimeBoss && item.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        if (Main.netMode == NetmodeID.SinglePlayer)
                            ConnectedQS = item.whoAmI;
                        else if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer == Projectile.owner)
                            new SyncFishQSModule(Projectile.identity, item.whoAmI).Send();
                        break;
                    }
                }
            }

            return true;
        }

        public override bool PreDrawExtras()
        {
            int xPositionAdditive = 38;
            float yPositionAdditive = 33f;
            Player player = Main.player[Projectile.owner];

            if (!Projectile.bobber || player.inventory[player.selectedItem].holdStyle <= 0)
                return false;

            Vector2 lineOrigin = player.MountedCenter;
            lineOrigin.Y += player.gfxOffY;
            float gravity = player.gravDir;

            lineOrigin.X += xPositionAdditive * player.direction;
            if (player.direction < 0)
                lineOrigin.X -= 13f;
            lineOrigin.Y -= yPositionAdditive * gravity;

            if (gravity == -1f)
                lineOrigin.Y -= 12f;

            lineOrigin = player.RotatedRelativePoint(lineOrigin + new Vector2(8f), true) - new Vector2(8f);
            Vector2 playerToProjectile = Projectile.Center - lineOrigin;
            bool canDraw = true;

            if (playerToProjectile.X == 0f && playerToProjectile.Y == 0f)
                return false;

            float playerToProjectileMagnitude = playerToProjectile.Length();
            playerToProjectileMagnitude = 12f / playerToProjectileMagnitude;
            playerToProjectile *= playerToProjectileMagnitude;
            lineOrigin -= playerToProjectile;
            playerToProjectile = Projectile.Center - lineOrigin;

            while (canDraw)
            {
                float height = 12f;
                float positionMagnitude = playerToProjectile.Length();

                if (float.IsNaN(positionMagnitude))
                    break;

                if (positionMagnitude < 20f)
                {
                    height = positionMagnitude - 8f;
                    canDraw = false;
                }

                playerToProjectile *= 12f / positionMagnitude;
                lineOrigin += playerToProjectile;
                playerToProjectile.X = Projectile.position.X + Projectile.width * 0.5f - lineOrigin.X;
                playerToProjectile.Y = Projectile.position.Y + Projectile.height * 0.1f - lineOrigin.Y;

                if (positionMagnitude > 12f)
                {
                    float positionInverseMultiplier = 0.3f;
                    float absVelocitySum = Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y);

                    if (absVelocitySum > 16f)
                        absVelocitySum = 16f;

                    absVelocitySum = 1f - absVelocitySum / 16f;
                    positionInverseMultiplier *= absVelocitySum;
                    absVelocitySum = positionMagnitude / 80f;

                    if (absVelocitySum > 1f)
                        absVelocitySum = 1f;

                    positionInverseMultiplier *= absVelocitySum;

                    if (positionInverseMultiplier < 0f)
                        positionInverseMultiplier = 0f;

                    absVelocitySum = 1f - Projectile.localAI[0] / 100f;
                    positionInverseMultiplier *= absVelocitySum;

                    if (playerToProjectile.Y > 0f)
                    {
                        playerToProjectile.Y *= 1f + positionInverseMultiplier;
                        playerToProjectile.X *= 1f - positionInverseMultiplier;
                    }
                    else
                    {
                        absVelocitySum = Math.Abs(Projectile.velocity.X) / 3f;
                        if (absVelocitySum > 1f)
                            absVelocitySum = 1f;
                        absVelocitySum -= 0.5f;
                        positionInverseMultiplier *= absVelocitySum;
                        if (positionInverseMultiplier > 0f)
                            positionInverseMultiplier *= 2f;
                        playerToProjectile.Y *= 1f + positionInverseMultiplier;
                        playerToProjectile.X *= 1f - positionInverseMultiplier;
                    }
                }

                Color lineColor = Lighting.GetColor((int)lineOrigin.X / 16, (int)(lineOrigin.Y / 16f), Color.White);
                float rotation = playerToProjectile.ToRotation() - MathHelper.PiOver2;
                Main.spriteBatch.Draw(TextureAssets.FishingLine.Value, new Vector2(lineOrigin.X - Main.screenPosition.X + TextureAssets.FishingLine.Value.Width * 0.5f, lineOrigin.Y - Main.screenPosition.Y + TextureAssets.FishingLine.Value.Height * 0.5f), new Rectangle(0, 0, TextureAssets.FishingLine.Value.Width, (int)height), lineColor, rotation, new Vector2(TextureAssets.FishingLine.Value.Width * 0.5f, 0f), 1f, SpriteEffects.None, 0f);
            }
            
            return false;
        }
    }
}
