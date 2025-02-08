using BossForgiveness.Content.NPCs.Vanilla.Enemies;
using BossForgiveness.Content.Tiles.Vanilla;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Enemies;

internal class MimicPacificationNPC : GlobalNPC
{
    private const int MaxConvince = 10;

    public override bool InstancePerEntity => true;

    private Point16? chestLoc = null;
    private bool _hasSeenPlayer = false;
    private int _convincing = 0;
    private bool _mimicOpen = true;
    private bool _goingForChest = false;
    private float _fakeFrameCounter = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.Mimic;

    public override bool PreAI(NPC npc)
    {
        MimicPacificationNPC mimic = npc.GetGlobalNPC<MimicPacificationNPC>();

        if (mimic._hasSeenPlayer)
        {
            npc.ai[0] = 1;
            return true;
        }

        if (_convincing > MaxConvince)
        {
            float type = npc.ai[3];
            npc.Pacify<MimicPacified>();
            npc.ai[3] = type;
            return false;
        }

        Player player = Main.player[npc.target];

        if (Collision.CanHit(npc, player) && player.velocity.LengthSquared() > 3 * 3)
        {
            mimic._hasSeenPlayer = true;
            return true;
        }

        if (HasMimicChestNearby(npc, out Point16 chestLoc))
        {
            if (Vector2.DistanceSquared(npc.Center, chestLoc.ToWorldCoordinates()) > 30 * 30)
                HopTowardsChest(npc, chestLoc.ToWorldCoordinates());
            else
            {
                npc.velocity.X *= 0.8f;

                int chest = Chest.FindChest(chestLoc.X, chestLoc.Y);

                if (chest != -1)
                {
                    int chestFrame = Main.chest[chest].frame;

                    if (chestFrame == 0 && mimic._mimicOpen)
                    {
                        mimic._convincing++;
                        mimic._mimicOpen = false;
                    }
                    else if (chestFrame == 2 && !mimic._mimicOpen)
                    {
                        mimic._convincing++;
                        mimic._mimicOpen = true;
                    }
                }
            }

            return false;
        }

        return true;
    }

    public override void PostAI(NPC npc)
    {
        if (!npc.GetGlobalNPC<MimicPacificationNPC>()._hasSeenPlayer)
        {
            npc.ai[0] = 0;

            if (!npc.GetGlobalNPC<MimicPacificationNPC>()._goingForChest)
            {
                npc.velocity.X = 0;
                npc.velocity.Y = MathHelper.Max(npc.velocity.Y, 0);
            }
        }
    }

    public override void FindFrame(NPC npc, int frameHeight)
    {
        // Vanilla animation can't run due to the AI suppression, manually redo it
        if (npc.GetGlobalNPC<MimicPacificationNPC>()._goingForChest)
        {
            int frameSpeed = 3;
            ref float frameCounter = ref npc.GetGlobalNPC<MimicPacificationNPC>()._fakeFrameCounter;

            if (npc.velocity.Y == 0f)
                frameCounter--;
            else
                frameCounter++;

            if (frameCounter < 0.0)
                frameCounter = 0;

            if (frameCounter > frameSpeed * 4)
                frameCounter = frameSpeed * 4;

            if (frameCounter < frameSpeed)
                npc.frame.Y = frameHeight;
            else if (frameCounter < frameSpeed * 2)
                npc.frame.Y = frameHeight * 2;
            else if (frameCounter < frameSpeed * 3)
                npc.frame.Y = frameHeight * 3;
            else if (frameCounter < frameSpeed * 4)
                npc.frame.Y = frameHeight * 4;
            else if (frameCounter < frameSpeed * 5)
                npc.frame.Y = frameHeight * 5;
        }
    }

    public override void HitEffect(NPC npc, NPC.HitInfo hit) => npc.GetGlobalNPC<MimicPacificationNPC>()._hasSeenPlayer = true;

    private static void HopTowardsChest(NPC npc, Vector2 chestLoc)
    {
        if (npc.collideY)
        {
            npc.ai[2]++;

            if (npc.ai[2] >= 20)
            {
                npc.GetGlobalNPC<MimicPacificationNPC>()._goingForChest = true;

                if (npc.Center.X < chestLoc.X)
                    npc.velocity.X = 2;
                else
                    npc.velocity.X = -2;

                npc.velocity.Y = -3.5f;
                npc.ai[2] = 0;

                npc.direction = Math.Sign(npc.velocity.X);
                npc.spriteDirection = npc.direction;
            }
            else
                npc.velocity.X *= 0.9f;
        }
    }

    private static bool HasMimicChestNearby(NPC npc, out Point16 chestLoc)
    {
        if (npc.GetGlobalNPC<MimicPacificationNPC>().chestLoc is not null)
        {
            chestLoc = npc.GetGlobalNPC<MimicPacificationNPC>().chestLoc.Value;
            return true;
        }

        const int ScanDistance = 60;

        Point16 center = npc.Center.ToTileCoordinates16();

        for (int i = center.X - ScanDistance; i < center.X + ScanDistance; ++i)
        {
            for (int j = center.Y - ScanDistance; j < center.Y + ScanDistance; ++j)
            {
                Tile tile = Main.tile[i, j];

                if (tile.HasTile && tile.TileType == ModContent.TileType<MimicChest>())
                {
                    chestLoc = new(i, j);
                    return true;
                }
            }
        }

        chestLoc = Point16.Zero;
        npc.GetGlobalNPC<MimicPacificationNPC>().chestLoc = null;
        return false;
    }
}
