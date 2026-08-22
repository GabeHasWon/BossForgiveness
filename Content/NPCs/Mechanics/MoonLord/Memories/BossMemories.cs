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

    public static Dictionary<int, Memory> BossMemoryTemplates;

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

        BossMemoryTemplates = new()
        {
            { NPCID.EyeofCthulhu, new Memory(NPCID.EyeofCthulhu, Vector2.Zero, MemoryDelegates.EoCUpdate) },
            { NPCID.KingSlime, new Memory(NPCID.KingSlime, Vector2.Zero, MemoryDelegates.KingSlimeUpdate) },
            { NPCID.SkeletronHead, new Memory(NPCID.SkeletronHead, Vector2.Zero, MemoryDelegates.SkeletronUpdate, 0.015f) },
            { NPCID.QueenBee, new Memory(NPCID.QueenBee, Vector2.Zero, MemoryDelegates.QueenBeeUpdate) },
            { NPCID.CultistBoss, new Memory(NPCID.CultistBoss, Vector2.Zero, MemoryDelegates.CultistUpdate, 0.005f) },
        };
    });

    public override void PostUpdateDusts()
    {
        foreach (Memory mem in Memories)
            mem.Update();

        Memories.RemoveAll(x => x.Collected && x.Particles.Count == 0);

        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Y) && Main.oldKeyState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.Y))
            Memories.Clear();

        if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.U) && Main.oldKeyState.IsKeyUp(Microsoft.Xna.Framework.Input.Keys.U))
        {
            List<int> unpac = MoonLordPacificationNPC.GetUnpacifiedBosses();

            if (unpac.Count > 0)
            {
                int npc;

                do
                    npc = Main.rand.Next(unpac);
                while (!BossMemoryTemplates.ContainsKey(npc));

                CreateMemory(npc, Main.MouseWorld);
            }
        }
    }

    public void CreateMemory(int npc, Vector2 pos)
    {
        Memory item = BossMemoryTemplates[npc].Clone();
        item.Position = pos;
        Memories.Add(item);
    }

    public override void PostDrawTiles()
    {
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.Default, Main.Rasterizer);

        foreach (Memory mem in Memories)
            mem.Draw();

        Main.spriteBatch.End();
    }
}
