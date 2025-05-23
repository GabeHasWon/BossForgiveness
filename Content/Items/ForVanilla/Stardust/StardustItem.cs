using BossForgiveness.Content.NPCs.Mechanics.Lunar.Stardust;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal abstract class StardustItem : ModItem
{
    internal abstract int PlaceStyle { get; }

    protected override bool CloneNewInstances => true;

    private float Opacity
    {
        get => 1 - Item.alpha / 255f;
        set => Item.alpha = 255 - (byte)(value * 255f);
    }

    Vector2 originalVel = Vector2.Zero;

    float lifeTime = 0;
    float maxLifeTime = 0;

    public override ModItem Clone(Item newEntity)
    {
        var item = base.Clone(newEntity) as StardustItem;
        item.originalVel = originalVel;
        item.maxLifeTime = maxLifeTime;
        return item;
    }

    public override void SetDefaults()
    {
        Item.rare = ItemRarityID.Cyan;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.consumable = true;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        if (originalVel == Vector2.Zero && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Vector2 copyVel = Item.velocity == Vector2.Zero ? Main.rand.NextVector2CircularEdge(1, 1) : Vector2.Normalize(Item.velocity);
            originalVel = copyVel * Main.rand.NextFloat(1.5f, 2.5f);
        }

        Item.velocity = Vector2.Zero;
        Item.position += originalVel;

        gravity = 0;
        maxFallSpeed = 20;

        if (maxLifeTime == 0)
        {
            maxLifeTime = Main.rand.NextFloat(3, 5) * 60;

            if (Main.netMode != NetmodeID.MultiplayerClient)
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item.whoAmI);
        }

        lifeTime++;

        Opacity = 1f;

        if (lifeTime < maxLifeTime * 0.33f)
            Opacity = lifeTime / (maxLifeTime * 0.33f);
        else if (lifeTime > maxLifeTime * 0.67f)
            Opacity = Math.Max(1 - (lifeTime - maxLifeTime * 0.67f) / (maxLifeTime * 0.33f), 0);

        if (lifeTime > maxLifeTime)
        {
            Item.active = false;

            for (int i = 0; i < 18; ++i)
            {
                Vector2 pos = Item.position + new Vector2(Main.rand.NextFloat(Item.width), Main.rand.NextFloat(Item.height));
                Dust.NewDustPerfect(pos, DustID.Wet, originalVel + Main.rand.NextVector2Circular(6, 6), Scale: Main.rand.NextFloat(1, 3)).noGravity = true;
            }
        }
    }

    public override void HoldItem(Player player) => StardustPillarPacificationNPC.CheckComponents(static (comp, _) => comp.Hover = true);
    public override bool? UseItem(Player player) => StardustPillarPacificationNPC.CheckComponents(CheckPlace);

    private bool CheckPlace(Component comp, NPC npc)
    {
        if (comp.Style == PlaceStyle)
        {
            comp.Placed = true;
            comp.PlacedRotation = CompRotation.Up;

            StardustPillarPlayer.CheckCompletion(npc);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                new SendComponentModule(npc.whoAmI, comp.Position.ToPoint(), true, comp.PlacedRotation).Send();

            return true;
        }

        return false;
    }

    public override void UpdateInventory(Player player) => Opacity = MathHelper.Lerp(Opacity, 1f, 0.05f);

    public override void NetSend(BinaryWriter writer)
    {
        writer.Write((Half)maxLifeTime);
        writer.WriteVector2(originalVel);
    }

    public override void NetReceive(BinaryReader reader)
    {
        maxLifeTime = (float)reader.ReadHalf();
        originalVel = reader.ReadVector2();
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D tex = TextureAssets.Item[Type].Value;
        spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, lightColor * Opacity, rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0);
        return false;
    }
}
