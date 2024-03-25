using Terraria;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs;

internal class SpeedUpBehaviourNPC : GlobalNPC
{
    private bool SkipSpeedBoost = false;

    public override bool InstancePerEntity => true;

    public float behaviourSpeed = 0;

    private float _accumulatedSpeed = 0;

    public override void ResetEffects(NPC npc) => behaviourSpeed = 0;

    public override void PostAI(NPC npc)
    {
        if (SkipSpeedBoost)
            return;

        _accumulatedSpeed += behaviourSpeed;

        if (_accumulatedSpeed > 1)
        {
            SkipSpeedBoost = true;

            while (_accumulatedSpeed >= 1)
            {
                _accumulatedSpeed--;
                npc.UpdateNPC(npc.whoAmI);
            }
            SkipSpeedBoost = false;
        }
    }
}
