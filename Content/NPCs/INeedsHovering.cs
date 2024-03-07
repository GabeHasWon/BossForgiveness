using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;

namespace BossForgiveness.Content.NPCs;

public interface INeedsHovering
{
    public List<Hover> Hovers();
}

public readonly struct Hover(int who, Rectangle rect)
{
    public readonly int NPCWhoAmI = who;
    public readonly Rectangle Rectangle = rect;

    public readonly bool Contains(Vector2 pos) => Rectangle.Contains(pos.ToPoint());
}