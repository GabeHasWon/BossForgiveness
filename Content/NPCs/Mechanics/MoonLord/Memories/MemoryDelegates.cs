using BossForgiveness.Content.NPCs.Mechanics.MoonLord.Attacks;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

internal class MemoryDelegates
{
    public static void DefaultAnimation(Memory memory, float bossProgression)
    {
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        Rectangle src = new(0, 0, memory.Texture.Width, frameHeight);
        memory.Frame = src;
    }

    public static void EoCUpdate(Memory memory, float bossProgression)
    {
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        int frame = (int)(memory.LifeTime * 0.2f) % 3;

        if (bossProgression > 0.5f)
            frame += 3;

        Rectangle src = new(0, frameHeight * frame, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];

        if (!player.active || player.dead)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            if (player.DistanceSQ(memory.Center) > 1500 * 1500)
                memory.DespawnTime++;
            else
                memory.DespawnTime = Math.Max(memory.DespawnTime - 1, 0);

            if (memory.LifeTime % 96 == 0)
                memory.Velocity = memory.Center.DirectionFrom(player.Center) * 22;
            else
                memory.Velocity *= 0.97f;

            memory.Rotation = Utils.AngleLerp(memory.Rotation, memory.Velocity.ToRotation() - MathHelper.PiOver2, 0.3f);
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }

    public static void KingSlimeUpdate(Memory memory, float bossProgression)
    {
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        int frame = (int)(memory.LifeTime * 0.2f) % 3;
        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];

        Rectangle src = new(0, frameHeight * frame, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        if (!player.active || player.dead)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            if (player.DistanceSQ(memory.Center) > 1500 * 1500)
                memory.DespawnTime++;
            else
                memory.DespawnTime = Math.Max(memory.DespawnTime - 1, 0);

            if (Collision.SolidCollision(memory.Position + new Vector2(0, memory.Size.Y), (int)memory.Size.X, 20) && memory.LifeTime % 120 > 5)
            {
                memory.Velocity.X *= 0.8f;
                memory.Velocity.Y = 0;
            }
            else
                memory.Velocity.Y += 0.6f;

            if (memory.LifeTime % 120 == 119)
            {
                memory.Velocity.Y = -12 - bossProgression * 2;
                memory.Velocity.X = -MathF.Sign(player.Center.X - memory.Center.X) * 7;
            }
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }

    public static void SkeletronUpdate(Memory memory, float bossProgression)
    {
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];

        Rectangle src = new(0, 0, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        if (!player.active || player.dead)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            if (player.DistanceSQ(memory.Center) > 1500 * 1500)
                memory.DespawnTime++;
            else
                memory.DespawnTime = Math.Max(memory.DespawnTime - 1, 0);

            if (memory.LifeTime % 600 < 480)
            {
                memory.Rotation += 0.25f;
                memory.Velocity = memory.Center.DirectionFrom(player.Center) * (memory.Center.Distance(player.Center) / 200f);
            }
            else
            {
                memory.Velocity *= 0.9f;
                memory.Rotation = Utils.AngleLerp(memory.Rotation, 0, 0.05f);
            }
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }

    public static void QueenBeeUpdate(Memory memory, float bossProgression)
    {
        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        int frame = (int)(memory.LifeTime / 10f) % 8 + 4;
        bool returning = memory.LifeTime % 400 > 200;

        if (!returning)
            frame = (int)(memory.LifeTime / 10f) % 4;

        Rectangle src = new(0, frameHeight * frame, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        if (!player.active || player.dead)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            if (player.DistanceSQ(memory.Center) > 1500 * 1500)
                memory.DespawnTime++;
            else
                memory.DespawnTime = Math.Max(memory.DespawnTime - 1, 0);

            if (returning)
                memory.Velocity = Vector2.Lerp(memory.Velocity, memory.Center.DirectionTo(player.Center) * 3, 0.1f);
            else
            {
                memory.Velocity.Y *= 0.8f;
                memory.Velocity.X = Math.Sign(memory.Center.X - player.Center.X) * 10;
            }

            memory.SpriteEffect = Math.Sign(memory.Velocity.X) == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }

    public static void WallOfFleshUpdate(Memory memory, float bossProgression)
    {
        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        int frame = (int)(memory.LifeTime / 10f) % 2;

        Rectangle src = new(0, frameHeight * frame, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        if (memory.LifeTime == 1 && memory.NpcType != NPCID.WallofFleshEye)
        {
            Memory eye = BossMemories.BossMemoryTemplates[NPCID.WallofFleshEye].Clone();
            eye.Position = memory.Position + new Vector2(0, 400);
            memory.AddChild(eye);
            
            eye = BossMemories.BossMemoryTemplates[NPCID.WallofFleshEye].Clone();
            eye.Position = memory.Position - new Vector2(0, 400);
            memory.AddChild(eye);
        }

        if (memory.Center.X < 0 || memory.Center.X > Main.maxTilesX * 16)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            memory.Velocity.X = -2;
            memory.SpriteEffect = Math.Sign(memory.Velocity.X) == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }

    public static void SkeletronPrimeUpdate(Memory memory, float bossProgression)
    {
        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        int frame = (int)(memory.LifeTime / 10f) % 2;
        bool spinning = memory.LifeTime % 800 > 500;

        if (spinning)
            frame = 2;

        Rectangle src = new(0, frameHeight * frame, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        if (!player.active || player.dead)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            if (player.DistanceSQ(memory.Center) > 1500 * 1500)
                memory.DespawnTime++;
            else
                memory.DespawnTime = Math.Max(memory.DespawnTime - 1, 0);

            if (memory.LifeTime % 500 < 200)
                memory.Velocity *= 0.94f;
            else if (!spinning)
            {
                memory.Velocity = Vector2.Lerp(memory.Velocity, memory.Center.DirectionFrom(player.Center) * 12, 0.05f);
                memory.Rotation = Utils.AngleLerp(memory.Rotation, 0, 0.1f);
            }
            else
            {
                memory.Velocity = Vector2.Lerp(memory.Velocity, memory.Center.DirectionTo(player.Center).RotatedBy(MathHelper.PiOver2) * 12, 0.1f);
                memory.Rotation -= 0.2f;
            }

            memory.SpriteEffect = Math.Sign(memory.Velocity.X) == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }

    public static void PlanteraUpdate(Memory memory, float bossProgression)
    {
        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        int frame = (int)(memory.LifeTime / 10f) % 4;

        if (bossProgression > 0.5f)
            frame += 4;

        Rectangle src = new(0, frameHeight * frame, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        if (!player.active || player.dead)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            if (player.DistanceSQ(memory.Center) > 1500 * 1500)
                memory.DespawnTime++;
            else
                memory.DespawnTime = Math.Max(memory.DespawnTime - 1, 0);

            memory.Velocity = Vector2.Lerp(memory.Velocity, memory.Center.DirectionFrom(player.Center) * (MathF.Sin(memory.LifeTime * 0.02f) * 3 + 8), 0.1f);
            memory.Rotation = memory.Velocity.ToRotation() + MathHelper.PiOver2;
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }

    public static void CultistUpdate(Memory memory, float bossProgression)
    {
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        Player player = Main.player[Player.FindClosest(memory.Position, memory.Colors.Width, memory.Colors.Height)];
        Rectangle src = new(0, 0, memory.Texture.Width, frameHeight);
        memory.Frame = src;

        if (!player.active || player.dead)
        {
            memory.Velocity *= 0.9f;
            memory.DespawnTime++;
        }
        else
        {
            if (player.DistanceSQ(memory.Center) > 1500 * 1500)
                memory.DespawnTime++;
            else
                memory.DespawnTime = Math.Max(memory.DespawnTime - 1, 0);

            int time = memory.LifeTime % 800;
            int moveTime = memory.LifeTime % 300;

            if (moveTime == 5)
            {
                Vector2 nextPos = memory.Position + player.DirectionTo(memory.Position) * 700;
                memory.AddedInfo = new Tuple<Vector2, Vector2>(memory.Position, nextPos);
                memory.SpriteEffect = MathF.Sign(nextPos.X - memory.Position.X) == 1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            }

            if (moveTime > 5 && moveTime < 15)
            {
                var posInfo = (Tuple<Vector2, Vector2>)memory.AddedInfo;
                memory.Position = Vector2.Lerp(posInfo.Item1, posInfo.Item2, Utils.GetLerpValue(5, 15, moveTime));
            }

            if (time > 100 && time < 150)
            {
                Vector2 newPos = memory.Center + new Vector2(Main.rand.NextFloat(500), 0).RotatedByRandom(MathHelper.Pi);
                Dust newDust = VoidPortal.SpawnDust(newPos, 1, 1);
                newDust.velocity = newPos.DirectionTo(memory.Center) * 9;
            }
            
            if (time == 175 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 velocity = memory.Center.DirectionTo(player.Center) * 20;
                Projectile.NewProjectile(new EntitySource_Memory(memory), memory.Center, velocity, ModContent.ProjectileType<VoidPortal>(), 0, 0);
            }
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }
}
