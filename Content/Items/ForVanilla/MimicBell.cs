using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class MimicBell : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(22);
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 18;
        Item.shoot = ModContent.ProjectileType<MimicBellProj>();
        Item.shootSpeed = 8;
        Item.rare = ItemRarityID.Purple;
        Item.consumable = true;
        Item.maxStack = Item.CommonMaxStack;
    }

    public override void AddRecipes() 
    {
        CreateRecipe(5)
            .AddIngredient(ItemID.GoldBar)
            .AddIngredient(ItemID.CrystalShard)
            .AddTile(TileID.MythrilAnvil)
            .Register();

        CreateRecipe(5)
            .AddIngredient(ItemID.PlatinumBar)
            .AddIngredient(ItemID.CrystalShard)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }

    public class MimicBellProj : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.Size = new(10);
            Projectile.timeLeft = 12000;
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.2f;
            Projectile.rotation += Projectile.velocity.X * 0.2f;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X * 0.7f;
            else
                Projectile.velocity.X = oldVelocity.X * 0.8f;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon && Math.Abs(oldVelocity.Y) > 1f)
                Projectile.velocity.Y = -oldVelocity.Y * 0.7f;

            return false;
        }
    }
}
