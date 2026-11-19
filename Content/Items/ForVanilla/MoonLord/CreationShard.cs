using Terraria.ID;

namespace BossForgiveness.Content.Items.ForVanilla.MoonLord;

internal class CreationShard : ModItem
{
    public bool Stackable = true;

    public override ModItem Clone(Item newEntity)
    {
        CreationShard sh = (CreationShard)base.Clone(newEntity);
        sh.Stackable = Stackable;
        return sh;
    }

    public override void SetDefaults()
    {
        Item.Size = new(30);
        Item.value = 0;
        Item.maxStack = Item.CommonMaxStack;
        Item.rare = ItemRarityID.Lime;
    }

    public override bool CanStackInWorld(Item source) => Stackable;

    public override void AddRecipes()
    {
        Recipe.Create(ItemID.FragmentSolar, 2).AddIngredient(Type, 3).Register();
        Recipe.Create(ItemID.FragmentVortex, 2).AddIngredient(Type, 3).Register();
        Recipe.Create(ItemID.FragmentNebula, 2).AddIngredient(Type, 3).Register();
        Recipe.Create(ItemID.FragmentStardust, 2).AddIngredient(Type, 3).Register();
        Recipe.Create(ItemID.LunarBar).AddIngredient(Type, 7).Register();
    }
}
