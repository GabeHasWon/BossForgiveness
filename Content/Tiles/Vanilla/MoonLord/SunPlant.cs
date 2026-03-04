using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.GameContent.Metadata;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace BossForgiveness.Content.Tiles.Vanilla.MoonLord;

internal class SunPlant : ModTile
{
    public class SunPlantTE : ModTileEntity, IClientSideTE
    {
        public List<(SunPlantSegment segmentType, float length)> Segments = [];

        public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].HasTile && Main.tile[x, y].TileType == ModContent.TileType<SunPlant>();

        public static int AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            var tileData = TileObjectData.GetTileData(type, style, alternate);
            int topLeftX = i - tileData.Origin.X;
            int topLeftY = j - tileData.Origin.Y;

            return ModContent.GetInstance<SunPlantTE>().Place(topLeftX, topLeftY);
        }

        public override void Update()
        {
            if (Segments.Count == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int segments = Main.rand.Next(3, 8);

                for (int i = 0; i < segments; ++i)
                {
                    SunPlantSegment seg = SunPlantSegment.Small;
                    float length = 2 + Step(0, 2);

                    if (i == segments - 1)
                    {
                        seg = SunPlantSegment.Sun;
                        length = 2 + Step(8, 16);
                    }
                    else if (i >= segments / 2)
                    {
                        seg = SunPlantSegment.Large;
                        length = 2 + Step(1, 3);
                    }

                    Segments.Add((seg, length));
                }
            }

            SunHeatArea();

            static float Step(int minSteps, int maxSteps) => Main.rand.Next(minSteps, maxSteps) * 0.45f;
        }

        private void SunHeatArea()
        {
            int i = Position.X;
            int j = Position.Y;
            float offset = 0;
            float sine = GetSine(i, j);
            Vector2 lastPosition = TileExtensions.DrawPosition(i, j) + new Vector2(4, 0);

            foreach ((SunPlantSegment segment, float length) in Segments)
            {
                offset += length;

                Rectangle src = GetSegmentSource(offset, segment);
                Vector2 position = TileExtensions.DrawPosition(i, (int)(j - offset)) + new Vector2(sine, 0);

                Vector2 chainDirection = position.DirectionTo(lastPosition + new Vector2(4, 0));
                int chainCount = segment == SunPlantSegment.Sun ? 4 : 2;

                if (segment == SunPlantSegment.Sun)
                {
                    Vector2 worldPos = position + Main.screenPosition - TileExtensions.TileDrawOffset;

                    foreach (Player player in Main.ActivePlayers)
                    {
                        if (player.DistanceSQ(worldPos) < 90 * 90)
                        {
                            player.AddBuff(ModContent.BuffType<SunburnDebuff>(), 2);
                        }
                    }
                    
                    break;
                }

                lastPosition = position;
                sine += GetSine(i, (int)(j - offset));
            }
        }

        public override void SaveData(TagCompound tag)
        {
            tag.Add("segments", (byte[])[.. Segments.Select(x => (byte)x.segmentType)]);
            tag.Add("lengths", (float[])[.. Segments.Select(x => x.length)]);
        }

        public override void LoadData(TagCompound tag)
        {
            Segments.Clear();

            byte[] segments = tag.GetByteArray("segments");
            float[] lengths = tag.Get<float[]>("lengths");

            for (int i = 0; i < segments.Length; ++i)
            {
                Segments.Add(((SunPlantSegment)segments[i], lengths[i]));
            }
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write((byte)Segments.Count);
            
            foreach ((SunPlantSegment type, float length) in Segments)
            {
                writer.Write((byte)type);
                writer.Write((Half)length);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            Segments.Clear();
            int count = reader.ReadByte();

            for (int i = 0; i < count; ++i)
            {
                var segment = (SunPlantSegment)reader.ReadByte();
                float length = (float)reader.ReadHalf();

                Segments.Add((segment, length));
            }
        }
    }

    public class SunburnDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;

            BuffID.Sets.NurseCannotRemoveDebuff[Type] = true;
        }
    }

    public class SunburnPlayer : ModPlayer 
    {
        public override void UpdateBadLifeRegen()
        {
            if (!Player.HasBuff<SunburnDebuff>())
                return;

            Player.lifeRegen = Math.Min(0, Player.lifeRegen);
            Player.lifeRegen -= 120;
        }
    }

    public enum SunPlantSegment : byte
    {
        Small,
        Large,
        Sun
    }

    public override void SetStaticDefaults()
    {
        TileID.Sets.CanBeClearedDuringGeneration[Type] = false;
        TileID.Sets.CanBeClearedDuringOreRunner[Type] = false;
        TileID.Sets.SwaysInWindBasic[Type] = true;

        Main.tileFrameImportant[Type] = true;

        TileMaterials.SetForTileId(Type, TileMaterials._materialsByName["Plant"]);

        TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
        TileObjectData.newTile.CoordinateHeights = [16];
        TileObjectData.newTile.CoordinateWidth = 16;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom | AnchorType.AlternateTile, TileObjectData.newTile.Width, 0);
        TileObjectData.newTile.AnchorAlternateTiles = [ModContent.TileType<EmberTile>(), ModContent.TileType<CooledEmberTile>()];
        TileObjectData.newTile.StyleHorizontal = false;
        TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(SunPlantTE.AfterPlacement, -1, 0, false);
        TileObjectData.addTile(Type);

        AddMapEntry(new Color(255, 222, 91));

        DustType = DustID.Lead;
    }

    public override void SetSpriteEffects(int i, int j, ref SpriteEffects spriteEffects) => spriteEffects = i % 2 == 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
    public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem) => ModContent.GetInstance<SunPlantTE>().Kill(i, j);

    public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
    {
        Texture2D tex = TextureAssets.Tile[Type].Value;
        spriteBatch.Draw(tex, TileExtensions.DrawPosition(i, j), new Rectangle(0, 0, 16, 16), Color.White);

        if (TileEntity.ByPosition[new Point16(i, j)] is SunPlantTE plant)
        {
            float offset = 0;
            float sine = GetSine(i, j);
            Vector2 lastPosition = TileExtensions.DrawPosition(i, j) + new Vector2(4, 0);

            foreach ((SunPlantSegment segment, float length) in plant.Segments)
            {
                offset += length;

                Rectangle src = GetSegmentSource(offset, segment);
                Vector2 position = TileExtensions.DrawPosition(i, (int)(j - offset)) + new Vector2(sine, 0);

                Vector2 chainDirection = position.DirectionTo(lastPosition + new Vector2(4, 0));
                int chainCount = segment == SunPlantSegment.Sun ? 4 : 2;

                for (int k = 0; k < chainCount; ++k)
                {
                    Rectangle chainSrc = new(86, 12 * ((k + (int)offset) % 3), 6, 10);
                    spriteBatch.Draw(tex, position + chainDirection * (13 + k * 10), chainSrc, Color.White * (1 - (k + 1) / (float)chainCount), 0f, chainSrc.Size() / 2f, 1f, SpriteEffects.None, 0);
                }

                if (segment == SunPlantSegment.Sun)
                    DrawSunExtras(i, j, spriteBatch, tex, position, src);

                spriteBatch.Draw(tex, position.Floor(), src, Color.White, 0f, src.Size() / 2f, 1f, SpriteEffects.None, 0);

                lastPosition = position;
                sine += GetSine(i, (int)(j - offset));
            }
        }

        return false;


        static void DrawSunExtras(int i, int j, SpriteBatch spriteBatch, Texture2D tex, Vector2 position, Rectangle src)
        {
            float sine = GetSine(i, j) / 5f * 0.1f + 1f;
            spriteBatch.Draw(tex, position.Floor(), src, Color.White * 0.035f * sine, 0f, src.Size() / 2f, 6f * sine, SpriteEffects.None, 0);

            sine = GetSine(i, j + 2) / 5f * 0.1f + 1f;
            spriteBatch.Draw(tex, position.Floor(), src, Color.White * 0.05f * sine, 0f, src.Size() / 2f, 5f * sine, SpriteEffects.None, 0);

            sine = GetSine(i, j + 4) / 5f * 0.1f + 1f;
            spriteBatch.Draw(tex, position.Floor(), src, Color.White * 0.07f * sine, 0f, src.Size() / 2f, 3f * sine, SpriteEffects.None, 0);

            Rectangle raySource = new(138, 2, 26, 136);

            for (int k = 0; k < 5; ++k)
            {
                float rotation = i + k * MathHelper.TwoPi / 5f + Main.GameUpdateCount * k switch
                {
                    0 or 4 => 0.005f,
                    1 or 3 => 0.0052f,
                    _ => 0.0056f,
                };

                var scale = new Vector2(1, GetSine(i, j, k * 0.33f) / 20f + 0.5f);
                Color color = Color.Yellow * (GetSine(i, j, k * 0.33f + MathHelper.Pi) / 20f + 0.5f);
                spriteBatch.Draw(tex, position.Floor(), raySource, color, rotation, raySource.Size() / new Vector2(2f, 1), scale, SpriteEffects.None, 0);
            }
        }
    }

    static float GetSine(int i, int j, float offset = 0) => MathF.Sin(i * MathHelper.PiOver2 + j * 0.67f + Main.GameUpdateCount * 0.05f + offset) * 5;

    private static Rectangle GetSegmentSource(float offset, SunPlantSegment segment) => segment switch
    {
        SunPlantSegment.Small => new Rectangle(0, 18 * (int)(offset % 3), 16, 16),
        SunPlantSegment.Large => new Rectangle(18, 28 * (int)(offset % 3), 26, 26),
        _ => new Rectangle(46, 38 * (int)(offset % 3), 36, 36)
    };
}
