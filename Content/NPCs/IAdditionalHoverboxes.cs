using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;

namespace BossForgiveness.Content.NPCs;

public interface IAdditionalHoverboxes
{
    public List<Hoverbox> GetAdditionalHoverboxes();
}

public readonly struct Hoverbox(int who, Rectangle rect, object mapIcon, string mapName = null)
{
    public readonly int NPCWhoAmI = who;
    public readonly Rectangle Rectangle = rect;
    public readonly object MapIcon = mapIcon;
    public readonly string MapName = mapName;

    public readonly bool Contains(Vector2 pos) => Rectangle.Contains(pos.ToPoint());
}