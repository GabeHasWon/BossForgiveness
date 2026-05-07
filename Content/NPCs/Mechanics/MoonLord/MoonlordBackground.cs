using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria.GameContent;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonlordBackground : ModSystem
{
    public delegate bool PreDrawBackgroundElement(BackgroundElement element, ref Vector2 pos, ref Vector2 scale, ref Vector2 origin, ref Color color);

    public class BackgroundElement(string texture, Vector2 pos, Vector2 scale, Color color, Rectangle? src, float parallaxLevel, float rotation, PreDrawBackgroundElement onDraw)
    {
        public string Texture = texture;
        public Vector2 Position = pos;
        public Vector2 Scale = scale;
        public Color Color = color;
        public Rectangle? Source = src;
        public float Parallax = parallaxLevel;
        public PreDrawBackgroundElement OnDraw = onDraw;
        public bool Flipped = Main.rand.NextBool(2);
        public float Rotation = rotation;
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
        Add("StillnessObject");

        static void Add(string tex) => Textures.Add(tex, Request(tex));
        static Asset<Texture2D> Request(string tex) => ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/MoonLord/" + tex);
    }

    private static void GetScreenDrawArea(Vector2 screenPosition, Vector2 offSet, out int firstTileX, out int lastTileX, out int firstTileY, out int lastTileY)
    {
        firstTileX = (int)((screenPosition.X - offSet.X) / 16f - 1f);
        lastTileX = (int)((screenPosition.X + Main.screenWidth + offSet.X) / 16f) + 2;
        firstTileY = (int)((screenPosition.Y - offSet.Y) / 16f - 1f);
        lastTileY = (int)((screenPosition.Y + Main.screenHeight + offSet.Y) / 16f) + 5;

        if (firstTileX < 4)
            firstTileX = 4;
        if (lastTileX > Main.maxTilesX - 4)
            lastTileX = Main.maxTilesX - 4;
        if (firstTileY < 4)
            firstTileY = 4;
        if (lastTileY > Main.maxTilesY - 4)
            lastTileY = Main.maxTilesY - 4;
    }

    internal static void Draw()
    {
        PreDrawUpdate(Main.LocalPlayer, Main.LocalPlayer.GetModPlayer<MoonlordDomainPlayer>().DomainTimer);
        GetScreenDrawArea(Main.screenPosition, Vector2.Zero, out int left, out int right, out int top, out int bottom);

        Rectangle area = new(left * 16, top * 16, (right - left) * 16, (bottom - top) * 16);

        Main.spriteBatch.Draw(Textures["Gradient"].Value, new Rectangle(-20, -20, Main.screenWidth + 40, Main.screenHeight + 40), Color.White);

        float yPos = Main.LocalPlayer.Center.Y / 16f;

        if (yPos < MoonLordPacificationSubworld.StillnessLayer + 20)
        {
            float opacity = Utils.GetLerpValue(MoonLordPacificationSubworld.StillnessLayer + 20, MoonLordPacificationSubworld.StillnessLayer, yPos, true);

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(-20, -20, Main.screenWidth + 40, Main.screenHeight + 40), Color.Black * opacity);
        }

        foreach (var element in Elements)
        {
            Vector2 visualOffset = Main.screenPosition * new Vector2(element.Parallax, element.Parallax);
            Vector2 drawPosition = element.Position + visualOffset - Main.screenPosition;
            Vector2 scale = Vector2.One;
            Vector2 origin = Vector2.Zero;
            Color color = element.Color;
            Texture2D texture = Textures[element.Texture].Value;
            Rectangle box = new((int)drawPosition.X - texture.Width * 2 + (int)Main.screenPosition.X, 
                (int)drawPosition.Y - texture.Width * 2 + (int)Main.screenPosition.Y, texture.Width * 4, texture.Width * 4);

            if (!area.Intersects(box))
                continue;

            if (element.OnDraw?.Invoke(element, ref drawPosition, ref scale, ref origin, ref color) != false)
            {
                SpriteEffects flip = element.Flipped ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                Main.spriteBatch.Draw(texture, drawPosition, element.Source, color, element.Rotation, origin, scale, flip, 0);
            }
        }
    }

    private static void PreDrawUpdate(Player player, int domainTimer)
    {
        //Elements.Clear();
        //ElementCountsByName.Clear();

        if (!MaxedElements("Object0", 1250))
        {
            for (int i = 0; i < 250; ++i)
            {
                var pos = new Vector2(Main.rand.NextFloat(300, Main.maxTilesX * 8 - 300), Main.rand.NextFloat(Main.maxTilesY * 0.65f, Main.maxTilesY * 0.68f) * 16);
                var scale = new Vector2(Main.rand.NextFloat(0.5f, 0.9f), Main.rand.NextFloat(0.8f, 2f));

                Rectangle src = new(0, 82 * Main.rand.Next(4), 100, 80);
                AddElement(new BackgroundElement("Object0", pos, scale, Color.White, src, Main.rand.NextFloat(1f), Main.rand.NextFloat(-0.2f, 0.2f), PreDrawMiscObject));
            }
        }

        if (!MaxedElements("StillnessObject", 3010))
        {
            float bossFactor = PacificationTracker.MoonLordBeatFactor;

            for (int i = 0; i < 250; ++i)
            {
                float parallax = Main.rand.NextFloat(1f);
                var pos = new Vector2(Main.rand.NextFloat(300, Main.maxTilesX * 8 - 300), Main.rand.NextFloat(Main.maxTilesY * 0.05f, Main.maxTilesY * 0.15f) * 16);
                pos *= MathHelper.Lerp(1, 1.8f, 1 - parallax);
                var scale = new Vector2(Main.rand.NextFloat(0.5f, 0.9f), Main.rand.NextFloat(0.8f, 2f));

                Rectangle src = new(37 * Main.rand.Next(4), 37 * Main.rand.Next(4), 36, 36);
                var topRange = Color.Lerp(Color.White, Main.hslToRgb(new Vector3(Main.rand.NextFloat(1f), 1, 0.5f)), bossFactor);
                float rot = Main.rand.NextFloat(MathHelper.TwoPi);
                AddElement(new BackgroundElement("StillnessObject", pos, scale, Color.Lerp(topRange, Color.Gray * 0.5f, parallax), src, parallax, rot, PreDrawMiscObject));
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
