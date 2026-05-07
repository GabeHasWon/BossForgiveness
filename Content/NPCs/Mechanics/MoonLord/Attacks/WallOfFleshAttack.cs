using BossForgiveness.Content.Tiles.Vanilla.MoonLord;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Attacks;

internal class WallOfFleshAttack : MoonLordAttacksNPC.CustomAttack
{
    public class DelayedTileProj : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_0";

        private ref float FrameX => ref Projectile.ai[0];
        private ref float FrameY => ref Projectile.ai[1];
        private ref float Timer => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.timeLeft = 2;
            Projectile.Size = new Vector2(16);
            Projectile.aiStyle = -1;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.timeLeft++;

            Timer--;

            if (Timer == 0)
            {
                Tile tile = Main.tile[Projectile.Center.ToTileCoordinates16()];

                int oldFrameX = tile.TileType;
                int oldFrameY = tile.HasTile ? 1 : 0;

                tile.HasTile = true;
                tile.TileType = (ushort)ModContent.TileType<EldritchFleshTile>();
                tile.TileFrameX = (short)FrameX;
                tile.TileFrameY = (short)FrameY;

                FrameX = oldFrameX;
                FrameY = oldFrameY;
            }
            else if (Timer <= -6 * 60)
            {
                Point16 pos = Projectile.Center.ToTileCoordinates16();
                Tile tile = Main.tile[pos];

                if (FrameY == 0)
                    tile.HasTile = false;
                else
                {
                    tile.HasTile = true;
                    tile.TileType = (ushort)FrameX;
                }

                WorldGen.TileFrame(pos.X, pos.Y, true);
                Projectile.Kill();
            }
        }
    }

    public override int[] PacifiedRequirements => [NPCID.WallofFlesh];

    private readonly int[] _kitingTimes = new int[Main.maxPlayers];
    private readonly float?[] _trackingAngles = new float?[Main.maxPlayers];

    public override void Update(NPC npc)
    {
        foreach (Player player in Main.ActivePlayers)
        {
            ref int timer = ref _kitingTimes[player.whoAmI];
            ref float? angle = ref _trackingAngles[player.whoAmI];

            angle ??= player.velocity.ToRotation();

            if (MathF.Abs(player.velocity.ToRotation() - angle.Value) < MathHelper.PiOver4 && player.velocity.LengthSquared() > 2f)
            {
                timer++;

                if (timer > 60 * 2.5f)
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Point16 pos = (player.Center + player.velocity.SafeNormalize(Vector2.Zero) * 900).ToTileCoordinates16();
                        Dictionary<Point16, (bool, ushort, short, short)> positions = [];

                        const int Range = 16;

                        float velLength = player.velocity.Length();

                        for (int i = pos.X - Range; i < pos.X + Range; ++i)
                        {
                            for (int j = pos.Y - Range; j < pos.Y + Range; j++)
                            {
                                var absolute = new Vector2(i - pos.X, j - pos.Y);
                                float dotRange = absolute.Length() * velLength * 0.8f;
                                Tile tile = Main.tile[i, j];
                                float fuzzyDistance = Range * Range * Main.rand.NextFloat(0.75f, 1);
                                
                                if (Vector2.Dot(absolute, player.velocity) > dotRange && absolute.LengthSquared() < fuzzyDistance && Main.tileSolid[tile.TileType])
                                    positions.Add(new Point16(i, j), (tile.HasTile, tile.TileType, tile.TileFrameX, tile.TileFrameY));
                            }
                        }

                        foreach (Point16 position in positions.Keys)
                        {
                            Tile tile = Main.tile[position];
                            tile.HasTile = true;
                            tile.TileType = (ushort)ModContent.TileType<EldritchFleshTile>();
                        }

                        foreach (Point16 position in positions.Keys)
                            WorldGen.TileFrame(position.X, position.Y);

                        foreach (KeyValuePair<Point16, (bool, ushort, short, short)> position in positions)
                        {
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                Tile tile = Main.tile[position.Key];

                                Vector2 projPos = position.Key.ToWorldCoordinates(0, 0);
                                int type = ModContent.ProjectileType<DelayedTileProj>();
                                Projectile.NewProjectile(npc.GetSource_FromThis(), projPos, Vector2.Zero, type, 0, 0, Main.myPlayer, tile.TileFrameX, tile.TileFrameY, Main.rand.Next(10, 60));

                                tile.HasTile = position.Value.Item1;
                                tile.TileType = position.Value.Item2;
                                tile.TileFrameX = position.Value.Item3;
                                tile.TileFrameY = position.Value.Item4;
                            }
                        }
                    }

                    timer = 0;
                    angle = null;
                }
            }
            else
            {
                angle = player.velocity.ToRotation();
                timer = 0;
            }
        }
    }
}
