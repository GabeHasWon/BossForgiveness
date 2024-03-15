using BossForgiveness.Content.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.Graphics.Renderers;
using Terraria.ID;
using Terraria.Localization;
using Terraria.Map;
using Terraria.ModLoader;
using Terraria.UI;

namespace BossForgiveness.Content.NPCs;

internal class HoverOverSegmentPlayer : ModPlayer
{
    private static readonly List<Hoverbox> hovers = [];

    public override void Load() => IL_Player.Update += HijackRangeForTalkNPC;

    private static void HijackRangeForTalkNPC(ILContext il)
    {
        ILCursor c = new(il);

        c.GotoNext(x => x.MatchCall<Player>(nameof(Player.SetTalkNPC)));
        c.GotoPrev(x => x.MatchLdsfld<Player>(nameof(Player.tileRangeY)));
        c.GotoNext(MoveType.After, x => x.MatchCall(typeof(Rectangle).GetConstructor([typeof(int), typeof(int), typeof(int), typeof(int)])));

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldloca_S, (byte)147);
        c.EmitDelegate(ModifyRangeRectangle);
    }

    public static void ModifyRangeRectangle(Player player, ref Rectangle rectangle)
    {
        if (player.TalkNPC is not null && player.TalkNPC.ModNPC is IAdditionalHoverboxes)
        {
            int tileRange = int.MaxValue / 32 - 20;
            rectangle = new((int)(player.Center.X - (tileRange * 16)), (int)(player.Center.Y - (tileRange * 16)), tileRange * 16 * 2, tileRange * 16 * 2);
        }
    }

    public override void PostUpdateEquips()
    {
        if (Main.myPlayer == Player.whoAmI)
        {
            UpdateHovers();

            if (hovers.Any(x => x.Contains(Main.MouseWorld)))
            {
                Hoverbox hover = hovers.First(x => x.Contains(Main.MouseWorld));
                Player.cursorItemIconEnabled = true;
                Player.cursorItemIconID = ModContent.ItemType<ChatItemIcon>();
                Player.cursorItemIconText = "";
                Player.noThrow = 2;

                if (Main.mouseRight && Main.mouseRightRelease)
                    OpenDialogue(hover);
            }
        }
    }

    private void OpenDialogue(Hoverbox hover)
    {
        Main.CancelHairWindow();
        Main.SetNPCShopIndex(0);
        Main.InGuideCraftMenu = false;
        Player.dropItemCheck();
        Main.npcChatCornerItem = 0;
        Player.sign = -1;
        Main.editSign = false;
        Player.SetTalkNPC(hover.NPCWhoAmI);
        Main.playerInventory = false;
        Player.SetTalkNPC(hover.NPCWhoAmI);
        Main.npcChatText = Main.npc[hover.NPCWhoAmI].GetChat();
        SoundEngine.PlaySound(SoundID.MenuOpen);

        if (Main.netMode != NetmodeID.SinglePlayer)
            NetMessage.SendData(MessageID.SyncTalkNPC, -1, -1, null, Main.myPlayer);
    }

    private void UpdateHovers()
    {
        hovers.Clear();

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];

            if (npc.active && npc.ModNPC is IAdditionalHoverboxes hovering)
                hovers.AddRange(hovering.GetAdditionalHoverboxes());
        }
    }

    class HoverMapLayer : ModMapLayer
    {
        public static FieldInfo ContentInfo = typeof(NPCHeadRenderer).GetField("_contents", BindingFlags.Instance | BindingFlags.NonPublic);

        public override void Draw(ref MapOverlayDrawContext context, ref string text)
        {
            foreach (var item in hovers)
            {
                if (item.MapIcon is null)
                    continue;

                Vector2 pos = item.Rectangle.Center.ToVector2() / 16f;
                Texture2D tex = null;

                if (item.MapIcon is int bossHeadSlot)
                {
                    var array = ContentInfo.GetValue(Main.BossNPCHeadRenderer) as NPCHeadDrawRenderTargetContent[];
                    NPCHeadDrawRenderTargetContent content = array[bossHeadSlot];

                    if (content is null)
                    {
                        array[bossHeadSlot] = new NPCHeadDrawRenderTargetContent();
                        array[bossHeadSlot].SetTexture(TextureAssets.NpcHeadBoss[bossHeadSlot].Value);
                        content = array[bossHeadSlot];
                    }

                    if (content.IsReady)
                        tex = content.GetTarget();
                    else
                        content.Request();
                }
                else if (item.MapIcon is Asset<Texture2D> asset)
                    tex = asset.Value;

                if (tex is null)
                    return;

                if (context.Draw(tex, pos, Color.White, new SpriteFrame(1, 1, 0, 0), 1f, 1f, Alignment.Center).IsMouseOver)
                    text = Language.GetTextValue(item.MapName);
            }
        }
    }
}
