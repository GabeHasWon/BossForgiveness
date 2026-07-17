namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

internal class MemoryAnimations
{
    public static void DefaultAnimation(Memory memory, float bossProgression)
    {
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        Rectangle src = new(0, 0, memory.Texture.Width, frameHeight);
        memory.Frame = src;
    }

    public static void EoCAnimation(Memory memory, float bossProgression)
    {
        int frameHeight = memory.Texture.Height / Main.npcFrameCount[memory.NpcType];
        int frame = (int)(memory.LifeTime * 0.2f) % 3;

        if (bossProgression > 0.5f)
            frame += 3;

        frame = 5;

        Rectangle src = new(0, frameHeight * frame, memory.Texture.Width, frameHeight);
        memory.Frame = src;
    }
}
