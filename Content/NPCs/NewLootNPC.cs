using BossForgiveness.Content.Items.ForVanilla;
using BossForgiveness.Content.Systems.PacifySystem;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs;

internal class NewLootNPC : GlobalNPC
{
    public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
    {
        if (npc.type == NPCID.EyeofCthulhu)
        {
            LeadingConditionRule rule = new(new PacifyingCondition());
            rule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EoCLeash>()));
            npcLoot.Add(rule);
        }
    }

    public class PacifyingCondition : IItemDropRuleCondition
    {
        public bool CanDrop(DropAttemptInfo info) => PacifiedNPCHandler.TransformingNPC;
        public bool CanShowItemDropInUI() => true;
        public string GetConditionDescription() => Language.GetTextValue("Mods.BossForgiveness.PacifiedCondition");
    }
}
