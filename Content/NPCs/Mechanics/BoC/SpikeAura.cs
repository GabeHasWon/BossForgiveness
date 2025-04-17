using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Graphics;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;

namespace BossForgiveness.Content.NPCs.Mechanics.BoC;

public class SpikeAura : ModProjectile
{
    private bool SpawnedSpikes
    {
        get => Projectile.ai[0] == 1;
        set => Projectile.ai[0] = value ? 1 : 0;
    }

    private ref float Owner => ref Projectile.ai[1];

    private bool ScaledUpAlready
    {
        get => Projectile.ai[2] == 1;
        set => Projectile.ai[2] = value ? 1 : 0;
    }

    private ref float ScaleVelocity => ref Projectile.localAI[0];

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 3;
    }

    public override void SetDefaults()
    {
        Projectile.Size = new(96);
        Projectile.timeLeft = 600;
        Projectile.penetrate = 1;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.friendly = false;
        Projectile.tileCollide = false;
        Projectile.scale = 0f;
        Projectile.Opacity = 0.3f;
    }

    public override void AI()
    {
        Projectile.scale = MathHelper.Lerp(Projectile.scale, ScaleVelocity, 0.05f);

        if (Projectile.scale > 1f || NPC.crimsonBoss == -1)
        {
            ScaledUpAlready = true;

            if (!SpawnedSpikes)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient && NPC.crimsonBoss != -1) 
                    SpamSpikes(Projectile, Projectile.GetSource_FromAI(), (int)Owner);
                
                SpawnedSpikes = true;
            }
        }

        if (ScaledUpAlready)
        {
            ScaleVelocity = MathHelper.Lerp(ScaleVelocity, 0, 0.05f);

            if (Projectile.scale <= 0.05f)
            {
                Projectile.Kill();
            }
        }
        else
        {
            ScaleVelocity = MathHelper.Lerp(ScaleVelocity, 1.1f, 0.05f);
        }
    }

    private static void SpamSpikes(Projectile projectile, IEntitySource source, int owner)
    {
        const int SpikeDist = 6;

        int x = (int)(projectile.Center.X / 16f);
        int y = (int)(projectile.Center.Y / 16f);

        var points = new HashSet<Vector3>();

        for (int i = -SpikeDist; i < SpikeDist; ++i)
        {
            for (int j = -SpikeDist; j < SpikeDist; j++)
            {
                if (!WorldGen.SolidTile(x + i, y + j) && CanConnect(x + i, y + j, out float rot) && Main.rand.NextBool(3))
                    points.Add(new(x + i, y + j, rot));
            }
        }

        int type = ModContent.ProjectileType<BoCSpike>();

        foreach (var item in points)
        {
            var pos = new Vector2(item.X, item.Y).ToWorldCoordinates();
            int p = Projectile.NewProjectile(source, pos, Vector2.Zero, type, 20, 0f, Main.myPlayer);
            Projectile proj = Main.projectile[p];
            proj.rotation = item.Z - MathHelper.PiOver2;
            proj.frame = Main.rand.Next(3);
            proj.ai[2] = -1;
            (proj.ModProjectile as BoCSpike).Parent = owner;

            if (Main.netMode == NetmodeID.Server)
                NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, p);
        }
    }

    private static bool CanConnect(int x, int y, out float rot)
    {
        rot = 0f;

        for (int i = -1; i <= 1; ++i)
        {
            for (int j = -1; j <= 1; ++j)
            {
                if (i == 0 && j == 0)
                    continue;

                if (WorldGen.SolidTile(x + i, y + j))
                {
                    rot = new Vector2(x, y).AngleTo(new Vector2(x + i, y + j));
                    return true;
                }
            }
        }

        return false;
    }

    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i < 20; ++i)
            Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.t_Flesh);
    }

    public override bool OnTileCollide(Vector2 oldVelocity) => false;
    public override Color? GetAlpha(Color lightColor) => Color.White * Projectile.Opacity;

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Vector2 position = Projectile.Center - Main.screenPosition;
        Color color = Projectile.GetAlpha(lightColor);
        int height = tex.Height / 3;
        Rectangle baseFrame = new(0, 0, tex.Width, height);

        for (int i = 0; i < 4; ++i)
        {
            Rectangle frame = baseFrame with { Y = height * (int)(Main.timeForVisualEffects / 8f % 3) };
            Vector2 off = Main.rand.NextVector2Circular(2, 2);
            float factor = (i / 3f);
            Main.EntitySpriteDraw(tex, position + off, frame, color * factor, 0f, frame.Size() / 2f, Projectile.scale * (factor * 0.5f + 0.5f), SpriteEffects.None, 0);
        }

        return false;
    }
}