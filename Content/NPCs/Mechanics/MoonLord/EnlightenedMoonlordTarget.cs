using Microsoft.CodeAnalysis;
using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal enum TargetType
{
    NPCs,
    Dusts,
    Projectiles
}

internal class EnlightenedMoonlordTarget(TargetType type) : ARenderTargetContentByRequest
{
    internal static bool DrawingSpecialML = false;

    private readonly TargetType Type = type;

    internal static HashSet<int> MoonLordProjTypes = [ProjectileID.MoonlordArrow, ProjectileID.MoonlordBullet, ProjectileID.PhantasmalSphere, ProjectileID.PhantasmalDeathray,
        ProjectileID.PhantasmalEye, ProjectileID.MoonlordArrowTrail];

    protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
    {
        Vector2 size = new(device.Viewport.Width, device.Viewport.Height);
        PrepareARenderTarget_WithoutListeningToEvents(ref _target, Main.instance.GraphicsDevice, (int)size.X, (int)size.Y, RenderTargetUsage.PreserveContents);

        device.SetRenderTarget(_target);
        device.Clear(Color.Transparent);

        if (Type == TargetType.Dusts)
            DrawDust(Main.instance);

        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);
        DrawingSpecialML = true;

        if (Type == TargetType.NPCs)
            foreach (NPC npc in Main.ActiveNPCs)
                Main.instance.DrawNPCDirect(spriteBatch, npc, false, Main.screenPosition);

        if (Type == TargetType.Projectiles)
        {
            foreach (Projectile proj in Main.ActiveProjectiles)
            {
                if (!MoonLordProjTypes.Contains(proj.type))
                    continue;

                if (proj.type == ProjectileID.PhantasmalDeathray)
                {
                    if (proj.velocity == Vector2.Zero)
                        return;
                    Texture2D value160 = TextureAssets.Projectile[proj.type].Value;
                    Texture2D value161 = TextureAssets.Extra[ExtrasID.PhantasmalDeathrayBody].Value;
                    Texture2D value162 = TextureAssets.Extra[ExtrasID.PhantasmalDeathrayEnd].Value;
                    float num265 = proj.localAI[1];
                    Color color145 = new Color(255, 255, 255, 255) * 0.9f;
                    Main.EntitySpriteDraw(value160, proj.Center - Main.screenPosition, null, color145, proj.rotation, value160.Size() / 2f, proj.scale, SpriteEffects.None);
                    num265 -= (float)(value160.Height / 2 + value162.Height) * proj.scale;
                    Vector2 center3 = proj.Center;
                    center3 += proj.velocity * proj.scale * value160.Height / 2f;

                    if (num265 > 0f)
                    {
                        float num266 = 0f;
                        Rectangle value163 = new Rectangle(0, 16 * (proj.timeLeft / 3 % 5), value161.Width, 16);
                        while (num266 + 1f < num265)
                        {
                            if (num265 - num266 < (float)value163.Height)
                                value163.Height = (int)(num265 - num266);
                            Main.EntitySpriteDraw(value161, center3 - Main.screenPosition, value163, color145, proj.rotation, new Vector2(value163.Width / 2, 0f), proj.scale, SpriteEffects.None);
                            num266 += (float)value163.Height * proj.scale;
                            center3 += proj.velocity * value163.Height * proj.scale;
                            value163.Y += 16;
                            if (value163.Y + value163.Height > value161.Height)
                                value163.Y = 0;
                        }
                    }

                    Main.EntitySpriteDraw(value162, center3 - Main.screenPosition, null, color145, proj.rotation, value162.Frame().Top(), proj.scale, SpriteEffects.None);
                }
                else
                    Main.instance.DrawProjDirect(proj);
            }
        }

        DrawingSpecialML = false;
        Main.spriteBatch.End();

        device.SetRenderTarget(null);

        _wasPrepared = true;
    }

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "DrawDust")]
    private static extern void DrawDust(Main main);
}

internal class HideProjectiles : GlobalProjectile
{
    public override void Load()
    {
        On_Main.DrawCachedProjs += StopCachedDraws;
        On_Main.DrawDust += StopDrawDust;
    }

    private void StopDrawDust(On_Main.orig_DrawDust orig, Main self)
    {
        if (SubworldSystem.Current is MoonLordPacificationSubworld && (!NPC.AnyNPCs(NPCID.MoonLordCore) || EnlightenedMoonlordTarget.DrawingSpecialML))
            orig(self);
        else if (SubworldSystem.Current is not MoonLordPacificationSubworld)
            orig(self);
    }

    private void StopCachedDraws(On_Main.orig_DrawCachedProjs orig, Main self, List<int> projCache, bool startSpriteBatch) => orig(self, projCache, startSpriteBatch);

    public override bool PreDraw(Projectile projectile, ref Color lightColor)
    {
        if (EnlightenedMoonlordTarget.MoonLordProjTypes.Contains(projectile.type))
            return EnlightenedMoonlordTarget.DrawingSpecialML;

        return false;
    }

    public override bool PreDrawExtras(Projectile projectile)
    {
        if (EnlightenedMoonlordTarget.MoonLordProjTypes.Contains(projectile.type))
            return EnlightenedMoonlordTarget.DrawingSpecialML;

        return true;
    }
}