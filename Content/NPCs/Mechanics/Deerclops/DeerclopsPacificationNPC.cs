using BossForgiveness.Content.NPCs.Vanilla;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Deerclops;

internal class DeerclopsPacificationNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private int _satisfaction = 0;
    private int _rageTime = 0;
    private bool _raging = false;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Deerclops;

    public override void AI(NPC npc)
    {
        if (npc.life < npc.lifeMax)
            return;

        if (_raging)
        {
            _rageTime--;
            npc.localAI[3] = 30;

            if (_rageTime <= 0)
                _raging = false;
        }
        else
        {
            BreakBreakableTiles(npc);
        }

        if (_satisfaction > 200)
            npc.Pacify<PacifiedDeerclops>();
    }

    private void BreakBreakableTiles(NPC npc)
    {
        Rectangle hit = npc.Hitbox;
        hit.Inflate(6, 6);

        int minX = (int)(hit.X / 16f) - 1;
        int maxX = (int)((hit.X + (float)hit.Width) / 16f) + 2;
        int minY = (int)(hit.Y / 16f) - 1;
        int maxY = (int)((hit.Y + (float)hit.Height) / 16f) + 2;
        int realX = Utils.Clamp(minX, 0, Main.maxTilesX - 1);
        maxX = Utils.Clamp(maxX, 0, Main.maxTilesX - 1);
        minY = Utils.Clamp(minY, 0, Main.maxTilesY - 1);
        maxY = Utils.Clamp(maxY, 0, Main.maxTilesY - 1);
        Vector2 vector = default;
        HashSet<Point> loc = [];

        for (int i = realX; i < maxX; i++)
        {
            for (int j = minY; j < maxY; j++)
            {
                if (!Main.tile[i, j].IsActuated && Main.tile[i, j].HasTile)
                {
                    vector.X = i * 16;
                    vector.Y = j * 16;
                    int num2 = 16;

                    if (Main.tile[i, j].IsHalfBlock)
                    {
                        vector.Y += 8f;
                        num2 -= 8;
                    }

                    if (hit.X + hit.Width > vector.X && hit.X < vector.X + 16f && hit.Y + hit.Height > vector.Y && hit.Y < vector.Y + num2 && ValidTile(i, j))
                        loc.Add(new(i, j));
                }
            }
        }

        foreach (var item in loc)
        {
            int type = Main.tile[item].TileType;
            int mod = type switch
            {
                TileID.HayBlock => 1,
                _ => 10
            };

            if (Main.tile[item].HasTile)
            {
                if (!_raging)
                {
                    _satisfaction += mod;
                    _rageTime += mod;
                }

                WorldGen.KillTile(item.X, item.Y, false, false, false);
            }

            var vel = npc.velocity.RotatedByRandom(0.2f) * Main.rand.NextFloat(1.8f, 3f) - new Vector2(0, 3);

            if (type is TileID.HayBlock or TileID.TargetDummy)
                Projectile.NewProjectile(npc.GetSource_FromAI(), item.ToWorldCoordinates(), vel, ModContent.ProjectileType<HayNeedle>(), 20, 0, Main.myPlayer);
            else
                Projectile.NewProjectile(npc.GetSource_FromAI(), item.ToWorldCoordinates(), vel, ModContent.ProjectileType<Splinter>(), 40, 0, Main.myPlayer);

            if (_rageTime > 20)
            {
                _raging = true;
                _rageTime = 480;
            }
        }
    }

    private static bool ValidTile(int i, int j) => Main.tile[i, j].TileType is TileID.HayBlock or TileID.TargetDummy or TileID.DisplayDoll;
}
