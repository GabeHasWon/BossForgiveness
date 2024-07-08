using BossForgiveness.Content.NPCs.Mechanics.Mech;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

internal class CobaltOrb : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(36);
        Item.damage = 40;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 20;
        Item.shoot = ModContent.ProjectileType<CobaltOrbProjectile>();
        Item.shootSpeed = 12;
        Item.rare = ItemRarityID.Pink;
        Item.noMelee = true;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
    }

    public override void AddRecipes() 
    {
        CreateRecipe(20)
            .AddIngredient(ItemID.CobaltBar, 3)
            .AddIngredient(ItemID.SoulofLight, 2)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

public class CobaltOrbProjectile : ModProjectile
{
    public bool Stop
    {
        get => Projectile.ai[0] == 1;
        set => Projectile.ai[0] = value ? 1 : 0;
    }

    public ref float Timer => ref Projectile.ai[1];

    public override void SetDefaults()
    {
        Projectile.Size = new(36);
        Projectile.timeLeft = 200;
        Projectile.aiStyle = -1;
    }

    public override bool? CanCutTiles() => false;
    public override bool? CanDamage() => !Stop ? false : null;
    public override bool? CanHitNPC(NPC target) => !MechBossPacificationNPC.IsValidEntity(target) ? null : false;

    public override void AI()
    {
        Timer++;

        if (!Stop)
        {
            Projectile.velocity.Y += 0.3f;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.Hitbox.Intersects(Projectile.Hitbox))
                {
                    Projectile.velocity = Vector2.Zero;
                    Stop = true;
                }
            }
        }
        else
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.Hitbox.Intersects(Projectile.Hitbox) && MechBossPacificationNPC.IsValidEntity(npc))
                {
                    float value = Main.rand.NextFloat(-1, 1f);
                    MechBossPacificationNPC.ModifyModifiers(npc, -value, value * 2);
                }
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity = Vector2.Zero;
        return false;
    }
}