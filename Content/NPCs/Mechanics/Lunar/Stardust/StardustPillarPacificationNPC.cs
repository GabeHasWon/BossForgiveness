using BossForgiveness.Content.Items.ForVanilla.Stardust;
using BossForgiveness.Content.Tiles.Vanilla;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Chat;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.UI.Chat;
using Terraria.Utilities;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Stardust;

public enum CompRotation
{
    Up,
    Right,
    Left,
    Down
}

public class Component(Point16 Position, CompRotation Rotation, int Style, bool Placed = false, CompRotation PlacedRotation = CompRotation.Down, bool Hover = false)
{
    public bool Finished => Placed && Rotation == PlacedRotation;

    public Point16 Position = Position;
    public CompRotation Rotation = Rotation;
    public int Style = Style;
    public bool Placed = Placed;
    public CompRotation PlacedRotation = PlacedRotation;
    public bool Hover = Hover;

    public void Write(BinaryWriter writer)
    {
        writer.Write((sbyte)Position.X);
        writer.Write((sbyte)Position.Y);
        writer.Write((byte)Style);
        writer.Write((byte)Rotation);
        writer.Write((byte)PlacedRotation);
        writer.Write(Placed);
    }

    public void Read(BinaryReader reader)
    {
        Position = new Point16(reader.ReadSByte(), reader.ReadSByte());
        Style = reader.ReadByte();
        Rotation = (CompRotation)reader.ReadByte();
        PlacedRotation = (CompRotation)reader.ReadByte();
        Placed = reader.ReadBoolean();
    }
}

internal class StardustPillarPacificationNPC : GlobalNPC
{
    const int ComponentCount = 12;

    public override bool InstancePerEntity => true;

    private static readonly WeightedRandom<int> stardustItemPool = new();

    private static readonly Dictionary<CompRotation, float> rotationToAngle = new()
    {
        { CompRotation.Up, 0 },
        { CompRotation.Left, -MathHelper.PiOver2 },
        { CompRotation.Right, MathHelper.PiOver2 },
        { CompRotation.Down, MathHelper.Pi }
    };

    internal readonly Dictionary<Point16, Component> components = [];

    internal bool won = false;

    private int timer = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.LunarTowerStardust;

    public override void SetStaticDefaults()
    {
        stardustItemPool.Clear();

        foreach (var component in Mod.GetContent<ModItem>())
            if (component is StardustItem)
                stardustItemPool.Add(component.Type, 1);
    }

    public override void AI(NPC npc)
    {
        if (npc.life < npc.lifeMax || won)
            return;

        if (components.Count == 0 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            for (int i = 0; i < ComponentCount; ++i)
            {
                Point16 pos;

                do
                {
                    pos = new Point16(0, 0);

                    if (components.Count is not 0 and < 5)
                        pos = RandomDirection();
                    else if (components.Count >= 4)
                        pos = Main.rand.Next(components.Keys.ToList()) + RandomDirection();
                } while (components.ContainsKey(pos) || CountSides(components, pos) >= 2);

                Component comp = new(pos, (CompRotation)Main.rand.Next(4), components.Count == 0 ? 0 : Main.rand.Next(4) + 1, false);
                components.Add(pos, comp);
            }

            int width = components.MaxBy(x => x.Value.Position.X).Value.Position.X - components.MinBy(x => x.Value.Position.X).Value.Position.X;
            int height = components.MaxBy(x => x.Value.Position.Y).Value.Position.Y - components.MinBy(x => x.Value.Position.Y).Value.Position.Y;
        }

        timer++;
        
        //480
        if (timer % 60 == 0 && Main.netMode != NetmodeID.MultiplayerClient && !won)
        {
            Vector2 position = npc.Center + new Vector2(0, Main.rand.NextFloat(-600, -250)).RotatedByRandom(MathHelper.PiOver2);
            int id;

            do
            {
                id = stardustItemPool;
            } while (!components.Values.Any(x => (ContentSamples.ItemsByType[id].ModItem as StardustItem).PlaceStyle == x.Style && !x.Finished));

            int item = Item.NewItem(npc.GetSource_FromAI(), position, id);
            Main.item[item].velocity = Main.rand.NextVector2CircularEdge(6, 6) * Main.rand.NextFloat(0.6f, 1);
        }
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        foreach (var comp in components)
        {
            comp.Value.Write(binaryWriter);
        }

        bitWriter.WriteBit(won);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        components.Clear();

        for (int i = 0; i < 12; ++i)
        {
            Component comp = new(default, default, default);
            comp.Read(binaryReader);
            components.Add(comp.Position, comp);
        }

        won = bitReader.ReadBit();
    }

    private static int CountSides(Dictionary<Point16, Component> components, Point16 pos)
    {
        int count = 0;

        if (components.ContainsKey(new Point16(pos.X + 1, pos.Y)))
            count++;

        if (components.ContainsKey(new Point16(pos.X - 1, pos.Y)))
            count++;

        if (components.ContainsKey(new Point16(pos.X, pos.Y + 1)))
            count++;

        if (components.ContainsKey(new Point16(pos.X, pos.Y - 1)))
            count++;

        return count;
    }

    private static Point16 RandomDirection() => Main.rand.Next(4) switch
    {
        0 => new Point16(0, 1),
        1 => new Point16(0, -1),
        2 => new Point16(1, 0),
        _ => new Point16(-1, 0),
    };

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.life > npc.lifeMax)
            return;

        Texture2D tile = TextureAssets.Tile[ModContent.TileType<StardustPieces>()].Value;

        foreach (Component comp in components.Values)
        {
            DrawComponent(spriteBatch, tile, npc, comp, comp.Position.X + comp.Position.Y * 1.6f);
        }
    }

    private static void DrawComponent(SpriteBatch spriteBatch, Texture2D tile, NPC npc, Component comp, float offset)
    {
        var src = new Rectangle(34 * comp.Style, 36, 32, 34);
        float opacity = 0.4f;
        var color = Color.Lerp(Color.White * opacity, Color.Blue * (opacity / 2f), MathF.Sin((float)Main.timeForVisualEffects * 0.08f + offset));
        Vector2 position = GetComponentPosition(npc, comp) - Main.screenPosition;

        if (comp.Hover)
        {
            color = Color.Red;
        }

        comp.Hover = false;
        spriteBatch.Draw(tile, position, src, color, rotationToAngle[comp.Rotation], src.Size() / 2f, 1f, SpriteEffects.None, 0);

        if (comp.Placed)
        {
            opacity = 1f;

            if (comp.Finished)
                color = Color.White;
            else
                color = Color.Lerp(Color.White * opacity, Color.Purple, MathF.Sin((float)Main.timeForVisualEffects * 0.08f + offset));

            spriteBatch.Draw(tile, position, src, color, rotationToAngle[comp.PlacedRotation], src.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
    }

    internal static Vector2 GetComponentPosition(NPC npc, Component comp) => npc.Center - new Vector2(0, 350) + comp.Position.ToVector2() * 32;

    internal static bool CheckComponents(Func<Component, NPC, bool> perComponentAction)
    {
        int pillar = NPC.FindFirstNPC(NPCID.LunarTowerStardust);

        if (pillar != -1)
        {
            StardustPillarPacificationNPC pacNPC = Main.npc[pillar].GetGlobalNPC<StardustPillarPacificationNPC>();

            foreach (var comp in pacNPC.components.Values)
            {
                Vector2 position = StardustPillarPacificationNPC.GetComponentPosition(Main.npc[pillar], comp);
                Rectangle bounds = new((int)position.X - 16, (int)position.Y - 16, 32, 34);

                if (bounds.Contains(Main.MouseWorld.ToPoint()))
                    return perComponentAction(comp, Main.npc[pillar]);
            }
        }

        return false;
    }
}
