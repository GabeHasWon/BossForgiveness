using BossForgiveness.Content.Items.ForVanilla;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Misc;

public class TruffleLeechNPC : ModNPC
{
    private Projectile SharknadoParent => Main.projectile[(int)SharknadoWho];
    private Player PlayerParent => Main.player[(int)PlayerWho];

    private ref float SharknadoWho => ref NPC.ai[0];
    private ref float PlayerWho => ref NPC.ai[1];

    private Vector2 PlayerOffset
    {
        get => new(NPC.ai[2], NPC.ai[3]);

        set
        {
            NPC.ai[2] = value.X;
            NPC.ai[3] = value.Y;
        }
    }

    public override void SetStaticDefaults() => Main.npcCatchable[NPC.type] = true;

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.damage = 1;
        NPC.Size = new Vector2(10);
        NPC.lifeMax = 20;
        NPC.defense = 5;
        NPC.noGravity = true;
        NPC.dontCountMe = true;
        NPC.catchItem = (short)ModContent.ItemType<TruffleLeech>();
        NPC.friendly = false;
        NPC.dontTakeDamageFromHostiles = true;
    }

    public override bool CanHitPlayer(Player target, ref int cooldownSlot) => PlayerWho == -1;

    public override void AI()
    {
        if (PlayerWho == -1 && SharknadoWho == -1)
        {
            NPC.Opacity *= 0.9f;

            if (NPC.Opacity < 0.05f)
                NPC.active = false;
            return;
        }

        if (PlayerWho != -1)
        {
            if (!PlayerParent.active || PlayerParent.dead)
            {
                PlayerWho = -1;
                return;
            }

            PlayerParent.AddBuff(BuffID.Bleeding, 2);
            PlayerParent.AddBuff(BuffID.Poisoned, 2);
            NPC.Center = PlayerParent.Center + PlayerOffset;
            return;
        }

        if (SharknadoWho == -1 || !SharknadoParent.active || SharknadoParent.type != ProjectileID.Sharknado)
        {
            SharknadoWho = -1;
            NPC.noGravity = false;
            return;
        }

        float offset = MathF.Sin(SharknadoParent.timeLeft * 0.15f) * SharknadoParent.width * SharknadoParent.scale * 1.25f;
        NPC.position.X = SharknadoParent.position.X + offset + SharknadoParent.width / 2f;
        NPC.velocity.X = -MathF.Cos(SharknadoParent.timeLeft * 0.15f) * 5;
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo hurtInfo)
    {
        SharknadoWho = -1;
        PlayerWho = target.whoAmI;
        PlayerOffset = NPC.Center - target.Center;
    }
}
