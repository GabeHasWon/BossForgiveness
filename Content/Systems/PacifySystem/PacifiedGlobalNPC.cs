using Terraria;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem;

internal class PacifiedGlobalNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public int unhitTime = 0;

    public override bool PreAI(NPC npc)
    {
        unhitTime++;

        if (PacifiedNPCHandler.Handlers.TryGetValue(npc.type, out PacifiedNPCHandler handler) && handler.CanPacify(npc))
        {
            handler.OnPacify(npc);
            return false;
        }
        return true;
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) => unhitTime = 0;
}
