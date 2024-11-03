using BossForgiveness.Content.NPCs.Mechanics;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla.Food;

public class WormMorsel : FoodItem
{
    internal override Point Size => new(22, 26);
    internal override int BuffTime => 5 * 60;

    public override void Defaults()
    {
        Item.buffTime = BuffTime;
        Item.buffType = BuffID.Poisoned;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 0)
        {
            Item.shoot = ProjectileID.None;
            Item.UseSound = SoundID.Item2;
        }
        else
        {
            Item.UseSound = SoundID.Item1;
            Item.shoot = ModContent.ProjectileType<WormMorselProj>();
            Item.shootSpeed = 8;
        }

        return true;
    }

    public override void AddRecipes() => CreateRecipe(7).AddIngredient(ItemID.RottenChunk, 6).Register();

    private class WormMorselProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.Size = new(18);
            Projectile.timeLeft = 12000;
            Projectile.penetrate = 1;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.999f;
            Projectile.velocity.Y += 0.2f;
            Projectile.rotation += Projectile.velocity.X * 0.1f;

            if (Main.netMode == NetmodeID.SinglePlayer)
            {
                for (int i = 0; i < Main.maxNPCs; ++i)
                {
                    NPC npc = Main.npc[i];

                    if (npc.active && npc.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        if (npc.type is NPCID.EaterofWorldsTail or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead)
                            WormPacificationNPC.AddFoodToHead(npc);
                        else
                            npc.AddBuff(Main.rand.NextBool(4) ? BuffID.Venom : BuffID.Poisoned, 60 * 60);

                        Projectile.Kill();
                        break;
                    }
                }
            }
            else
            {
                if (Main.myPlayer != Projectile.owner)
                {
                    for (int i = 0; i < Main.maxNPCs; ++i)
                    {
                        NPC npc = Main.npc[i];

                        if (npc.active && npc.Hitbox.Intersects(Projectile.Hitbox))
                        {
                            if (npc.type is NPCID.EaterofWorldsTail or NPCID.EaterofWorldsBody or NPCID.EaterofWorldsHead)
                                new SyncEoWMorselModule(i, Projectile.whoAmI).Send(-1, -1);
                            else
                                npc.AddBuff(Main.rand.NextBool(4) ? BuffID.Venom : BuffID.Poisoned, 60 * 60);

                            Projectile.Kill();

                            break;
                        }
                    }
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; ++i)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.NextBool(4) ? DustID.Bone : DustID.CorruptGibs);

            if (Main.netMode != NetmodeID.Server)
                SoundEngine.PlaySound(SoundID.Item2 with { PitchRange = (-0.4f, 0.4f), Volume = 0.7f });
        }
    }
}
