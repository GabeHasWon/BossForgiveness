using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using System.Linq;
using System;

namespace BossForgiveness.Content.NPCs.Mechanics.Skeletron;

public class SkullLamp : ModProjectile
{
    public ref float OrderNumber => ref Projectile.ai[0];
    public ref float NextLamp => ref Projectile.ai[1];

    public bool Lit
    {
        get => Projectile.ai[2] == 1;
        set => Projectile.ai[2] = value ? 1 : 0;
    }

    internal bool final = false;
    
    private bool _touched = false;
    private int _floatTime = 0;
    private bool _setDirection = true;

    public override void SetDefaults()
    {
        Projectile.Size = new(30, 64);
        Projectile.timeLeft = 2;
        Projectile.aiStyle = -1;
        Projectile.tileCollide = false;
    }

    public override void AI()
    {
        if (!NPC.AnyNPCs(NPCID.SkeletronHead))
        {
            Projectile.Kill();
            return;
        }
        
        if (_setDirection)
        {
            _setDirection = false;
            Projectile.direction = Projectile.spriteDirection = Main.rand.NextBool() ? -1 : 1;
        }

        Projectile.velocity.Y = MathF.Sin((_floatTime++ + Projectile.identity * 200) * 0.01f) * 0.8f;

        if (Lit)
        {
            if (Main.rand.NextBool(6))
                Dust.NewDust(Projectile.position, Projectile.width, 20, DustID.Torch);

            TorchID.TorchColor(TorchID.Torch, out float r, out float g, out float b);
            Lighting.AddLight(Projectile.Center, new Vector3(r, g, b) * (_touched ? 1f : 0.5f));

            if (!_touched)
            {
                foreach (var item in Main.ActivePlayers)
                {
                    if (item.Hitbox.Intersects(Projectile.Hitbox))
                    {
                        _touched = true;
                        
                        if (!final)
                            (Main.projectile.First(x => x.identity == (int)NextLamp).ModProjectile as SkullLamp).Lit = true;
                        else
                            Main.npc[(int)NextLamp].GetGlobalNPC<SkeletronPacificationNPC>().FinishLamps(Main.npc[(int)NextLamp]);
                    }
                }
            }
        }

        Projectile.timeLeft++;
    }

    public override bool PreDraw(ref Color lightColor)
    {
        Texture2D tex = TextureAssets.Projectile[Type].Value;
        Rectangle src = new((int)(Main.GameUpdateCount / 4f % 3) * 32, Lit ? (_touched ? 66 : 132) : 0, 30, 64);
        Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, src, lightColor, Projectile.rotation, src.Size() / 2f, 1f, SpriteEffects.None, 0);
        return false;
    }
}