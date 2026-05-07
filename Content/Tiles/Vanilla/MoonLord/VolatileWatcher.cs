using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class VolatileWatcher : ModTile
{
    public class VolatileWatcherExplosion : ModProjectile
    {
        public override string Texture => "Terraria/Images/NPC_0";

        private ref float WaitTimer => ref Projectile.ai[0];
        private ref float WaitTimerMax => ref Projectile.ai[1];

        public override void SetDefaults()
        {
            Projectile.Size = new(3 * 16);
            Projectile.timeLeft = 3;
            Projectile.friendly = true;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
        }

        public override bool? CanDamage() => WaitTimer <= 0;

        public override void AI()
        {
            if (WaitTimer > 0)
            {
                WaitTimer--;
                Projectile.timeLeft++;
                return;
            }

            if (Projectile.timeLeft == 2)
            {
                for (int i = 0; i < 15; ++i)
                {
                    int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch);
                    Main.dust[dust].velocity = Main.rand.NextVector2Circular(6, 6) * WaitTimerMax / 6f;
                }

                if (WaitTimerMax == 6)
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = WaitTimerMax / 6f, PitchRange = (-0.6f, 0.6f) }, Projectile.Center);
            }
        }
    }

    public class VolatileWatcherTE : ModTileEntity, IClientSideTE
    {
        internal enum Direction : byte
        {
            None, Left, Right, Up, Down
        }

        internal Direction Dir = Direction.None;

        public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<VolatileWatcher>();

        public static int AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            var tileData = TileObjectData.GetTileData(type, style, alternate);
            int topLeftX = i - tileData.Origin.X;
            int topLeftY = j - tileData.Origin.Y;

            return ModContent.GetInstance<VolatileWatcherTE>().Place(topLeftX, topLeftY);
        }

        public override void Update()
        {
            if (Dir == Direction.None)
                Dir = (Direction)Main.rand.Next(1, 5);

            CheckExplosion();
        }

        private void CheckExplosion()
        {
            const int Thin = 3 * 16;

            Rectangle seeBox = Dir switch
            {
                Direction.Left => new Rectangle((Position.X - ThickDistance) * 16, (Position.Y - 1) * 16, Thick, Thin),
                Direction.Right => new Rectangle((Position.X + 1) * 16, (Position.Y - 1) * 16, Thick, Thin),
                Direction.Up => new Rectangle((Position.X - 1) * 16, (Position.Y - ThickDistance) * 16, Thin, Thick),
                Direction.Down => new Rectangle((Position.X - 1) * 16, (Position.Y + 1) * 16, Thin, Thick),
                _ => throw new Exception("Huh?")
            };

            if (Main.rand.NextBool(12))
                SpawnDust(seeBox);

            foreach (Player player in Main.ActivePlayers)
            {
                if (player.Hitbox.Intersects(seeBox))
                {
                    Explode(seeBox);
                    return;
                }
            }
        }

        private void Explode(Rectangle area)
        {
            for (int i = 0; i < 20; ++i)
                SpawnDust(area);

            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;

            Kill(Position.X, Position.Y);
            WorldGen.KillTile(Position.X, Position.Y, false, false, true);

            if (Main.dedServ)
                NetMessage.SendTileSquare(-1, Position.X, Position.Y, 3);

            for (int i = 0; i < ThickDistance / 3f; ++i)
            {
                Vector2 position = Dir switch
                {
                    Direction.Up => new Vector2(0, 3 * 16 * -i),
                    Direction.Down => new Vector2(16, 3 * 16 * i),
                    Direction.Left => new Vector2(16 - i * 3 * 16, 0),
                    Direction.Right => new Vector2(16 + i * 3 * 16, 0),
                    _ => throw new Exception("???")
                };

                position += Position.ToWorldCoordinates(0, 0);
                int time = i;
                Projectile.NewProjectile(new EntitySource_TileEntity(this), position, Vector2.Zero, ModContent.ProjectileType<VolatileWatcherExplosion>(), 40, 8, Main.myPlayer, time, time);
            }
        }

        private static void SpawnDust(Rectangle area)
        {
            int dust = Dust.NewDust(area.TopLeft(), area.Width, area.Height, DustID.Torch);
            Main.dust[dust].velocity = Vector2.Zero;
        }

        public override void SaveData(TagCompound tag) => tag.Add("dir", (byte)Dir);
        public override void LoadData(TagCompound tag) => Dir = (Direction)tag.GetByte("dir");

        public override void NetSend(BinaryWriter writer) => writer.Write((byte)Dir);
        public override void NetReceive(BinaryReader reader) => Dir = (Direction)reader.ReadByte();
    }

    const int ThickDistance = 25;
    const int Thick = ThickDistance * 16;

    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorAlternateTiles = [ModContent.TileType<EmberTile>(), ModContent.TileType<CooledEmberTile>()];
        TileObjectData.newTile.StyleHorizontal = false;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(VolatileWatcherTE.AfterPlacement, -1, 0, false);

        TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
        TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
        TileObjectData.addAlternate(1);

        TileObjectData.addTile(Type);

        AddMapEntry(new Color(255, 222, 91));

        DustType = DustID.Lead;
    }

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        if (TileEntity.ByPosition[new Point16(i, j)] is VolatileWatcherTE te)
        {
            Texture2D tex = TextureAssets.Tile[Type].Value;
            float rotation = te.Dir switch
            {
                VolatileWatcherTE.Direction.Down => 0,
                VolatileWatcherTE.Direction.Up => MathHelper.Pi,
                VolatileWatcherTE.Direction.Left => MathHelper.PiOver2, 
                VolatileWatcherTE.Direction.Right => -MathHelper.PiOver2, 
                _ => 0
            };

            Vector2 drawPos = TileExtensions.DrawPosition(i, j) + new Vector2(8);

            var lightSrc = new Rectangle(38, 0, 28, 36);
            Vector2 scale = new(1f + MathF.Sin(Main.GameUpdateCount * 0.02f + i + j) * 0.2f, Thick / lightSrc.Height - MathF.Sin(Main.GameUpdateCount * 0.02f + i + j));
            Color color = Lighting.GetColor(i, j) * (MathF.Sin(Main.GameUpdateCount * 0.02f + i + j) * 0.25f + 0.5f);
            spriteBatch.Draw(tex, drawPos, lightSrc, color, rotation - MathHelper.Pi, new Vector2(14, 34), scale, SpriteEffects.None, 0); // Sight
            spriteBatch.Draw(tex, drawPos, new Rectangle(0, 0, 36, 36), Lighting.GetColor(i, j), rotation, new Vector2(18), 1f, SpriteEffects.None, 0);
        }

        return false;
    }
}
