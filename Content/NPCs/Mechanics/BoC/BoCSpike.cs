using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace BossForgiveness.Content.NPCs.Mechanics.BoC;

public class BoCSpike : ModProjectile
{
    public ref float Parent => ref Projectile.ai[0];
    public ref float Time => ref Projectile.ai[1];
    public ref float ConnectedNPC => ref Projectile.ai[2];

    private ref float _spikeIn => ref Projectile.localAI[0];

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

        if (Main.CurrentFrameFlags.AnyActiveBossNPC)
            Projectile.timeLeft++;

        Lighting.AddLight(Projectile.Center, new Vector3(0.3f + MathF.Pow(MathF.Sin(Time++ * 0.03f + Projectile.whoAmI), 2) * 0.3f, 0.1f, 0.1f));
        NPC parent = Main.npc[(int)Parent];

        if (!parent.active || parent.type != NPCID.BrainofCthulhu || parent.life < parent.lifeMax)
        {
            Projectile.Kill();
            Projectile.netUpdate = true;
        }

        if (Main.netMode != NetmodeID.MultiplayerClient)
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
                Projectile.netUpdate = true;

                if (Main.netMode == NetmodeID.SinglePlayer)
                    npc.GetGlobalNPC<CreeperPacificationNPC>().rage++;
                else if (Main.netMode == NetmodeID.Server)
                    new SyncSpikedCreeperModule(npc.whoAmI).Send();

                break;
            }

            if (ConnectedNPC > -1 && npc.type == NPCID.BrainofCthulhu)
            {
                Projectile.Kill();
                Projectile.netUpdate = true;

                if (Main.netMode == NetmodeID.SinglePlayer)
                    npc.GetGlobalNPC<BoCPacificationNPC>().sleepyness++;
                else if (Main.netMode == NetmodeID.Server)
                    new SyncSleepyBoCModule((int)Parent).Send();

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

    public override void SendExtraAI(BinaryWriter writer)
    {
        writer.Write(Projectile.rotation);
        writer.WriteVector2(_offset);
    }

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        Projectile.rotation = reader.ReadSingle();
        _offset = reader.ReadVector2();
    }

    public override bool PreDraw(ref Color lightColor)
    {
        float factor = 1f;

        if (_spikeIn++ < 60)
            factor = (_spikeIn / 60f);

        Texture2D tex = TextureAssets.Projectile[Type].Value;
        int height = tex.Height / Main.projFrames[Type];
        var src = new Rectangle(0, (int)(height * factor), tex.Width, (int)(height * factor));
        Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, src, lightColor, Projectile.rotation, src.Size() / new Vector2(2f, 5f), 1f, SpriteEffects.None, 0);

        return false;
    }
}