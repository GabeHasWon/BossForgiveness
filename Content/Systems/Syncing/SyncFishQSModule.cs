using BossForgiveness.Content.Items.ForVanilla;
using BossForgiveness.Content.NPCs.Mechanics;
using Microsoft.Xna.Framework;
using NetEasy;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace BossForgiveness.Content.Systems.Syncing;

[Serializable]
public class SyncFishQSModule(int projIdentity, int qsWhoAmI) : Module
{
    private readonly int _projIdentity = projIdentity;
    private readonly int _qsWhoAmI = qsWhoAmI;

    protected override void Receive()
    {
        Projectile proj = Main.projectile.FirstOrDefault(x => x.identity == _projIdentity);

        if (proj is not null)
        {
            (proj.ModProjectile as CrystalFisher.CrystalBobber).ConnectedQS = _qsWhoAmI;
            Main.npc[_qsWhoAmI].GetGlobalNPC<QueenSlimePacificationNPC>().crystalHooked = true;
            Main.npc[_qsWhoAmI].GetGlobalNPC<QueenSlimePacificationNPC>().crystalOffset = Vector2.Zero;
        }

        if (Main.netMode == NetmodeID.Server)
            Send(-1, -1, false);
    }
}
