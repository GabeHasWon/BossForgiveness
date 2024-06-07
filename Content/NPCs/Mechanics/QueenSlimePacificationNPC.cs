using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class QueenSlimePacificationNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    public Vector2 crystalOffset = Vector2.Zero;
    public Vector2 crystalPosition = Vector2.Zero;
    public bool crystalHooked = false;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.QueenSlimeBoss;

    public override void Load() => IL_Main.DrawNPCDirect_Inner += HijackQueenSlimeCoreDrawing;

    private void HijackQueenSlimeCoreDrawing(ILContext il)
    {
        ILCursor c = new(il);

        if (!c.TryGotoNext(x => x.MatchCall<Main>(nameof(Main.DrawNPCDirect_QueenSlimeWings))))
            return;

        if (!c.TryGotoNext(MoveType.Before, x => x.MatchCallvirt<EffectPass>(nameof(EffectPass.Apply))))
            return;

        c.Emit(OpCodes.Ldloca_S, (byte)54);
        c.Emit(OpCodes.Ldarg_2);
        c.EmitDelegate(ModifyCrystalPosition);
    }

    public static void ModifyCrystalPosition(ref Vector2 position, NPC self)
    {
        position += self.GetGlobalNPC<QueenSlimePacificationNPC>().crystalOffset;
        self.GetGlobalNPC<QueenSlimePacificationNPC>().crystalPosition = position + Main.screenPosition;
    }

    public override bool PreAI(NPC npc)
    {
        ref float time = ref npc.ai[0];
        Player target = Main.player[npc.target];

        if (crystalHooked)
        {
            time++;

            if (time <= 0)
                npc.damage = 0;
            else if (time is > 0 and < 60)
                npc.Center = Vector2.Lerp(npc.Center, target.Center - new Vector2(Math.Clamp(-target.velocity.X * 40, -160, 160), 400), 0.1f);
            else
            {
                if (npc.collideY || npc.velocity.Y == 0)
                {
                    time = -30;

                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Bottom, Vector2.Zero, ProjectileID.QueenSlimeSmash, 30, 4f);

                    for (int i = 0; i < 4; ++i)
                    {
                        var pos = new Vector2(npc.position.X + Main.rand.Next(Main.rand.Next(npc.width)), npc.Center.Y + npc.height / 4f);
                        Projectile.NewProjectile(npc.GetSource_FromAI(), pos, new Vector2(0, -8).RotatedByRandom(0.5f), ProjectileID.QueenSlimeGelAttack, 30, 0);
                    }
                }

                npc.damage = npc.defDamage;
                npc.velocity.Y = Main.expertMode ? 10 : 12.5f;
                npc.MaxFallSpeedMultiplier *= 2f;
            }

            return false;
        }

        return true;
    }
}