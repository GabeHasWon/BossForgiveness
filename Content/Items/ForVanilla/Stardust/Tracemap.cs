﻿using Microsoft.Xna.Framework;

namespace BossForgiveness.Content.Items.ForVanilla.Stardust;

internal class Tracemap : StardustItem
{
    internal override int PlaceStyle => 0;

    public override void SetDefaults()
    {
        base.SetDefaults();
        Item.Size = new Vector2(36, 30);
        Item.value = 0;
    }
}
