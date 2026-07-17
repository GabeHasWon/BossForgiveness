using Microsoft.Xna.Framework.Graphics;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

public readonly struct MemoryColorInfo
{
    public readonly Color[,] Pixels;
    public readonly int Width;
    public readonly int Height;
    public readonly Texture2D Texture;

    public MemoryColorInfo(Texture2D tex)
    {
        Texture = tex;

        var colors = new Color[tex.Width * tex.Height];
        tex.GetData(colors);

        Pixels = new Color[tex.Width / 2, tex.Height / 2];
        Width = tex.Width / 2;
        Height = tex.Height / 2;

        for (int x = 0; x < tex.Width; x += 2)
        {
            for (int y = 0; y < tex.Height; y += 2)
            {
                Color col = colors[x + y * Width];
                Pixels[x / 2, y / 2] = col;
            }
        }
    }

    public override string ToString() => $"Pixels: {Width * Height}ct; {Width}x{Height}";
}