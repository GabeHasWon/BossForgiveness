using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Terraria.GameContent;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

public class Memory(int npc, Vector2 position, Memory.UpdateDelegate update = null, float particleRatio = 0.002f)
{
    public struct MemoryParticle
    {
        public static readonly Asset<Texture2D> MemoryTex = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/MoonLord/Memories");

        public readonly Memory Parent;
        public readonly Color Color = Color.Transparent;
        public readonly Vector2 Offset;
        public readonly byte Frame = (byte)Main.rand.Next(5);
        public readonly float Speed = Main.rand.NextFloat(0.7f, 1.2f);
        public readonly float DecayStrength = Main.rand.NextFloat(0.1f, 0.4f);
        public readonly bool DecayForward = Main.rand.NextBool();

        public float Timer = 0;
        public Vector2 DecayOffset = Vector2.Zero;

        public MemoryParticle(Memory parent, MemoryColorInfo info)
        {
            Parent = parent;
            Rectangle src = parent.Frame;

            while (Color.A == 0)
            {
                int x = Main.rand.Next(src.Width);
                int y = src.Y + Main.rand.Next(src.Height);

                Color = info.Pixels[x, y];
                Offset = new Vector2(x % src.Width, y % src.Height);
            }
        }

        public readonly void Draw()
        {
            Vector2 rotation = (Offset - Parent.Size / 2f).RotatedBy(Parent.Rotation);
            Vector2 position = Parent.Center + DecayOffset + rotation;
            var source = new Rectangle((int)Timer / 10 * 20, 20 * Frame, 20, 20);
            Main.spriteBatch.Draw(MemoryTex.Value, position - Main.screenPosition, source, Color with { A = 0 }, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0);
        }
    }

    public delegate void UpdateDelegate(Memory self, float bossProgression);

    public readonly MemoryColorInfo Colors = BossMemories.InfoByType[npc];
    public readonly Texture2D Texture = TextureAssets.Npc[npc].Value;
    public readonly int NpcType = npc;
    public readonly UpdateDelegate Updating = update ?? MemoryDelegates.DefaultAnimation;
    
    public Vector2 Size => new(Colors.Width, Colors.FrameHeight);
    public Vector2 Center => Position + Size / 2f;
    
    public List<MemoryParticle> Particles = [];
    public Vector2 Position = position;
    public Vector2 Velocity = Vector2.Zero;
    public Rectangle Frame = Rectangle.Empty;
    public float Rotation = 0;
    public float LastRotation = 0;
    public int LifeTime = 0;
    public int DespawnTime = 0;
    public bool Collected = false;
    public float ParticleRatio = particleRatio;
    public object AddedInfo = new();

    public void Update()
    {
        Position += Velocity;
        LifeTime++;

        LastRotation = Rotation;

        if (!Collected)
            Updating.Invoke(this, 1);
        else
        {
            Velocity.X *= 0.95f;
            Velocity.Y = MathHelper.Lerp(Velocity.Y, -12, 0.03f);
        }

        if (!Collected)
        {
            Rectangle bounds = new((int)Position.X, (int)Position.Y, (int)Size.X, (int)Size.Y);

            foreach (Player player in Main.ActivePlayers)
            {
                if (player.Hitbox.Intersects(bounds))
                {
                    Collected = true;
                    var npc = MoonLordPacificationNPC.GetPacificationNPC();
                    npc?.PacifiedBosses.Add(NpcType);

                    break;
                }
            }
        }

        foreach (ref MemoryParticle particle in CollectionsMarshal.AsSpan(Particles))
        {
            particle.Timer += particle.Speed;
            particle.DecayOffset += Velocity * particle.DecayStrength * (particle.DecayForward ? 1 : -1);
        }

        Particles.RemoveAll(x => x.Timer > 49);
    }

    public void Draw()
    {
        foreach (ref MemoryParticle particle in CollectionsMarshal.AsSpan(Particles))
            particle.Draw();

        if (Frame == Rectangle.Empty || Collected)
            return;

        float cap = Texture.Width * Texture.Height * ParticleRatio;

        while (Particles.Count < cap)
        {
            var particle = new MemoryParticle(this, Colors);

            if (Particles.Count < cap / 2)
                particle.Timer = Main.rand.Next(40);

            Particles.Add(particle);
        }
    }

    public Memory Clone()
    {
        var clone = (Memory)MemberwiseClone();
        clone.Particles = [];
        return clone;
    }
}