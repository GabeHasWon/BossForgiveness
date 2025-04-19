using Microsoft.Build.Tasks.Deployment.ManifestUtilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal abstract class StardustItem : ModItem
{
    protected override bool CloneNewInstances => true;

    private float Opacity
    {
        get => 1 - Item.alpha / 255f;
        set => Item.alpha = 255 - (byte)(value * 255f);
    }

    float lifeTime = 0;
    float maxLifeTime = 0;
    Vector2 originalVel = Vector2.Zero;

    public override ModItem Clone(Item newEntity)
    {
        var item = base.Clone(newEntity) as StardustItem;
        item.originalVel = originalVel;
        item.maxLifeTime = maxLifeTime;
        return item;
    }

    public override void Update(ref float gravity, ref float maxFallSpeed)
    {
        if (originalVel == Vector2.Zero)
        {
            originalVel = Vector2.Normalize(Item.velocity) * Main.rand.NextFloat(1.5f, 2.5f);
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
            Item.active = false;
    }

    public override void NetSend(BinaryWriter writer) => writer.Write((Half)maxLifeTime);
    public override void NetReceive(BinaryReader reader) => maxLifeTime = (float)reader.ReadHalf();

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D tex = TextureAssets.Item[Type].Value;
        spriteBatch.Draw(tex, Item.Center - Main.screenPosition, null, lightColor * Opacity, rotation, tex.Size() / 2f, scale, SpriteEffects.None, 0);
        return false;
    }
}
