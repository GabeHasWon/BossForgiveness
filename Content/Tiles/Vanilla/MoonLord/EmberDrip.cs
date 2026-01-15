namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class EmberDrip : ModDust
{
    public override void OnSpawn(Dust dust)
    {
        dust.frame = Main.rand.Next(3) switch
        {
            0 => new Rectangle(0, 8 * Main.rand.Next(3), 6, 6),
            1 => new Rectangle(8, 10 * Main.rand.Next(3), 8, 8),
            _ => new Rectangle(18, 12 * Main.rand.Next(3), 10, 10)
        };

        if (Main.rand.NextBool(3))
            dust.frame = new Rectangle(4, 6 * Main.rand.Next(3), 4, 4);

        dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
        dust.customData = true;
    }

    public override bool Update(Dust dust)
    {
        dust.velocity.Y += 0.08f;
        dust.position += dust.velocity;
        dust.scale *= 0.95f;

        if (dust.scale < 0.1f)
            dust.active = false;

        return false;
    }
}