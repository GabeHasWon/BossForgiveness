using Microsoft.Xna.Framework.Graphics;
using System;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

public readonly struct MemoryColorInfo
{
    public readonly Color[,] Pixels;
    public readonly int Width;
    public readonly int Height;
    public readonly int FrameHeight;
    public readonly Texture2D Texture;

    internal MemoryColorInfo(int width, int height)
    {
        Width = width; 
        Height = height;
        Pixels = null;
        Texture = null;
    }

    public MemoryColorInfo(Texture2D tex, int npcId)
    {
        Texture = tex;

        var colors = new Color[tex.Width * tex.Height];
        tex.GetData(colors);

        Pixels = new Color[tex.Width, tex.Height];
        Width = tex.Width;
        Height = tex.Height;
        FrameHeight = tex.Height / Main.npcFrameCount[npcId];

        for (int x = 0; x < tex.Width; x++)
        {
            for (int y = 0; y < tex.Height; y++)
            {
                Color col = colors[x + y * Width];
                Pixels[x, y] = col;
            }
        }
    }

    internal static MemoryColorInfo FromEmptySize(NPC npc) => new(npc.width, npc.height);

    public override string ToString() => $"Pixels: {Width * Height}ct; {Width}x{Height} {FrameHeight}fh";
}