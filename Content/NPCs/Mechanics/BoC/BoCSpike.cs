using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using SteelSeries.GameSense;

namespace BossForgiveness.Content.NPCs.Mechanics.BoC;

public class BoCSpike : ModProjectile
{
    public ref float Parent => ref Projectile.ai[0];
    public ref float Time => ref Projectile.ai[1];
    public ref float ConnectedNPC => ref Projectile.ai[2];

    private Vector2 _offset = Vector2.Zero;

    public override void SetStaticDefaults() => Main.projFrames[Type] = 3;

    public override void SetDefaults()
    {
        Projectile.Size = new(20);
        Projectile.timeLeft = 600;
        Projectile.penetrate = 1;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        Time++;
        Projectile.timeLeft++;

        Lighting.AddLight(Projectile.Center, new Vector3(0.2f + MathF.Pow(MathF.Sin(Time++ * 0.03f + Projectile.whoAmI), 2) * 0.3f, 0, 0));
        NPC parent = Main.npc[(int)Parent];

        if (!parent.active || parent.type != NPCID.BrainofCthulhu || parent.life < parent.lifeMax)
            Projectile.Kill();

        CollideWithNPCs();

        if (ConnectedNPC != -1)
        {
            Projectile.Center = Main.npc[(int)ConnectedNPC].Center - _offset;

            if (!parent.active)
                ConnectedNPC = -1;
        }
    }

    private void CollideWithNPCs()
    {
        foreach (var npc in Main.ActiveNPCs)
        {
            if (!npc.Hitbox.Intersects(Projectile.Hitbox)) 
                continue;

            if (ConnectedNPC == -1 && npc.type == NPCID.Creeper)
            {
                ConnectedNPC = npc.whoAmI;
                _offset = npc.Center - Projectile.Center;

                npc.GetGlobalNPC<CreeperPacificationNPC>().rage++;
            }

            if (ConnectedNPC > -1 && npc.type == NPCID.BrainofCthulhu)
            {
                Projectile.Kill();

                npc.GetGlobalNPC<BoCPacificationNPC>().sleepyness++;
                break;
            }
        }
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 20; ++i)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Flesh);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(BuffID.Slimed, 120);
}