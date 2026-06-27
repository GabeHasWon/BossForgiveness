using Humanizer;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using SubworldLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Terraria.GameContent;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonlordBackground : ModSystem
{
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct TrailDrawer
    {
        private static readonly VertexStrip _vertexStrip = new();

        public readonly void Draw(Vector2 size, Vector2[] positions, float[] rotations)
        {
            MiscShaderData miscShaderData = GameShaders.Misc["MagicMissile"];
            miscShaderData.UseSaturation(-2.8f);
            miscShaderData.UseOpacity(2f);
            miscShaderData.Apply();
            Vector2 drawPos = -Main.screenPosition + size / 2f;
            _vertexStrip.PrepareStripWithProceduralPadding(positions, rotations, StripColors, StripWidth, drawPos);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        private readonly Color StripColors(float progressOnStrip)
        {
            Color result = Color.Lerp(Color.White, Color.Gray, Utils.GetLerpValue(0f, 0.7f, progressOnStrip, true)) * (1f - Utils.GetLerpValue(0f, 0.98f, progressOnStrip));
            result.A = (byte)(result.A * 0.7f);
            return result;
        }

        private static float StripWidth(float progress) => MathHelper.Lerp(30, 42f, Utils.GetLerpValue(0f, 0.2f, progress, true)) * Utils.GetLerpValue(0f, 0.07f, progress, true);
    }

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

    public class MovingElement(string texture, Vector2 pos, Vector2 scale, Color color, Rectangle? src, float para, float rotation, PreDrawBackgroundElement onDraw, int trailCount)
        : BackgroundElement(texture, pos, scale, color, src, para, rotation, onDraw)
    {
        public List<Vector2> OldPositions = new(trailCount);
        public List<float> OldRotations = new(trailCount);
        public Vector2 Velocity = Vector2.Zero;

        public void Update()
        {
            OldPositions.Add(Position);
            OldRotations.Add(Rotation);

            if (OldPositions.Count > trailCount)
            {
                OldPositions.RemoveAt(OldPositions.Count - 1);
                OldRotations.RemoveAt(OldRotations.Count - 1);
            }

            Velocity = new Vector2(0, -0.5f);
            Position += Velocity;
        }
    }

    public static Dictionary<string, Asset<Texture2D>> Textures = [];
    public static List<BackgroundElement> Elements = [];
    public static Dictionary<string, int> ElementCountsByName = [];

    private static EnlightenedMoonlordTarget NPCTarget = null;
    private static EnlightenedMoonlordTarget DustTarget = null;
    private static EnlightenedMoonlordTarget ProjectileTarget = null;

    public override void Load()
    {
        Textures.Clear();
        Add("Gradient");
        Add("Shinespot");
        Add("Object0");
        Add("StillnessObject");
        Add("DancingWyrms");

        SetTarget(TargetType.NPCs, ref NPCTarget);
        SetTarget(TargetType.Dusts, ref DustTarget);
        SetTarget(TargetType.Projectiles, ref ProjectileTarget);

        On_Main.DoDraw_DrawNPCsBehindTiles += DrawOverMoonLord;

        ModContent.Request<Effect>("BossForgiveness/Assets/Effects/MirageEffect");

        static void Add(string tex) => Textures.Add(tex, Request(tex));
        static Asset<Texture2D> Request(string tex) => ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/MoonLord/" + tex);

        static void SetTarget(TargetType type, ref EnlightenedMoonlordTarget target)
        {
            target = new EnlightenedMoonlordTarget(type);
            Main.ContentThatNeedsRenderTargets.Add(target);
        }
    }

    private void DrawOverMoonLord(On_Main.orig_DoDraw_DrawNPCsBehindTiles orig, Main self)
    {
        orig(self);

        // If the drawer isn't ready, wait until it is.
        if (SubworldSystem.Current is MoonLordPacificationSubworld)
        {
            GraphicsDevice device = Main.instance.GraphicsDevice;
            Effect effect = ModContent.Request<Effect>("BossForgiveness/Assets/Effects/MirageEffect").Value;

            float str = Main.LocalPlayer.selectedItem / 9f;// PacificationTracker.MoonLordBeatFactor;

            DrawTarget(device, effect, str, NPCTarget, Color.White);
            DrawTarget(device, effect, str, DustTarget, Color.Pink);
            DrawTarget(device, effect, str, ProjectileTarget, new Color(255, 100, 100));
        }

        DustTarget.Request();
        NPCTarget.Request();
        ProjectileTarget.Request();
    }

    private static void DrawTarget(GraphicsDevice device, Effect effect, float str, EnlightenedMoonlordTarget target, Color color)
    {
        effect.Parameters["effectStrength"].SetValue(MathF.Min(1.5f - str, 1));
        effect.Parameters["strength"].SetValue(1 - str + 0.001f);
        effect.Parameters["resolution"].SetValue(new Vector2(device.Viewport.Width, device.Viewport.Height));
        effect.Parameters["timer"].SetValue((float)(Main.timeForVisualEffects * 0.06f));
        effect.Parameters["white"].SetValue(str);
        effect.Parameters["fadeColor"].SetValue(color.ToVector3());
        effect.Parameters["bleedEffect"].SetValue(0.5f);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointWrap, DepthStencilState.Default, Main.Rasterizer, effect);

        Main.spriteBatch.Draw(target.GetTarget(), Vector2.Zero, Color.White);

        Main.spriteBatch.End();
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

    public override void PostUpdateDusts()
    {
        if (Main.dedServ)
            return;

        foreach (BackgroundElement element in Elements)
        {
            if (element is MovingElement moving)
                moving.Update();
        }
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

        if (domainTimer % 30 == 0 && !MaxedElements("DancingWyrms", 2500))
        {
            for (int i = 0; i < 10; ++i)
            {
                var pos = new Vector2(Main.rand.NextFloat(300, Main.maxTilesX * 8 - 300), Main.rand.NextFloat(Main.maxTilesY * 0.65f, Main.maxTilesY * 0.68f) * 16);
                var scale = new Vector2(Main.rand.NextFloat(0.5f, 2f));

                AddElement(new MovingElement("DancingWyrms", pos, scale, Color.Lerp(Color.White, Color.Gray * 0.5f, 0), null, 1f, 0f, PreDrawWyrm, Main.rand.Next(20, 80))
                {
                    Velocity = new Vector2(0, -2f)
                });
            }
        }
    }

    private static bool PreDrawWyrm(BackgroundElement element, ref Vector2 pos, ref Vector2 scale, ref Vector2 origin, ref Color color)
    {
        PreDrawMiscObject(element, ref pos, ref scale, ref origin, ref color);

        var self = (MovingElement)element;
        new TrailDrawer().Draw(Textures[self.Texture].Size(), [.. self.OldPositions], [.. self.OldRotations]);
        self.Velocity.Y = -0.5f;
        return true;
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
