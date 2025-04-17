using BossForgiveness.Content.Items.ForVanilla;
using BossForgiveness.Content.NPCs.Misc;
using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class DukePacificationNPC : GlobalNPC, ICustomBarNPC
{
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.DukeFishron;

    public override bool PreAI(NPC npc)
    {
        if (npc.life < npc.lifeMax)
            return true;

        int omnileech = NPC.FindFirstNPC(ModContent.NPCType<Omnileech>());

        if (omnileech > -1)
        {
            npc.localAI[3]++;
            npc.velocity = npc.DirectionTo(Main.npc[omnileech].Center) * npc.localAI[3] * 0.2f;
            npc.rotation = npc.velocity.ToRotation();

            if (npc.velocity.X < 0)
            {
                npc.spriteDirection = 1;
                npc.rotation -= MathHelper.Pi;
            }

            return false;
        }
        else
            npc.localAI[3] = 2;

        return true;
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = 0;
        barMax = 1;

        CustomBarEdit.OverrideText = Language.GetText("Mods.BossForgiveness.BarLines.Duke").Format(ModContent.ItemType<OmnileechItem>());
        return npc.life == npc.lifeMax;
    }

    public override void OnHitNPC(NPC npc, NPC target, NPC.HitInfo hit)
    {
        if (target.type == ModContent.NPCType<Omnileech>())
            npc.Pacify<DukePacified>();
    }
}
