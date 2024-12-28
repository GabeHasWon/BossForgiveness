using BossForgiveness.Content.NPCs.Mechanics;
using BossForgiveness.Content.NPCs.Mechanics.Mech;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
        Projectile.tileCollide = false;
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

        if (Main.myPlayer == Projectile.owner)
        {
            Vector2 dirToMouse = Owner.DirectionTo(Main.MouseWorld) * 60;
            Projectile.Center = Owner.Center + dirToMouse - new Vector2(16, 0);
            Projectile.rotation = dirToMouse.ToRotation() + MathHelper.PiOver2;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, Projectile.whoAmI);
        }

        int empress = NPC.FindFirstNPC(NPCID.HallowBoss);

        if (empress != -1)
            EmpressBehaviour(empress);
    }

    private void EmpressBehaviour(int empress)
    {
        NPC boss = Main.npc[empress];

        if (boss.ai[0] != 0 && boss.DistanceSQ(Projectile.Center) < 1400 * 1400 && Main.myPlayer == Projectile.owner)
        {
            Timer++;

            for (int i = 0; i < 2; ++i)
            {
                Vector2 vel = boss.DirectionTo(Projectile.Center).RotatedByRandom(MathHelper.PiOver2) * Main.rand.NextFloat(5, 11);
                int proj = ModContent.ProjectileType<PrismLight>();

                Projectile.NewProjectile(Projectile.GetSource_FromAI(), boss.Center + boss.netOffset, vel, proj, 0, 0, Projectile.owner, Projectile.whoAmI, 0, empress);
            }
        }
    }
}

public class PrismLight : ModProjectile
{
    private Projectile Target => Main.projectile[TargetProj];

    private int TargetProj => (int)Projectile.ai[0];
    private ref float Timer => ref Projectile.ai[1];
    private int EmpressWho => (int)Projectile.ai[2];

    public override void SetDefaults()
    {
        Projectile.Size = new Vector2(14);
        Projectile.extraUpdates = 1;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        if (!Target.active || Target.type != ModContent.ProjectileType<PrismLensProj>())
        {
            Projectile.Kill();
            return;
        }

        Timer++;
        Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Target.Center) * 11, MathF.Min(Timer / 120f, 1));
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

        if (Projectile.velocity.HasNaNs())
            Projectile.velocity = Vector2.Zero;

        float dist = Projectile.DistanceSQ(Target.Center);

        if (dist < 12 * 12)
        {
            Projectile.Kill();

            Main.player[Target.owner].velocity += Projectile.velocity.RotatedBy(MathHelper.Pi) * 0.01f;

            if (Main.npc[EmpressWho].TryGetGlobalNPC<EmpressPacificationNPC>(out var empress))
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                    empress.AddLight();
                else if (Main.netMode == NetmodeID.MultiplayerClient)
                    new SyncEmpressLightModule(EmpressWho).Send();
            }
        }
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Color color = Main.hslToRgb((Timer * 0.01f + 0.5f + (Projectile.whoAmI * 0.3f)) % 1f, 1f, 0.5f) * Projectile.Opacity;
        Vector2 position = Projectile.Center - Main.screenPosition;
        var scale = new Vector2(1, Projectile.velocity.Length() / 9f);

        Main.EntitySpriteDraw(tex, position, null, color * 0.1f, Projectile.rotation, tex.Size() / 2f, scale * 4f, SpriteEffects.None);
        Main.EntitySpriteDraw(tex, position, null, color * 0.2f, Projectile.rotation, tex.Size() / 2f, scale * 3.25f, SpriteEffects.None);
        Main.EntitySpriteDraw(tex, position, null, Color.Lerp(color, Color.White, 0.6f) * 0.5f, Projectile.rotation, tex.Size() / 2f, scale * 2f, SpriteEffects.None);
        Main.EntitySpriteDraw(tex, position, null, Color.White * 0.7f, Projectile.rotation, tex.Size() / 2f, scale, SpriteEffects.None);
        return false;
    }
}