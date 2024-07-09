using BossForgiveness.Content.NPCs.Mechanics.Mech;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

public class CobaltOrb : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(36);
        Item.damage = 25;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.useTime = Item.useAnimation = 45;
        Item.shoot = ModContent.ProjectileType<CobaltOrbProjectile>();
        Item.shootSpeed = 12;
        Item.rare = ItemRarityID.Pink;
        Item.noMelee = true;
        Item.noUseGraphic = true;
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
    private static Asset<Texture2D> _emp = null;

    public bool Stop
    {
        get => Projectile.ai[0] == 1;
        set => Projectile.ai[0] = value ? 1 : 0;
    }

    public ref float EMPTimer => ref Projectile.ai[1];

    public override void SetStaticDefaults() => _emp ??= ModContent.Request<Texture2D>(Texture.Replace("CobaltOrbProjectile", "OrbEMP"));

    public override void SetDefaults()
    {
        Projectile.Size = new(24);
        Projectile.timeLeft = 200;
        Projectile.aiStyle = -1;
    }

    public override bool? CanCutTiles() => false;
    public override bool? CanDamage() => !Stop ? false : null;
    public override bool? CanHitNPC(NPC target) => !MechBossPacificationNPC.IsValidEntity(target) ? null : false;

    public override void AI()
    {
        Projectile.rotation += Projectile.velocity.X * 0.02f;
        Projectile.timeLeft++;

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
            float fade = EMPTimer / 40f;
            Lighting.AddLight(Projectile.Center, Color.LightPink.ToVector3() * fade * 0.65f);

            EMPTimer++;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.DistanceSQ(Projectile.Center) < MathF.Pow(280f * MathF.Pow(EMPTimer / 40f, 4), 2))
                {
                    if (MechBossPacificationNPC.IsValidEntity(npc))
                    {
                        float value = Main.rand.NextFloat(-1, 1f);
                        MechBossPacificationNPC.ModifyModifiers(npc, -value, value * 2);
                    }
                    else
                    {
                        var hitInfo = npc.CalculateHitInfo(Projectile.damage, Projectile.Center.X < npc.Center.X ? -1 : 1, damageVariation: true);
                        npc.StrikeNPC(hitInfo);
                    }
                }
            }

            if (Main.rand.NextBool(2))
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Electric);
            }

            Projectile.Opacity = 1 - fade;

            if (EMPTimer > 40f)
                Projectile.Kill();
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity = Vector2.Zero;
        Stop = true;
        return false;
    }

    public override void PostDraw(Color lightColor)
    {
        if (!Stop)
            return;

        Vector2 pos = Projectile.Center - Main.screenPosition;

        for (int i = 0; i < 5; ++i)
        {
            float rot = Main.rand.NextFloat(MathHelper.Pi);
            float scale = 2f * MathF.Pow(EMPTimer / 40f, 4);
            Main.spriteBatch.Draw(_emp.Value, pos, null, Color.White * (i / 4f) * (1 - scale / 2f), rot, _emp.Size() / 2f, scale * 2f, SpriteEffects.None, 0);
        }
    }
}

public class PalladiumOrb : CobaltOrb
{
    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.shoot = ModContent.ProjectileType<PalladiumOrbProjectile>();
    }

    public override void AddRecipes()
    {
        CreateRecipe(20)
            .AddIngredient(ItemID.PalladiumBar, 3)
            .AddIngredient(ItemID.SoulofLight, 2)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

public class PalladiumOrbProjectile : CobaltOrbProjectile
{
}