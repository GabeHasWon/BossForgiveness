using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.GameContent;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

public class Memory(int npc, Vector2 position, Memory.AnimationDelegate animation = null)
{
    public struct MemoryParticle
    {
        public static readonly Asset<Texture2D> MemoryTex = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/MoonLord/Memories");

        public readonly Memory Parent;
        public readonly Color Color = Color.Transparent;
        public readonly Vector2 Offset;
        public readonly byte Frame = (byte)Main.rand.Next(5);
        public readonly float Speed = Main.rand.NextFloat(0.7f, 1.2f);

        public float Timer = 0;

        public MemoryParticle(Memory parent, MemoryColorInfo info)
        {
            Parent = parent;
            Rectangle src = parent.Frame;

            while (Color.A == 0)
            {
                int x = Main.rand.Next(src.Width / 2);
                int y = src.Y / 2 + Main.rand.Next(src.Height / 2);

                Color = info.Pixels[x, y];
                Offset = new Vector2(x * 2, y);
            }
        }

        public readonly void Draw()
        {
            Vector2 position = Parent.Position + Offset.RotatedBy(Parent.Rotation);
            Main.spriteBatch.Draw(MemoryTex.Value, position - Main.screenPosition, new Rectangle((int)Timer / 10 * 20, 20 * Frame, 20, 20), Color with { A = 0 });
        }
    }

    public delegate void AnimationDelegate(Memory self, float bossProgression);

    public readonly MemoryColorInfo Colors = BossMemories.InfoByType[npc];
    public readonly List<MemoryParticle> Particles = [];
    public readonly Texture2D Texture = TextureAssets.Npc[npc].Value;
    public readonly int NpcType = npc;
    public readonly AnimationDelegate Animation = animation ?? MemoryAnimations.DefaultAnimation;
    
    public Vector2 Position = position;
    public Vector2 Velocity = Vector2.Zero;
    public Rectangle Frame = Rectangle.Empty;
    public float Rotation = 0;
    public int LifeTime = 0;

    public void Update()
    {
        Position += Velocity;
        LifeTime++;

        foreach (ref MemoryParticle particle in CollectionsMarshal.AsSpan(Particles))
            particle.Timer += particle.Speed;

        Particles.RemoveAll(x => x.Timer > 49);
    }

    public void Draw()
    {
        Animation.Invoke(this, 1);

        foreach (ref MemoryParticle particle in CollectionsMarshal.AsSpan(Particles))
            particle.Draw();

        while (Particles.Count < Texture.Width * Texture.Height * 0.002f)
            Particles.Add(new MemoryParticle(this, Colors));
    }
}