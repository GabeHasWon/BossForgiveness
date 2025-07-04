using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Vortex;

internal readonly record struct CachedDummyData(Vector2 Center, Vector2 Velocity, int Direction);

internal class VortexPillarPacificationNPC : GlobalNPC
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

    public override void Load() => Aura = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/Lunar/Vortex/Aura");

    public override void SetStaticDefaults() => NPCID.Sets.MustAlwaysDraw[NPCID.LunarTowerVortex] = true;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.LunarTowerVortex;

    public override bool PreAI(NPC npc)
    {
        foreach (Player player in Main.ActivePlayers)
        {
            playerTimers.TryAdd(player.whoAmI, 0);

            if (player.DistanceSQ(npc.Center) < 1200 * 1200)
                playerTimers[player.whoAmI]++;
            else
                playerTimers[player.whoAmI] = Math.Max(playerTimers[player.whoAmI] - 1, 0);

            if (playerTimers[player.whoAmI] > 600 && !clones.ContainsKey(player.whoAmI))
            {
                var clone = new VortexPlayer() { Dummy = new Player(), OldPositionSlot = 20 };
                clone.Dummy.CopyVisuals(player);
                clone.Dummy.Center = npc.Center + (player.Center - npc.Center);
                clone.Dummy.GetModPlayer<VortexModPlayer>().Dummy = true;
                clones.Add(player.whoAmI, [clone]);
            }

            foreach (List<VortexPlayer> listClones in clones.Values)
            {
                foreach (VortexPlayer clone in listClones)
                {
                    if (clone.Dummy.Center.DistanceSQ(player.Center) < 120 * 120)
                        player.statLife--;
                }
            }
        }

        foreach (var pair in clones)
        {
            (int who, List<VortexPlayer> players) = pair;

            foreach (var player in players)
                UpdateVortexPlayer(npc, who, player);
        }

        return true;
    }

    private static void UpdateVortexPlayer(NPC npc, int who, VortexPlayer player)
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
            player.Dummy.Center = npc.Center + (npc.Center - Main.player[who].Center);
            player.Dummy.direction = Math.Sign(player.Dummy.Center.X - npc.Center.X);
            player.Dummy.velocity = original.velocity;

            if (player.Timer > 600)
            {
                player.Timer = 0;
                player.Free = true;
            }
        }
        else
        {
            data = original.GetModPlayer<VortexModPlayer>().OldInformation[^player.OldPositionSlot];
            player.Dummy.Center = Vector2.Lerp(player.Dummy.Center, data.Center.Floor(), 0.2f);
            player.Dummy.direction = data.Direction;
            player.Dummy.velocity = data.Velocity;
        }
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
}
