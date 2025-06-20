﻿using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.BoC;

internal class BoCPacificationNPC : GlobalNPC, ICustomBarNPC
{
    public const int MaxSleepy = 60;

    private static Asset<Texture2D> zzzTex;

    public override bool InstancePerEntity => true;

    public int sleepyness = 0;
    public int sleepyTime = 0;
    public bool anyCreepersHurt = false;

    private int _spikeAttackTimer = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.BrainofCthulhu;
    public override void SetStaticDefaults() => zzzTex = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/BoC/Zzzz");
    public override void Unload() => zzzTex = null;

    public override bool PreAI(NPC npc)
    {
        npc.position -= npc.velocity * Math.Max(sleepyness / (float)MaxSleepy, 0);

        if (anyCreepersHurt || npc.life < npc.lifeMax)
        {
            if (sleepyTime > 0)
                sleepyness--;

            return true;
        }

        if (sleepyness >= MaxSleepy)
        {
            if (Collision.SolidCollision(npc.position, npc.width, npc.height))
                return true;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!NPC.AnyNPCs(ModContent.NPCType<BoCPacified>()))
                {
                    npc.Pacify<BoCPacified>();
                    (npc.ModNPC as BoCPacified).BuildCreepers();
                    npc.netUpdate = true;
                }
                else
                {
                    if (Main.netMode != NetmodeID.Server)
                    {
                        for (int i = 0; i < 20; ++i)
                        {
                            var pos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                            Gore.NewGore(npc.GetSource_Death(), pos, Vector2.Zero, GoreID.Smoke1 + Main.rand.Next(3));
                        }
                    }

                    ClearCreepers();
                }
            }

            return false;
        }

        if (_spikeAttackTimer++ > 2 * 60 && !TooManySpikes())
        {
            _spikeAttackTimer = 0;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 pos = FindSpikePositions(npc, out bool failed);

                if (!failed)
                    Projectile.NewProjectile(npc.GetSource_FromAI(), pos, Vector2.Zero, ModContent.ProjectileType<SpikeAura>(), 0, 0, Main.myPlayer, 0, npc.whoAmI);
            }
        }

        CheckCreepersHurt(npc);
        return true;
    }

    private static bool TooManySpikes()
    {
        int count = 0;

        foreach (Projectile projectile in Main.ActiveProjectiles)
        {
            if (projectile.ModProjectile is BoCSpike)
                count++;
        }

        return count > 50;
    }

    private static Vector2 FindSpikePositions(NPC npc, out bool failed)
    {
        const int Range = 400;

        int attempts = 0;

        while (true)
        {
            attempts++;

            if (attempts > 30000)
            {
                failed = true;
                return Vector2.Zero;
            }

            Vector2 pos = Main.player[npc.target].Center + new Vector2(Main.rand.NextFloat(-Range, Range), Main.rand.NextFloat(-Range, Range));

            if (Collision.SolidCollision(pos, 120, 120) && ReverseSolidCollision(pos, 120, 120))
            {
                failed = false;
                return pos;
            }
        }
    }

    public static bool ReverseSolidCollision(Vector2 Position, int Width, int Height)
    {
        int value = (int)(Position.X / 16f) - 1;
        int value2 = (int)((Position.X + (float)Width) / 16f) + 2;
        int value3 = (int)(Position.Y / 16f) - 1;
        int value4 = (int)((Position.Y + (float)Height) / 16f) + 2;
        int num = Utils.Clamp(value, 0, Main.maxTilesX - 1);

        value2 = Utils.Clamp(value2, 0, Main.maxTilesX - 1);
        value3 = Utils.Clamp(value3, 0, Main.maxTilesY - 1);
        value4 = Utils.Clamp(value4, 0, Main.maxTilesY - 1);

        for (int i = num; i < value2; i++)
        {
            for (int j = value3; j < value4; j++)
            {
                Tile tile = Main.tile[i, j];

                if (!WorldGen.SolidTile(tile))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((byte)sleepyness);
        binaryWriter.Write(sleepyTime);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        sleepyness = binaryReader.ReadByte();
        sleepyTime = binaryReader.ReadInt32();
    }

    private static void ClearCreepers()
    {
        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];

            if (npc.CanBeChasedBy() && npc.type == NPCID.Creeper)
            {
                npc.active = false;
                npc.netUpdate = true;
                npc.SetAllPlayerInteraction();
                npc.NPCLoot();

                for (int j = 0; j < 3; ++j)
                {
                    var pos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
                    Gore.NewGore(npc.GetSource_Death(), pos, Vector2.Zero, GoreID.Smoke1 + Main.rand.Next(3));
                }
            }
        }
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

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        float alpha = sleepyness / (float)MaxSleepy * npc.Opacity;
        spriteBatch.Draw(zzzTex.Value, npc.Center - screenPos, null, drawColor * alpha, 0f, zzzTex.Size() / new Vector2(2f, 1f), 1f, SpriteEffects.None, 0);
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = sleepyness;
        barMax = MaxSleepy;
        return true;
    }
}
