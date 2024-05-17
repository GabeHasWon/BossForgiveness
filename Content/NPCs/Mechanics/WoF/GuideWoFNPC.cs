using BossForgiveness.Content.Items.ForVanilla;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.WoF;

internal class GuideWoFNPC : GlobalNPC
{
    public override void GetChat(NPC npc, ref string chat)
    {
        if (!ModContent.GetInstance<GuideLocketSystem>().locketGiven && NPC.downedBoss2)
        {
            chat = Language.GetTextValue("Mods.BossForgiveness.GuideLocketDialogue");
            Item.NewItem(npc.GetSource_GiftOrReward("GuideLocket"), npc.Hitbox, ModContent.ItemType<GuidesLocket>(), noGrabDelay: true);
            ModContent.GetInstance<GuideLocketSystem>().locketGiven = true;
        }
    }
}
