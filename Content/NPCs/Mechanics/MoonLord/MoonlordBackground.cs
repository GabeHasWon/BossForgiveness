using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonlordBackground : ModSystem
{
    public delegate bool PreDrawBackgroundElement(BackgroundElement element, ref Vector2 pos, ref Vector2 scale, ref Vector2 origin, ref Color color);

    public class BackgroundElement(string texture, Vector2 pos, Vector2 scale, Color color, Rectangle? src, float parallaxLevel, PreDrawBackgroundElement onDraw)
    {
        public string Texture = texture;
        public Vector2 Position = pos;
        public Vector2 Scale = scale;
        public Color Color = color;
        public Rectangle? Source = src;
        public float Parallax = parallaxLevel;
        public PreDrawBackgroundElement OnDraw = onDraw;
        public bool Flipped = Main.rand.NextBool(2);
    }

    public static Dictionary<string, Asset<Texture2D>> Textures = [];
    public static List<BackgroundElement> Elements = [];
    public static Dictionary<string, int> ElementCountsByName = [];

    public override void Load()
    {
        Textures.Clear();
        Add("Gradient");
        Add("Shinespot");
        Add("Object0");

        static void Add(string tex) => Textures.Add(tex, Request(tex));
        static Asset<Texture2D> Request(string tex) => ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/MoonLord/" + tex);
    }

    internal static void Draw()
    {
        PreDrawUpdate(Main.LocalPlayer, Main.LocalPlayer.GetModPlayer<MoonlordDomainPlayer>().DomainTimer);

        Main.spriteBatch.Draw(Textures["Gradient"].Value, new Rectangle(-20, -20, Main.screenWidth + 40, Main.screenHeight + 40), Color.White);

        foreach (var element in Elements)
        {
            Vector2 visualOffset = Main.screenPosition * new Vector2(element.Parallax, element.Parallax);
            Vector2 drawPosition = element.Position + visualOffset - Main.screenPosition;
            Vector2 scale = Vector2.One;
            Vector2 origin = Vector2.Zero;
            Color color = element.Color;

            if (element.OnDraw?.Invoke(element, ref drawPosition, ref scale, ref origin, ref color) != false)
            {
                SpriteEffects flip = element.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Main.spriteBatch.Draw(Textures[element.Texture].Value, drawPosition, element.Source, color, 0f, origin, scale, flip, 0);
            }
        }
    }

    private static void PreDrawUpdate(Player player, int domainTimer)
    {
        if (!MaxedElements("Object0", 1250))
        {
            for (int i = 0; i < 250; ++i)
            {
                var pos = new Vector2(Main.rand.NextFloat(-160, Main.maxTilesX * 8 + 160), Main.rand.NextFloat(Main.maxTilesY * 0.65f, Main.maxTilesY * 0.7f) * 16);
                var scale = new Vector2(Main.rand.NextFloat(0.5f, 0.9f), Main.rand.NextFloat(0.8f, 2f));

                Rectangle src = new(0, 82 * Main.rand.Next(4), 100, 80);
                AddElement(new BackgroundElement("Object0", pos, scale, Color.White, src, Main.rand.NextFloat(1f), PreDrawMiscObject));
            }
        }
    }

    private static bool PreDrawMiscObject(BackgroundElement element, ref Vector2 pos, ref Vector2 scale, ref Vector2 origin, ref Color color)
    {
        float factor = element.Parallax;
        scale *= 1 - factor;
        color = Color.Lerp(color, Color.Gray, factor);
        return true;
    }

    private static void AddElement(BackgroundElement element)
    {
        Elements.Add(element);
        ElementCountsByName.TryAdd(element.Texture, 0);
        ElementCountsByName[element.Texture]++;
    }

    private static bool MaxedElements(string name, int max) => ElementCountsByName.TryGetValue(name, out int count) && count >= max;
}
