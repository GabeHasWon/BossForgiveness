using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class EoCPacificationNPC : GlobalNPC
{
    public const int MaxDiscontent = 5;

    public override bool InstancePerEntity => true;

    public bool IsContent => _discontentness == 0;

    private float _discontentness = 5;
    private bool _wet = false;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.EyeofCthulhu;

    public override bool PreAI(NPC npc)
    {
        if (Collision.WetCollision(npc.position, npc.width, npc.height))
        {
            if (!_wet)
                _discontentness--;
            _wet = true;
        }
        else
            _wet = false;

        return true;
    }

    public override Color? GetAlpha(NPC npc, Color drawColor) => Color.Lerp(drawColor, Color.Red, _discontentness / MaxDiscontent * 0.5f);
}
