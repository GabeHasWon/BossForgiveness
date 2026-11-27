using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Vortex;

internal readonly record struct CachedDummyData(Vector2 Center, Vector2 Velocity, int Direction);

internal class VortexPillarPacificationNPC : GlobalNPC, ICustomBarNPC
{
    private class VortexPlayer
    {
        public Player Dummy = new();
        public bool Free = false;
        public int Timer = 0;
        public int OldPositionSlot = 0;
    }

    internal static Asset<Texture2D> Aura = null;

    public override bool InstancePerEntity => true;

    private readonly Dictionary<int, List<VortexPlayer>> clones = [];
    private readonly Dictionary<int, int> playerTimers = [];

    private int _vortoid = -1;
    private bool _invalid = false;

    public override void Load() => Aura = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/Lunar/Vortex/Aura");

    public override void SetStaticDefaults() => NPCID.Sets.MustAlwaysDraw[NPCID.LunarTowerVortex] = true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.LunarTowerVortex;

    public override bool PreAI(NPC npc)
    {
        if (_invalid)
            return true;

        if (Main.netMode != NetmodeID.MultiplayerClient)
        {
            if (_vortoid == -1)
                SetVortoid(npc);

            NPC vort = Main.npc[_vortoid];
            bool invalidVortoid = !vort.active || vort.type != ModContent.NPCType<Vortoid>();

            if (invalidVortoid)
                SetVortoid(npc);
        }

        return true;

        foreach (NPC np in Main.ActiveNPCs)
        {
            if (np.type is NPCID.VortexHornet or NPCID.VortexHornetQueen or NPCID.VortexLarva or NPCID.VortexRifleman or NPCID.VortexSoldier && npc.life < npc.lifeMax)
            {
                _invalid = true;

                Main.npc[_vortoid].active = false;
                break;
            }
        }


        List<Vector2> clonePositions = [];

        foreach (Player player in Main.ActivePlayers)
        {
            playerTimers.TryAdd(player.whoAmI, 0);

            if (player.DistanceSQ(npc.Center) < 1200 * 1200)
                playerTimers[player.whoAmI] = Math.Min(playerTimers[player.whoAmI] + 1, 600);
            else
                playerTimers[player.whoAmI] = Math.Max(playerTimers[player.whoAmI] - 1, 0);

            if (playerTimers[player.whoAmI] >= 600 && !clones.ContainsKey(player.whoAmI))
            {
                var clone = AddVortexPlayerToPlayer(npc, player, 20);

                if (!clones.TryGetValue(player.whoAmI, out List<VortexPlayer> value))
                    clones.Add(player.whoAmI, [clone]);
                else
                    value.Add(clone);
            }

            if (player.dead || playerTimers[player.whoAmI] <= 0)
                clones.Remove(player.whoAmI);

            foreach (List<VortexPlayer> listClones in clones.Values)
            {
                foreach (VortexPlayer clone in listClones)
                {
                    clonePositions.Add(clone.Dummy.Center);

                    if (clone.Dummy.Center.DistanceSQ(player.Center) < 120 * 120)
                    {
                        var reason = NetworkText.FromKey("Mods.BossForgiveness.VortexDeath." + Main.rand.Next(3), player.name);
                        player.Hurt(PlayerDeathReason.ByCustomReason(reason), 1, 0, dodgeable: false);
                        CombatText.NewText(player.Hitbox, CombatText.DamagedFriendly, 1);
                    }
                }
            }
        }

        foreach (Vector2 pos in clonePositions)
        {
            NPC vortoid = Main.npc[_vortoid];

            if (vortoid.DistanceSQ(pos) < 120 * 120 && vortoid.life > 1)
            {
                vortoid.SimpleStrikeNPC(1, 0, false, 0, null, false, 0, true);
                break;
            }
        }

        List<(int, VortexPlayer)> playersToAdd = [];

        foreach (var pair in clones)
        {
            (int who, List<VortexPlayer> players) = pair;

            foreach (var player in players)
                UpdateVortexPlayer(npc, who, player, playersToAdd);
        }

        foreach (var (who, player) in playersToAdd)
        {
            if (!clones.ContainsKey(who))
                clones.Add(who, [player]);
            else
                clones[who].Add(player);
        }

        return true;
    }

    private void SetVortoid(NPC npc) => _vortoid = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.Center.X, (int)npc.Center.Y, ModContent.NPCType<Vortoid>(), 0, npc.whoAmI);

    public override void SendExtraAI(NPC npc, BitWriter bitWriter, BinaryWriter binaryWriter) => binaryWriter.Write((short)_vortoid);
    public override void ReceiveExtraAI(NPC npc, BitReader bitReader, BinaryReader binaryReader) => _vortoid = binaryReader.ReadInt16();

    private static VortexPlayer AddVortexPlayerToPlayer(NPC npc, Player player, int slot, Action<VortexPlayer> hook = null)
    {
        var clone = new VortexPlayer() { Dummy = new Player(), OldPositionSlot = slot };
        clone.Dummy.CopyVisuals(player);
        clone.Dummy.Center = npc.Center + (player.Center - npc.Center);
        clone.Dummy.GetModPlayer<VortexModPlayer>().Dummy = true;

        hook?.Invoke(clone);
        return clone;
    }

    private static void UpdateVortexPlayer(NPC npc, int who, VortexPlayer player, List<(int, VortexPlayer)> toAdd)
    {
        Player original = Main.player[who];
        player.Timer++;
        player.Dummy.Update(254);

        CachedDummyData data = new(player.Dummy.position, player.Dummy.velocity, player.Dummy.direction);
        player.Dummy.CopyVisuals(original);
        player.Dummy.position = data.Center;
        player.Dummy.direction = data.Direction;

        if (!player.Free)
        {
            if (player.Timer > 200)
            {
                player.Timer = 0;
                player.Free = true;
            }
            else
            {
                if (player.Timer is 55)
                {
                    for (int i = 0; i < 5; ++i)
                    {
                        var plr = AddVortexPlayerToPlayer(npc, original, 40 + i * 20, plr => BurstPlayerModification(plr, npc));
                        toAdd.Add((original.whoAmI, plr));
                    }
                }

                player.Dummy.Center = npc.Center + (npc.Center - Main.player[who].Center);
                player.Dummy.direction = Math.Sign(player.Dummy.Center.X - npc.Center.X);
                player.Dummy.velocity = original.velocity;
            }
        }
        else
        {
            data = original.GetModPlayer<VortexModPlayer>().OldInformation[^player.OldPositionSlot];
            player.Dummy.velocity = data.Velocity;
            player.Dummy.direction = data.Direction;
            player.Dummy.Center = Vector2.Lerp(player.Dummy.Center, data.Center.Floor(), 0.2f);
        }
    }

    private static void BurstPlayerModification(VortexPlayer player, NPC npc)
    {
        player.Dummy.velocity = new Vector2(0, Main.rand.NextFloat(-3, -1)).RotatedByRandom(MathHelper.PiOver4);
        player.Timer = 450;
        player.Dummy.Center = npc.Center;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Main.spriteBatch.End();

        List<Player> plr = [];

        foreach (var players in clones.Values)
        {
            foreach (var player in players)
            {
                plr.Add(player.Dummy);
                player.Dummy.GetModPlayer<VortexModPlayer>().Dummy = true;
            }
        }

        Main.PlayerRenderer.DrawPlayers(Main.Camera, plr);
        Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.AnisotropicWrap, null, Main.Rasterizer, null, Main.GameViewMatrix.ZoomMatrix);
    }

    bool ICustomBarNPC.ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        NPC vort = Main.npc[_vortoid];
        barProgress = vort.life;
        barMax = vort.lifeMax;
        return _invalid;
    }
}
