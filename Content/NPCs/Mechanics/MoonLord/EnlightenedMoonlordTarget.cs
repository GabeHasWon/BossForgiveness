using Microsoft.Xna.Framework.Graphics;
using SubworldLibrary;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        if (SubworldSystem.Current is MoonLordPacificationSubworld && EnlightenedMoonlordTarget.DrawingSpecialML)
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