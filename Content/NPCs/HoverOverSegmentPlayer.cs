using BossForgiveness.Content.Items;
using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs;

internal class HoverOverSegmentPlayer : ModPlayer
{
    private readonly List<Hover> hovers = [];

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
        if (player.TalkNPC is not null && player.TalkNPC.ModNPC is INeedsHovering)
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
                Hover hover = hovers.First(x => x.Contains(Main.MouseWorld));
                Player.cursorItemIconEnabled = true;
                Player.cursorItemIconID = ModContent.ItemType<ChatItemIcon>();
                Player.cursorItemIconText = "";
                Player.noThrow = 2;

                if (Main.mouseRight && Main.mouseRightRelease)
                    OpenDialogue(hover);
            }
        }
    }

    private void OpenDialogue(Hover hover)
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
    }

    private void UpdateHovers()
    {
        hovers.Clear();

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];

            if (npc.active && npc.ModNPC is INeedsHovering hovering)
                hovers.AddRange(hovering.Hovers());
        }
    }
}
