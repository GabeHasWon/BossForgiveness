using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.Graphics;
using Terraria.ID;
using Terraria.ModLoader;
using System.Runtime.CompilerServices;
using Terraria.ModLoader.IO;
using System.IO;
using BossForgiveness.Content.Items.ForVanilla;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Nebula;

internal class NebulaLinkNPC : GlobalNPC
{
    public override bool InstancePerEntity => true;

    private Player Link => Main.player[linkedTo.Value];

    public int? linkedTo = null;
    public bool invalid = false;

    /// <summary>
    /// For the Pillar.
    /// </summary>
    public int heldItem = -1;

    private int _timer = 0;

    /// <summary>
    /// For Brain Sucklers.
    /// </summary>
    private int _throwOffTimer = 0;

    /// <inheritdoc cref="_throwOffTimer"/>
    private int _throwOffDir = -1;

    /// <inheritdoc cref="_throwOffTimer"/>
    private int _thrownOffTimer = -1;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type is NPCID.NebulaBeast or NPCID.NebulaBrain or NPCID.NebulaHeadcrab 
        or NPCID.NebulaSoldier or NPCID.LunarTowerNebula;

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter)
    {
        binaryWriter.Write((short)(!linkedTo.HasValue ? -1 : linkedTo));
        bitWriter.WriteBit(invalid);

        if (npc.type == NPCID.NebulaHeadcrab)
        {
            binaryWriter.Write((short)_throwOffDir);
            binaryWriter.Write((short)_throwOffTimer);
            binaryWriter.Write((short)_thrownOffTimer);
        }

        if (npc.type == NPCID.LunarTowerNebula)
            binaryWriter.Write((short)heldItem);
    }

    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader)
    {
        short link = binaryReader.ReadInt16();

        if (link == -1)
            linkedTo = null;
        else
            linkedTo = link;

        invalid = bitReader.ReadBit();

        if (npc.type == NPCID.NebulaHeadcrab)
        {
            _throwOffDir = binaryReader.ReadInt16();
            _throwOffTimer = binaryReader.ReadInt16();
            _thrownOffTimer = binaryReader.ReadInt16();
        }

        if (npc.type == NPCID.LunarTowerNebula)
            heldItem = binaryReader.ReadInt16();
    }

    public override bool PreAI(NPC npc)
    {
        _timer++;

        if (npc.type == NPCID.NebulaHeadcrab)
        {
            if (_thrownOffTimer > 0)
            {
                _thrownOffTimer--;

                if (invalid)
                    return false;

                UpdateLinking(npc);

                return false;
            }

            if (npc.ai[0] == 5f)
            {
                Player target = Main.player[npc.target];

                if (target.controlLeft && _throwOffDir != -1)
                {
                    _throwOffTimer++;
                    _throwOffDir = -1;
                }

                if (target.controlRight && _throwOffDir != 1)
                {
                    _throwOffTimer++;
                    _throwOffDir = 1;
                }

                if (_throwOffTimer >= 6)
                {
                    npc.ai[0] = 0f;
                    npc.ai[1] = 0f;
                    npc.velocity = Vector2.Normalize(target.velocity) * -8;
                    _throwOffTimer = 0;
                    _thrownOffTimer = 30;
                }
            }
        }
        else if (npc.type == NPCID.LunarTowerNebula)
        {
            foreach (NPC other in Main.ActiveNPCs)
            {
                if (other.type is NPCID.NebulaBeast or NPCID.NebulaBrain or NPCID.NebulaHeadcrab or NPCID.NebulaSoldier)
                {
                    if (other.life < other.lifeMax * 0.3f)
                    {
                        invalid = true;
                    }
                    else if (other.life < other.lifeMax)
                    {
                        other.life++;
                    }
                }
            }

            if (invalid)
            {
                if (heldItem != -1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Main.item[heldItem].active = false;

                    if (Main.netMode == NetmodeID.Server)
                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, heldItem);

                    heldItem = -1;
                }

                return true;
            }

            if (heldItem == -1)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    heldItem = Item.NewItem(npc.GetSource_FromAI(), new Vector2(npc.Center.X, npc.Center.Y - 500), ModContent.ItemType<Telelink>());
                    npc.netUpdate = true;
                }
            }
            else
            {
                Item item = Main.item[heldItem];

                if (!item.active || item.type != ModContent.ItemType<Telelink>())
                    heldItem = -1;
                else
                {
                    item.velocity = Vector2.Zero;
                    item.position.Y = npc.Center.Y - 500 + MathF.Sin(_timer * 0.05f) * 20;
                }
            }
        }

        if (invalid)
            return true;

        UpdateLinking(npc);

        return true;
    }

    private void UpdateLinking(NPC npc)
    {
        if (linkedTo.HasValue)
        {
            if (npc.type != NPCID.LunarTowerNebula && npc.DistanceSQ(Link.Center) > NebulaLinkPlayer.LinkDistance * NebulaLinkPlayer.LinkDistance)
                npc.Center = Link.Center + Link.DirectionTo(npc.Center) * NebulaLinkPlayer.LinkDistance;

            npc.netUpdate = true;
        }
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (linkedTo.HasValue)
            LinkPrimDrawer.Draw(npc, Link, _timer, npc.Distance(Link.Center), npc.type == NPCID.LunarTowerNebula);

        return true;
    }

    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct LinkPrimDrawer
    {
        private static readonly VertexStrip _vertexStrip = new();

        public static void Draw(NPC npc, Entity link, int timer, float maxDistance, bool fromPillar)
        {
            float opacity = MathF.Min(1f, maxDistance / 400f);
            MiscShaderData miscShaderData = GameShaders.Misc["FlameLash"];
            miscShaderData.UseOpacity(2f * npc.Opacity * opacity);
            miscShaderData.UseImage0("Images/Extra_191");
            miscShaderData.Apply();

            const int PointCount = 20;

            var positions = new Vector2[PointCount];
            float[] angles = new float[PointCount];

            for (int i = 0; i < PointCount; ++i)
            {
                Vector2 pos = GetPosition(npc, link, timer, maxDistance, i, PointCount);

                if (i < 3)
                {
                    pos = Vector2.Lerp(pos, npc.Center, i / 2f);
                }

                positions[i] = pos;
                angles[i] = (i == 0 ? pos.AngleTo(GetPosition(npc, link, timer, maxDistance, i + 1, PointCount)) : pos.AngleFrom(positions[i - 1])) + MathHelper.Pi;
            }

            _vertexStrip.PrepareStripWithProceduralPadding(positions, angles, progress => StripColors(progress, fromPillar ? maxDistance : 0), 
                progress => StripWidth(fromPillar ? maxDistance : 0), -Main.screenPosition);
            _vertexStrip.DrawTrail();
            Main.pixelShader.CurrentTechnique.Passes[0].Apply();
        }

        private static Vector2 GetPosition(NPC npc, Entity link, int timer, float maxDistance, int i, int count)
        {
            var pos = Vector2.Lerp(npc.Center, link.Center, i / (float)(count - 1));
            Vector2 sineOff = npc.DirectionTo(link.Center).RotatedBy(MathHelper.PiOver2) * MathF.Sin(timer * 0.1f + i) * 50 * pos.Distance(link.Center) / maxDistance;
            pos += sineOff;
            return pos;
        }

        private static Color StripColors(float progressOnStrip, float pillarDistance)
        {
            var result = Color.Lerp(Color.Purple, Color.White, progressOnStrip);
            result = Color.Lerp(result, Color.Red, pillarDistance / (NebulaLinkPlayer.LinkDistance * 2));
            result.A = (byte)(result.A * 0.7f);

            return result;
        }

        private static float StripWidth(float pillarDistance) => 20 * (1 - pillarDistance / (NebulaLinkPlayer.LinkDistance * 2)) + 5;
    }
}
