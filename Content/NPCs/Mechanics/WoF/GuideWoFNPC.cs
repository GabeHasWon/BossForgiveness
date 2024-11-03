using BossForgiveness.Content.Items.ForVanilla;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.WoF;

internal class GuideWoFNPC : GlobalNPC
{
    public override void GetChat(NPC npc, ref string chat)
    {
        if (npc.type == NPCID.Guide && !ModContent.GetInstance<GuideLocketSystem>().locketGiven && (NPC.downedBoss2 || NPC.downedBoss3))
        {
            chat = Language.GetTextValue("Mods.BossForgiveness.GuideLocketDialogue");
            Item.NewItem(npc.GetSource_GiftOrReward("GuideLocket"), npc.Hitbox, ModContent.ItemType<GuidesLocket>(), noGrabDelay: true);
            ModContent.GetInstance<GuideLocketSystem>().locketGiven = true;
        }
    }
}
