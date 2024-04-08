using BossForgiveness.Content.NPCs.Mechanics;
using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
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
                        if (npc.type == NPCID.GolemHead || npc.type == NPCID.GolemHeadFree || npc.type == NPCID.GolemFistLeft ||
                            npc.type == NPCID.GolemFistRight || npc.type == NPCID.Golem)
                        {
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                new SyncGolemTaserModule(i, (byte)(npc.GetGlobalNPC<GolemPacificationNPC>().taserCount + 1)).Send(-1, -1, false);
                            else
                                npc.GetGlobalNPC<GolemPacificationNPC>().taserCount++;
                        }

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

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            new SyncNPCTransformModule(i, ModContent.NPCType<GolemHeadPacified>()).Send(-1, -1, false);
                        else
                            npc.Transform(ModContent.NPCType<GolemHeadPacified>());
                    }
                }
            }
        }
    }
}
