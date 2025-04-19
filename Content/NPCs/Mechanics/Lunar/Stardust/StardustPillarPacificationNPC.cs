using BossForgiveness.Content.Tiles.Vanilla;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Stardust;

public enum ComponentRotation
{
    Up,
    Right,
    Left,
    Down
}

public record Component(Point16 Position, ComponentRotation Rotation, int Style);

internal class StardustPillarPacificationNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private readonly Dictionary<Point16, Component> components = [];

    private Point16 size = new Point16();

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.LunarTowerStardust;

    public override void AI(NPC npc)
    {
        if (npc.life < npc.lifeMax)
            return;

        if (components.Count == 0)
        {
            for (int i = 0; i < 15; ++i)
            {
                Point16 pos;

                do
                {
                    pos = components.Count == 0 ? new Point16(5, 3) : Main.rand.Next(components.Keys.ToList()) + RandomDirection();
                } while (components.ContainsKey(pos));

                Component comp = new(pos, (ComponentRotation)Main.rand.Next(4), components.Count == 0 ? 0 : Main.rand.Next(4) + 1);
                components.Add(pos, comp);
            }

            int width = components.MaxBy(x => x.Value.Position.X).Value.Position.X - components.MinBy(x => x.Value.Position.X).Value.Position.X;
            int height = components.MaxBy(x => x.Value.Position.Y).Value.Position.Y - components.MinBy(x => x.Value.Position.Y).Value.Position.Y;
            size = new Point16(width * 32, height * 32);
        }
    }

    private static Point16 RandomDirection() => Main.rand.Next(4) switch 
    {
        0 => new Point16(0, 1),
        1 => new Point16(0, -1),
        2 => new Point16(1, 0),
        _ => new Point16(1, 0),
    };

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tile = TextureAssets.Tile[ModContent.TileType<StardustPieces>()].Value;
        Vector2 basePos = npc.position + new Vector2(-size.X, 350 - size.Y / 2) - Main.screenPosition;

        foreach (Component comp in components.Values)
        {
            float rotation = comp.Rotation switch
            {
                ComponentRotation.Up => 0,
                ComponentRotation.Left => -MathHelper.PiOver2,
                ComponentRotation.Right => MathHelper.PiOver2,
                _ => MathHelper.Pi,
            };

            var src = new Rectangle(18 * comp.Style, 0, 16, 16);
            spriteBatch.Draw(tile, basePos + comp.Position.ToWorldCoordinates() * 2, src, Color.White, rotation, src.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
    }
}
