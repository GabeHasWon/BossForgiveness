using NetEasy;
using System;
using Terraria;
using Terraria.ID;
using static BossForgiveness.Content.Items.ForVanilla.EoCLeash;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncEoCLassoModule(int whoAmI, int eocWhoAmI) : Module
{
    private readonly int _whoAmI = whoAmI;
    private readonly int _eocWhoAmI = eocWhoAmI;

    protected override void Receive()
    {
        NPC npc = Main.npc[_eocWhoAmI];
        npc.ai[1] = 1;
        npc.ai[2] = _whoAmI;

        Player owner = Main.player[_whoAmI];
        owner.GetModPlayer<EoCLassoPlayer>().ridingEoC = _eocWhoAmI;
        owner.GetModPlayer<EoCLassoPlayer>().eoCVelocity = npc.velocity;

        if (owner.mount.Active)
            owner.QuickMount();

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
