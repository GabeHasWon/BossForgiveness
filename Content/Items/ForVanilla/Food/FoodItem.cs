using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Items.ForVanilla.Food;

public abstract class FoodItem : ModItem
{
	internal abstract Point Size { get; }
	internal virtual int Rarity => ItemRarityID.Blue;
	internal virtual bool Consumeable => true;
    internal virtual int BuffTime => 5 * 60 * 60;

	public sealed override void SetStaticDefaults()
	{
        ItemID.Sets.IsFood[Type] = true;

        StaticDefaults();
	}

	public override sealed void SetDefaults()
	{
		Item.width = Size.X;
		Item.height = Size.Y;
		Item.rare = Rarity;
		Item.maxStack = 99;
		Item.value = Item.sellPrice(0, 0, 0, 50);
		Item.noUseGraphic = false;
		Item.useStyle = ItemUseStyleID.EatFood;
		Item.useTime = Item.useAnimation = 20;
		Item.noMelee = true;
		Item.consumable = Consumeable;
		Item.autoReuse = false;
		Item.UseSound = SoundID.Item2;
		Item.buffTime = BuffTime;
		Item.buffType = BuffID.WellFed;

		Defaults();
	}

	public virtual void StaticDefaults() { }
	public virtual void Defaults() { }

    public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        Vector2 offset = new(-2);
        spriteBatch.Draw(tex, position.ToPoint().ToVector2() + offset, new Rectangle(0, 0, Item.width, Item.height), drawColor, 0f, Item.Size / 3f, scale * 3, SpriteEffects.None, 0f);
        return false;
    }

    public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(tex, Item.Center - Main.screenPosition, new Rectangle(0, 0, Item.width, Item.height), lightColor, rotation, Item.Size / 2f, scale, SpriteEffects.None, 0f);
        return false;
    }
}
