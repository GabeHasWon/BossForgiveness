using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Vortex;

internal class AuraPlayerLayer : PlayerDrawLayer
{
    public override Position GetDefaultPosition() => new BeforeParent(PlayerDrawLayers.BackAcc);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        if (!drawInfo.drawPlayer.GetModPlayer<VortexModPlayer>().Dummy)
        {
            return;
        }

        Asset<Texture2D> tex = VortexPillarPacificationNPC.Aura;

        for (int i = 0; i < 4; ++i)
        {
            Vector2 pos = drawInfo.Center - Main.screenPosition;
            float rotation = i * MathHelper.TwoPi / 3f + Main.GameUpdateCount * 0.05f * (0.1f + i * 0.3f) + (i % 2 == 0 ? -1 : 1);
            drawInfo.DrawDataCache.Add(new(tex.Value, pos, null, Color.White * 0.12f, rotation, tex.Size() / 2f, 0.8f + i * 0.2f, SpriteEffects.None, 0));
        }
    }
}
