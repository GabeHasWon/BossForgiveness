using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.UI;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

internal class MemoryMapLayer : ModMapLayer
{
    public override void Draw(ref MapOverlayDrawContext context, ref string text)
    {
        foreach (Memory mem in ModContent.GetInstance<BossMemories>().Memories)
        {
            NPC npc = ContentSamples.NpcsByNetId[mem.NpcType];
            int index = npc.GetBossHeadTextureIndex();

            if (index == -1)
                continue;

            var pos = mem.Center.ToTileCoordinates().ToVector2();
            var result = context.Draw(TextureAssets.NpcHeadBoss[index].Value, pos, Color.White * 0.5f, new SpriteFrame(1, 1), 1, 1, Alignment.Center, SpriteEffects.None);

            if (result.IsMouseOver)
                text = npc.TypeName;
        }
    }
}
