using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;
using Terraria.ID;
using ReLogic.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BossForgiveness.Content.NPCs.Mechanics.Plantera;

public class RoseProjectile : ModProjectile
{
    private static Asset<Texture2D> _aura = null;

    private bool IsLanded
    {
        get => Projectile.ai[0] == 1f;
        set => Projectile.ai[0] = value ? 1f : 0f;
    }

    private bool Initialized
    {
        get => Projectile.ai[1] == 1f;
        set => Projectile.ai[1] = value ? 1f : 0f;
    }

    private ref float Timer => ref Projectile.ai[2];
    private ref float TargetId => ref Projectile.localAI[0];
    private ref float AuraFadein => ref Projectile.localAI[1];

    private NPC Target => Main.npc[(int)TargetId];

    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 3;

        _aura = ModContent.Request<Texture2D>(Texture + "_Aura");
    }

    public override void SetDefaults()
    {
        Projectile.Size = new(16);
        Projectile.timeLeft = 600;
        Projectile.penetrate = -1;
        Projectile.aiStyle = -1;
        Projectile.hostile = true;
        Projectile.friendly = false;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, new Vector3(0.9f, 0.1f, 0.1f));

        if (!Initialized)
        {
            Initialized = true;
            TargetId = NPC.FindFirstNPC(NPCID.Plantera);
        }

        if (TargetId == -1 || !Target.active || Target.type != NPCID.Plantera)
        {
            Projectile.Kill();
            return;
        }

        if (!IsLanded)
        {
            Projectile.velocity.Y += 0.01f;
            Projectile.timeLeft++;
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
        }
        else
        {
            CheckPlantTouch.CheckTouch(Projectile, Target);

            Timer++;

            Projectile.frame = (int)MathHelper.Clamp(Timer / 8, 0, 2);

            foreach (Player player in Main.ActivePlayers)
            {
                if (player.DistanceSQ(Projectile.Center) < 200 * 200)
                    player.velocity += player.DirectionTo(Projectile.Center) * 0.2f;
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Projectile.velocity *= 0;
        return !(IsLanded = true);
    }

    public override bool PreDraw(ref Color lightColor)
    {
        if (IsLanded)
        {
            AuraFadein = MathHelper.Lerp(AuraFadein, 1, 0.05f);

            Vector2 position = Projectile.Center - Main.screenPosition;
            Color color = Color.White * AuraFadein * 0.5f;
            Main.spriteBatch.Draw(_aura.Value, position, null, color, Timer * 0.02f, _aura.Size() / 2f, 2f, SpriteEffects.None, 0);
        }

        return true;
    }
}