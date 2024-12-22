using BossForgiveness.Content.NPCs.Mechanics.Mech;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla;

public class PrismLens : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(46);
        Item.rare = ItemRarityID.Purple;
        Item.noMelee = true;
        Item.noUseGraphic = true;
        Item.maxStack = 1;
        Item.consumable = false;
    }

    public override void HoldItem(Player player)
    {
        if (player.ownedProjectileCounts[ModContent.ProjectileType<PrismLensProj>()] <= 0 && Main.myPlayer == player.whoAmI)
            Projectile.NewProjectile(player.GetSource_FromAI(), player.Center, Vector2.Zero, ModContent.ProjectileType<PrismLensProj>(), 0, 0, player.whoAmI);
    }

    public override void AddRecipes() 
    {
        CreateRecipe(20)
            .AddIngredient(ItemID.EmpressButterfly)
            .AddIngredient(ItemID.SoulofLight, 2)
            .AddIngredient(ItemID.PixieDust, 10)
            .AddIngredient(ItemID.CrystalShard, 10)
            .AddTile(TileID.MythrilAnvil)
            .Register();
    }
}

public class PrismLensProj : ModProjectile
{
    private Player Owner => Main.player[Projectile.owner];

    private ref float Timer => ref Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.Size = new(12);
        Projectile.timeLeft = 2;
        Projectile.aiStyle = -1;
    }

    public override bool? CanCutTiles() => false;

    public override void AI()
    {
        Projectile.timeLeft++;

        if (Owner.HeldItem.type != ModContent.ItemType<PrismLens>())
        {
            Projectile.Kill();
            return;
        }

        Vector2 dirToMouse = Owner.DirectionTo(Main.MouseWorld) * 60;
        Projectile.Center = Owner.Center + dirToMouse;
        Projectile.rotation = dirToMouse.ToRotation() + MathHelper.PiOver2;

        int empress = NPC.FindFirstNPC(NPCID.HallowBoss);

        if (empress != -1)
            EmpressBehaviour(empress);
        else
        { }
    }

    private void EmpressBehaviour(int empress)
    {
        NPC boss = Main.npc[empress];

        if (boss.DistanceSQ(Projectile.Center) < 1400 * 1400)
        {
            Timer++;

            if (Timer % 3 == 0)
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), boss.Center, Vector2.Zero, ModContent.ProjectileType<PrismLight>(), 0, 0, Projectile.owner, Projectile.whoAmI);
        }
    }
}

public class PrismLight : ModProjectile
{
    private Projectile Target => Main.projectile[TargetProj];
    private int TargetProj => (int)Projectile.ai[0];

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(14);
        Projectile.extraUpdates = 1;
    }

    public override void AI()
    {
        Projectile.velocity += Projectile.DirectionTo(Target.Center) * 1.5f;
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        if (Projectile.velocity.LengthSquared() > 7 * 7)
            Projectile.velocity = Vector2.Normalize(Projectile.velocity) * 7;

        if (Projectile.DistanceSQ(Target.Center) < 8 * 8)
            Projectile.Kill();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;

        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Main.DiscoColor, Projectile.rotation, tex.Size() / 2f, 1f, SpriteEffects.None);
        return false;
    }
}