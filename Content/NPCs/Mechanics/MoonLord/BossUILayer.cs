using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Graphics;
using SubworldLibrary;
using System.Collections.Generic;
using System.Linq;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;
using Terraria.UI.Chat;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

#nullable enable

internal class BossUILayer : ModSystem
{
    public delegate bool DisplayDelegate(ref bool defaultValue);

    public readonly record struct BossEntry(int UISlot, float Priority, DisplayDelegate? CanDisplay = null);

    public readonly static Dictionary<int, BossEntry> BossIDToLayerID = [];

    private static Asset<Texture2D> BossUI = null!;
    private static string? HoverName = null;
    private static bool HoverGood = false;

    public override void Load()
    {
        // TODO: Relational placements
        AddLayer(NPCID.KingSlime, 4, 1);
        AddLayer(NPCID.EyeofCthulhu, 0, 2);
        AddLayer(NPCID.EaterofWorldsHead, 1, 3, (ref bool def) => def && !WorldGen.crimson);
        AddLayer(NPCID.BrainofCthulhu, 2, 3, (ref bool def) => def && WorldGen.crimson);
        AddLayer(NPCID.QueenBee, 15, 3.5f);
        AddLayer(NPCID.Deerclops, 16, 3.5f);
        AddLayer(NPCID.SkeletronHead, 8, 4);
        AddLayer(NPCID.WallofFlesh, 3, 5);
        AddLayer(NPCID.QueenSlimeBoss, 14, 5.5f);

        AddLayer(NPCID.Retinazer, 6, 6, (ref bool def) =>
        {
            def = PacificationTracker.HasBoss(NPCID.Retinazer) && PacificationTracker.HasBoss(NPCID.Spazmatism);
            return true;
        });

        AddLayer(NPCID.TheDestroyer, 7, 6);
        AddLayer(NPCID.SkeletronPrime, 9, 6);
        AddLayer(NPCID.Plantera, 10, 7);
        AddLayer(NPCID.Golem, 11, 8);
        AddLayer(NPCID.DukeFishron, 12, 9);
        AddLayer(NPCID.HallowBoss, 13, 9);
        AddLayer(NPCID.CultistBoss, 5, 10);

        BossUI = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/MoonLord/BossUI");

        static void AddLayer(int npcId, int uiSlot, float prio, DisplayDelegate? canDisplay = null) => BossIDToLayerID.Add(npcId, new BossEntry(uiSlot, prio, canDisplay));
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        if (SubworldSystem.Current is not MoonLordPacificationSubworld || !Main.playerInventory)
            return;

        int layer = layers.FindIndex(x => x.Name == "Vanilla: Inventory");
        int mouse = layers.FindIndex(x => x.Name == "Vanilla: Mouse Over");

        if (layer != -1)
        {
            layers.Insert(layer, new LegacyGameInterfaceLayer("Pacifist Route Pacification UI", () =>
            {
                DrawBossUI();
                return true;
            }));
        }

        if (mouse != -1)
        {
            layers.Insert(mouse + 1, new LegacyGameInterfaceLayer("Pacifist Route Pacification Text", () =>
            {
                if (HoverName is null)
                    return true;

                DynamicSpriteFont font = FontAssets.MouseText.Value;
                Color color = HoverGood ? Color.Green : Color.White;
                ChatManager.DrawColorCodedStringWithShadow(Main.spriteBatch, font, HoverName, Main.MouseScreen + new Vector2(20, 0), color, 0f, Vector2.Zero, Vector2.One);
                return true;
            }));
        }
    }

    private static void DrawBossUI()
    {
        int x = 20;
        int y = 270;
        Texture2D tex = BossUI.Value;
        var sorted = BossIDToLayerID.OrderBy(x => x.Value.Priority);
        HoverName = null;
        HoverGood = false;

        foreach (var pair in sorted)
        {
            bool hasPac = PacificationTracker.HasBoss(pair.Key);

            if (pair.Value.CanDisplay is not null && !pair.Value.CanDisplay.Invoke(ref hasPac))
                continue;

            int off = pair.Value.UISlot;
            var src = new Rectangle(30 * off, hasPac ? 30 : 0, 28, 28);
            Main.spriteBatch.Draw(tex, new Vector2(x, y), src, Main.MouseTextColorReal);

            if (new Rectangle(x, y, 30, 30).Contains(Main.MouseScreen.ToPoint()))
            {
                HoverName = Lang.GetNPCNameValue(pair.Key);
                HoverGood = hasPac;
            }

            x += 30;

            if (x > 420)
            {
                x = 20;
                y += 30;
            }
        }
    }
}
