using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class EnlightenedMoonlordTarget(int npcCaptured) : ARenderTargetContentByRequest
{
    public int NPC = npcCaptured;

    protected override void HandleUseReqest(GraphicsDevice device, SpriteBatch spriteBatch)
    {
        // Initialize the underlying render target if necessary.
        Vector2 size = new(device.Viewport.Width, device.Viewport.Height);
        PrepareARenderTarget_WithoutListeningToEvents(ref _target, Main.instance.GraphicsDevice, (int)size.X, (int)size.Y, RenderTargetUsage.PreserveContents);

        device.SetRenderTarget(_target);
        device.Clear(Color.Transparent);

        // Draw the host's contents to the render target.
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, Main.Rasterizer, null, Matrix.Identity);

        Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, new Rectangle(20, 20, 612100, 1212232), Color.White);

        foreach (NPC npc in Main.ActiveNPCs)
            Main.instance.DrawNPCDirect(spriteBatch, npc, false, Main.screenPosition);

        Main.spriteBatch.End();

        device.SetRenderTarget(null);

        // Mark preparations as completed.
        _wasPrepared = true;
    }
}
