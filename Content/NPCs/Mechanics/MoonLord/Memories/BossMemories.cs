using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord.Memories;

internal class BossMemories : ModSystem
{
    public static readonly Dictionary<int, MemoryColorInfo> InfoByType = [];

    public static readonly int[] NormalBosses = [NPCID.EyeofCthulhu, NPCID.KingSlime, NPCID.QueenBee, NPCID.Deerclops, NPCID.SkeletronHead, NPCID.CultistBoss, NPCID.QueenSlimeBoss,
        NPCID.SkeletronPrime, NPCID.Spazmatism, NPCID.Retinazer, NPCID.Plantera, NPCID.Golem, NPCID.DukeFishron, NPCID.HallowBoss];

    public List<Memory> Memories = [];

    public override void Load() => Main.RunOnMainThread(() =>
    {
        for (int i = 0; i < NormalBosses.Length; ++i)
        {
            int npcType = NormalBosses[i];
            Main.instance.LoadNPC(npcType);

            MemoryColorInfo info;

            if (!Main.dedServ)
            {
                Texture2D tex = TextureAssets.Npc[npcType].Value;
                info = new(tex, npcType);
            }
            else
                info = MemoryColorInfo.FromEmptySize(ContentSamples.NpcsByNetId[npcType]);

            InfoByType.Add(npcType, info);
        }
    });

    public override void PostUpdateDusts()
    {
        foreach (Memory mem in Memories)
            mem.Update();

        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.U) && Main.oldKeyState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.U))
            Memories.Add(new Memory(NPCID.EyeofCthulhu, Main.MouseWorld, MemoryDelegates.EoCUpdate));
    }

    public override void PostDrawTiles()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, Main.Rasterizer);

        foreach (Memory mem in Memories)
            mem.Draw();

        Main.spriteBatch.End();
    }
}
