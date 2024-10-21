using Terraria;

namespace BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;

internal interface ICustomBarNPC
{
    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax);
}
