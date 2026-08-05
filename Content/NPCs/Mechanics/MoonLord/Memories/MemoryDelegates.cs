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
            memory.Velocity *= 0.9f;
        else
        {
            if (memory.LifeTime % 90 == 0)
                memory.Velocity = memory.Center.DirectionTo(player.Center) * 17;
            else
                memory.Velocity *= 0.95f;

            memory.Rotation = Utils.AngleLerp(memory.Rotation, memory.Velocity.ToRotation() - MathHelper.PiOver2, 0.3f);
        }
    }
}
