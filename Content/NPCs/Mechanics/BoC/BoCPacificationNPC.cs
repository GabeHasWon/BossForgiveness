﻿using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.BoC;

internal class BoCPacificationNPC : GlobalNPC
{
    public const int MaxSleepy = 40;

    private static Asset<Texture2D> zzzTex;

    public override bool InstancePerEntity => true;

    public int sleepyness = 0;
    public int sleepyTime = 0;
    public bool anyCreepersHurt = false;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.BrainofCthulhu;
    public override void SetStaticDefaults() => zzzTex = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/BoC/Zzzz");
    public override void OnSpawn(NPC npc, IEntitySource source) => SpamSpikes(npc, source);
    public override void Unload() => zzzTex = null;

    public override bool PreAI(NPC npc)
    {
        npc.position -= npc.velocity * Math.Max(sleepyness / (float)MaxSleepy, 0);

        if (anyCreepersHurt)
        {
            if (sleepyTime++ > 60)
                sleepyness--;

            return true;
        }

        if (sleepyness >= MaxSleepy)
        {
            if (npc.noTileCollide && Collision.SolidCollision(npc.position, npc.width, npc.height))
                return true;

            npc.noTileCollide = true;
            sleepyTime++;

            if (sleepyTime > 120)
            {
                npc.Pacify<BoCPacified>();
                (npc.ModNPC as BoCPacified).BuildCreepers();
            }

            return false;
        }

        CheckCreepersHurt(npc);
        return true;
    }

    private void CheckCreepersHurt(NPC npc)
    {
        for (int i = npc.whoAmI; i < Main.maxNPCs; ++i)
        {
            NPC n = Main.npc[i];

            if (i == npc.whoAmI)
                continue;

            if (n.active && n.type == NPCID.Creeper && n.life < n.lifeMax)
            {
                anyCreepersHurt = true;
                return;
            }
        }
    }

    private static void SpamSpikes(NPC npc, IEntitySource source)
    {
        int x = (int)(npc.Center.X / 16f);
        int y = (int)(npc.Center.Y / 16f);

        var points = new HashSet<Vector3>();

        for (int i = -100; i < 100; ++i)
        {
            for (int j = -100; j < 100; j++)
            {
                if (Collision.CanHit(npc.Center, npc.width, npc.height, new Vector2(x + i, y + j).ToWorldCoordinates(0, 0), 16, 16) && CanConnect(x + i, y + j, out float rot))
                    points.Add(new(x + i, y + j, rot));
            }
        }

        foreach (var item in points)
        {
            if (Main.rand.NextBool(4))
            {
                int type = ModContent.ProjectileType<BoCSpike>();
                var pos = new Vector2(item.X, item.Y).ToWorldCoordinates();
                Projectile proj = Main.projectile[Projectile.NewProjectile(source, pos, Vector2.Zero, type, 20, 0f, Main.myPlayer)];
                proj.rotation = item.Z - MathHelper.PiOver2;
                proj.frame = Main.rand.Next(3);
                proj.ai[2] = -1;
                (proj.ModProjectile as BoCSpike).Parent = npc.whoAmI;
            }
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

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float alpha = sleepyness / (float)MaxSleepy * npc.Opacity;
        spriteBatch.Draw(zzzTex.Value, npc.Center - screenPos, null, drawColor * alpha, 0f, zzzTex.Size() / new Vector2(2f, 1f), 1f, SpriteEffects.None, 0);
    }
}
