using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class KingSlimePacificationNPC : GlobalNPC
{
    private const float MinScale = 1.1f;

    public override bool InstancePerEntity => true;

    private static bool Pacifist(NPC npc) => npc.GetGlobalNPC<KingSlimePacificationNPC>()._timer >= 1 * 10 && npc.life == npc.lifeMax;

    private int _timer = 0;
    private float _scale = 0;
    private bool _wasPacifist = false;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.KingSlime;

    public override bool PreAI(NPC npc)
    {
        if (_timer == 0)
            _scale = npc.scale;

        _timer++;

        if (Pacifist(npc))
        {
            _wasPacifist = true;

            if (_scale < MinScale && npc.ai[1] != 5 && npc.ai[1] != 6)
            {
                npc.Transform(ModContent.NPCType<KingSlimePacified>());
                npc.netUpdate = true;
                npc.scale = _scale;
                return false;
            }

            if (_timer % (int)(240 * (npc.scale - 0.25f)) == 0)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    var dir = new Vector2(0, -8f).RotatedBy(Main.rand.NextFloat(-0.8f, 0.8f));
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, dir, ModContent.ProjectileType<SlimeSpikeball>(), npc.damage, 2f, Main.myPlayer);
                }

                for (int i = 0; i < 20; ++i)
                {
                    float magnitude = Main.rand.NextFloat(3, 6);
                    float dir = npc.AngleTo(Main.player[npc.target].Center);
                    Vector2 vel = new Vector2(0, magnitude).RotatedBy(dir);

                    Dust.NewDust(npc.Center, 1, 1, DustID.t_Slime, vel.X, vel.Y);
                }

                _scale -= 0.01f;
            }
            return true;
        }

        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (npc.ai[1] == 5f || npc.ai[1] == 6f)
        {
            return;
        }

        if (Pacifist(npc) || _wasPacifist)
        {
            if (!Pacifist(npc) && _wasPacifist)
                _scale = MathHelper.Lerp(_scale, npc.scale, 0.1f);

            npc.scale = _scale;
        }
    }

    public class SlimeSpikeball : ModProjectile
    {
        private bool HitTile
        {
            get => Projectile.ai[0] == 1;
            set => Projectile.ai[0] = value ? 1 : 0;
        }

        public override void SetDefaults()
        {
            Projectile.Size = new(20);
            Projectile.timeLeft = 600;
            Projectile.penetrate = 6;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            if (!HitTile)
            {
                Projectile.rotation += Projectile.velocity.X * 0.01f;
                Projectile.velocity.Y += 0.2f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 20; ++i)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.BunnySlime);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.velocity *= 0;
            HitTile = true;
            return false;
        }
    }
}
