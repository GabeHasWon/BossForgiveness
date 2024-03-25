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

    public override void AddRecipes() => CreateRecipe(40).AddIngredient(ItemID.RottenChunk, 30).Register();

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

            for (int i = 0; i < Main.maxNPCs; ++i)
            {
                NPC npc = Main.npc[i];

                if (npc.active && npc.Hitbox.Intersects(Projectile.Hitbox))
                {
                    if (npc.type == NPCID.EaterofWorldsTail || npc.type == NPCID.EaterofWorldsBody || npc.type == NPCID.EaterofWorldsHead)
                        WormPacificationNPC.AddFoodToHead(npc);

                    SoundEngine.PlaySound(SoundID.Item2 with { PitchRange = (-0.4f, 0.4f), Volume = 0.7f });

                    Projectile.Kill();
                    break;
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; ++i)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, Main.rand.NextBool(4) ? DustID.Bone : DustID.CorruptGibs);
        }
    }

    public class WormPacificationNPC : GlobalNPC
    {
        public const int MaxFood = 40;

        public override bool InstancePerEntity => true;

        internal int foodCount = 0;
        internal int lastWormCount = 0;

        public override bool AppliesToEntity(NPC n, bool lateInstantiation) => n.type == NPCID.EaterofWorldsHead;

        public override bool PreAI(NPC npc)
        {
            if (foodCount > 0)
            {
                if (lastWormCount < foodCount)
                {
                    if (foodCount % 10 == 0)
                    {
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            int n = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, NPCID.DevourerHead, npc.whoAmI);
                            Main.npc[n].velocity = npc.velocity.RotatedByRandom(0.2f);
                        }

                        if (Main.netMode != NetmodeID.Server)
                            SoundEngine.PlaySound(SoundID.NPCDeath13, npc.Center);
                    }

                    lastWormCount = foodCount;
                }
            }
            return true;
        }

        internal static void AddFoodToHead(NPC npc)
        {
            if (npc.type == NPCID.EaterofWorldsHead)
            {
                npc.GetGlobalNPC<WormPacificationNPC>().foodCount += 2;
                return;
            }

            for (int i = npc.whoAmI - 1; i >= 0; --i)
            {
                NPC cur = Main.npc[i];

                if (cur.type == NPCID.EaterofWorldsHead)
                {
                    cur.GetGlobalNPC<WormPacificationNPC>().foodCount++;
                    break;
                }
            }
        }
    }
}
