using Microsoft.Xna.Framework;
using System;
using System.Runtime.CompilerServices;
using Terraria.ID;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class OffburnTile : ModTile
{
    public class OffburnDust : ModDust
    {
        public static ref bool IsSpawning(Dust dust) => ref Unsafe.Unbox<bool>(dust.customData);

        public override void OnSpawn(Dust dust)
        {
            dust.frame = new Rectangle(0, 4 * Main.rand.Next(3), 2, 2);

            if (Main.rand.NextBool(3))
                dust.frame = new Rectangle(4, 6 * Main.rand.Next(3), 4, 4);

            dust.scale = Main.rand.NextFloat(0.8f, 1.2f);
            dust.customData = true;
            dust.alpha = 250;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.X += Main.windSpeedCurrent;
            dust.velocity.Y += 0.2f;

            ref bool spawning = ref IsSpawning(dust);

            if (!spawning)
                dust.alpha += 12;
            else
            {
                dust.alpha -= 4;

                if (dust.alpha < 0)
                {
                    dust.alpha = 0;
                    spawning = false;
                }
            }

            if (dust.alpha > 255)
                dust.active = false;

            return false;
        }
    }

    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;

        Main.tileBrick[Type] = true;
        Main.tileSolid[Type] = true;
        Main.tileBlockLight[Type] = true;

        AddMapEntry(new Color(65, 54, 99));
        
        DustType = DustID.Lead;
    }

    public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
    {
        if (Main.rand.NextBool(30))
        {
            TrySpawnDust(i, j - 1, new Vector2(0, -1), true);
            TrySpawnDust(i, j + 1, new Vector2(0, 1), true);
            TrySpawnDust(i + 1, j, new Vector2(1, 0), false);
            TrySpawnDust(i - 1, j, new Vector2(-1, 0), false);
        }
    }

    private static void TrySpawnDust(int i, int j, Vector2 direction, bool isUp)
    {
        if (WorldGen.SolidOrSlopedTile(i, j))
            return;

        float range = Main.rand.NextFloat(16);
        Vector2 position = new Vector2(i, j).ToWorldCoordinates(isUp ? range : -direction.X * 8, !isUp ? range : -direction.Y * 8);
        Vector2 velocity = new(RandomizeAxis(direction.X), RandomizeAxis(direction.Y));

        Dust.NewDustPerfect(position, ModContent.DustType<OffburnDust>(), velocity);
    }

    private static float RandomizeAxis(float x) => x == 0 ? Main.rand.NextFloat(-1, 1) : x * Main.rand.NextFloat(2, 6);
}
