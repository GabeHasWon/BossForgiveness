using System;
using Terraria;

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

            int time = memory.LifeTime % 600;

            if (time == 5)
                memory.AddedInfo = (memory.Center, memory.Center + player.DirectionTo(memory.Center) * 400);

            if (time > 5 && time < 50)
            {

            }
        }

        if (memory.DespawnTime > 600)
            memory.Collected = true;
    }
}
