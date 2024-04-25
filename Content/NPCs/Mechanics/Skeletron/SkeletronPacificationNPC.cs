using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Skeletron;

internal class SkeletronPacificationNPC : GlobalNPC
{
    private static Asset<Texture2D> _flame = null;

    public override bool InstancePerEntity => true;

    private readonly HashSet<int> lamps = [];

    internal int lampFinishes = 0;
    
    private int lampTimer = 0;
    private int lastLamp = -1;

    public override void SetStaticDefaults() => _flame = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/Skeletron/SkeleFlame");
    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.SkeletronHead;

    public override bool PreAI(NPC npc)
    {
        if (Main.dayTime || npc.life < npc.lifeMax)
        {
            if (lamps.Count != 0)
                FinishLamps(npc);

            lampFinishes = 0;
            return true;
        }

        if (lampFinishes > 0)
        {
            TorchID.TorchColor(TorchID.Torch, out float r, out float g, out float b);
            Lighting.AddLight(npc.Center, new Vector3(r, g, b) * lampFinishes / 5f);

            if (lampFinishes > 5)
            {
                npc.playerInteraction[npc.target] = true;
                npc.NPCLoot();
                npc.Transform(ModContent.NPCType<SkeletronPacified>());
            }
        }

        lampTimer++;

        if (lampTimer is > 300 and < 365) // Place lamps
        {
            if (lampTimer % 10 == 0)
            {
                int proj = Projectile.NewProjectile(npc.GetSource_FromAI(), GetOpenLampSpace(npc), Vector2.Zero, ModContent.ProjectileType<SkullLamp>(), 0, 0, Main.myPlayer);
                lamps.Add(Main.projectile[proj].identity);

                var lamp = Main.projectile[proj].ModProjectile as SkullLamp;
                lamp.OrderNumber = lamps.Count - 1;

                if (lamps.Count == 1)
                    lamp.Lit = true;
                else
                {
                    var oldProj = Main.projectile.First(x => x.identity == lastLamp).ModProjectile as SkullLamp;
                    oldProj.NextLamp = Main.projectile[proj].identity;
                }

                lastLamp = Main.projectile[proj].identity;

                if (lampTimer == 360)
                {
                    lamp.final = true;
                    lamp.NextLamp = npc.whoAmI;
                }
            }
        }

        return true;
    }

    public override void OnHitPlayer(NPC npc, Player target, Player.HurtInfo hurtInfo)
    {
        if (lampFinishes > 0)
            target.AddBuff(BuffID.OnFire, 60 * lampFinishes);
    }

    private Vector2 GetOpenLampSpace(NPC npc)
    {
        const float Radius = 400;
        int repeats = -1;

        GetPos(npc, Radius, out Vector2 pos);

        if (lamps.Count == 0)
            return pos;

        retry:
        repeats++;
        float minDist = float.MaxValue;

        foreach (int id in lamps)
        {
            float dist = Main.projectile.First(x => x.identity == id).DistanceSQ(pos);

            if (dist < minDist)
                minDist = dist;
        }

        if (minDist < 200 * 200)
        {
            GetPos(npc, Radius, out pos); 
            goto retry;
        }

        return pos;
    }

    private static void GetPos(NPC npc, float Radius, out Vector2 pos)
    {
        pos = npc.Center + Main.rand.NextVector2Circular(Radius * 3, Radius);
        
        while (!Collision.SolidCollision(pos, 16, 16))
            pos.Y += 16;

        pos.Y -= Main.rand.Next(50, 350);

        while (Collision.SolidCollision(pos, 16, 16))
            pos.Y -= 16;
    }

    internal void FinishLamps(NPC npc)
    {
        lampFinishes++;
        lampTimer = 0;

        foreach (int id in lamps)
            Main.projectile.First(x => x.identity == id).Kill();

        lamps.Clear();
        npc.damage += 15;
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (lampFinishes <= 0)
            return true;

        var tex = _flame.Value;
        int frameX = (int)(Main.GameUpdateCount / 4f % 4);
        Rectangle frame = tex.Frame(2, 1, frameX % 2, 0, 0, 0);
        SpriteEffects effect = frameX <= 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

        drawColor *= lampFinishes / 5f;

        spriteBatch.Draw(tex, npc.Center - screenPos, frame, drawColor, npc.rotation, npc.Size / 2f + new Vector2(6, 46), 1f, effect, 0);
        return true;
    }
}
