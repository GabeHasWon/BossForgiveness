using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Nebula;

internal class NebulaLinkPlayer : ModPlayer
{
    public const int LinkDistance = 600;
    public const int MaxTimer = 60 * 30;

    private readonly HashSet<short> _connections = [];

    public int LinkCount => _connections.Count;

    internal bool lastCanLink = false;
    internal bool canLink = false;
    internal bool hovering = false;

    private bool _lastMouseLeft = false;
    private bool _mouseLeft = false;
    private short? _hasPillar = null;
    private int _timeLeft = 0;

    public override void PreUpdateMovement()
    {
        if (_hasPillar.HasValue)
        {
            NPC pillar = Main.npc[_hasPillar.Value];

            if (pillar.DistanceSQ(Player.Center) > MathF.Pow(LinkDistance * 2, 2))
                Player.Center = pillar.Center + pillar.DirectionTo(Player.Center) * (LinkDistance * 2);
        }
    }

    public override void UpdateDead()
    {
        lastCanLink = canLink;
        canLink = false;

        if (_connections.Count > 0)
            ClearConnections();
    }

    public override void ResetEffects()
    {
        lastCanLink = canLink;
        canLink = false;

        _lastMouseLeft = _mouseLeft;
        _mouseLeft = Main.mouseLeft;

        if (LinkCount >= 5)
        {
            _timeLeft++;

            if (Main.rand.NextFloat() < _timeLeft / (float)MaxTimer)
            {
                SpawnDust(Player);
            }

            if (_timeLeft > MaxTimer)
            {
                NPC pillar = Main.npc[_hasPillar.Value];
                pillar.active = false;
                pillar.netUpdate = true;

                Projectile.NewProjectile(pillar.GetSource_Death(), pillar.Center, Vector2.Zero, ModContent.ProjectileType<NebulaPortal>(), 0, 0, Main.myPlayer);

                foreach (var other in Main.ActiveNPCs)
                {
                    if (other.type is NPCID.NebulaBeast or NPCID.NebulaBrain or NPCID.NebulaHeadcrab or NPCID.NebulaSoldier)
                    {
                        other.active = false;
                        other.netUpdate = true;

                        for (int i = 0; i < 12; ++i)
                        {
                            SpawnDust(other);
                        }
                    }
                }
            }
        }
        else
        {
            _timeLeft = 0;
        }

        List<short> clearWho = [];
        bool abort = false;

        foreach (short who in _connections)
        {
            NPC npc = Main.npc[who];

            if (!npc.active || npc.life < npc.lifeMax)
            {
                abort = true;
                break;
            }
            else if (!npc.TryGetGlobalNPC<NebulaLinkNPC>(out _))
               clearWho.Add(who);
        }

        if (!abort)
        {
            foreach (short who in clearWho)
                _connections.Remove(who);
        }
        else
        {
            Main.npc[_hasPillar.Value].GetGlobalNPC<NebulaLinkNPC>().invalid = true;
            ClearConnections();
        }
    }

    private static void SpawnDust(Entity entity)
    {
        Vector2 pos = entity.position + new Vector2(Main.rand.NextFloat(entity.width), Main.rand.NextFloat(entity.height));
        Dust.NewDustPerfect(pos, DustID.PurpleTorch, Main.rand.NextVector2Circular(6, 6), Scale: Main.rand.NextFloat(1, 3)).noGravity = true;
    }

    private void ClearConnections()
    {
        Main.npc[_hasPillar.Value].GetGlobalNPC<NebulaLinkNPC>().linkedTo = null;

        foreach (short npc in _connections)
            Main.npc[npc].GetGlobalNPC<NebulaLinkNPC>().linkedTo = null;

        _connections.Clear();
        _hasPillar = null;
    }

    public override void UpdateEquips()
    {
        hovering = false;

        if (Main.netMode != NetmodeID.Server && lastCanLink)
        {
            bool clicked = _mouseLeft && !_lastMouseLeft;

            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (_connections.Contains((short)npc.whoAmI))
                    continue;

                Rectangle hitbox = npc.Hitbox;
                hitbox.Inflate(30, 30);

                bool inHitbox = hitbox.Contains(Main.MouseWorld.ToPoint());
                bool isPillar = npc.type == NPCID.LunarTowerNebula;
                bool close = npc.DistanceSQ(Player.Center) < MathF.Pow(isPillar ? LinkDistance * 2 : LinkDistance, 2);
                bool validOrPillar = _hasPillar.HasValue || isPillar;

                if (!hovering && close && validOrPillar)
                    hovering = inHitbox;

                if (clicked)
                {

                    if (close && npc.TryGetGlobalNPC(out NebulaLinkNPC neb) && inHitbox && validOrPillar && !neb.invalid)
                    {
                        if (Main.netMode == NetmodeID.SinglePlayer)
                            AddConnection(npc, neb);
                        else if (Main.netMode == NetmodeID.MultiplayerClient)
                            new SyncNebulaLink((byte)Player.whoAmI, (short)npc.whoAmI).Send();
                    }
                }
            }
        }
    }

    public void AddConnection(NPC npc, NebulaLinkNPC neb)
    {
        _connections.Add((short)npc.whoAmI);
        neb.linkedTo = Player.whoAmI;

        if (npc.type == NPCID.LunarTowerNebula)
            _hasPillar = (short)npc.whoAmI;
    }
}
