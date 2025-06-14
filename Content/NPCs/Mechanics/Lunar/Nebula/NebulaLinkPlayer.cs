using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.ModBrowser;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Nebula;

internal class NebulaLinkPlayer : ModPlayer
{
    public const int LinkDistance = 600;

    public int LinkCount => _connections.Count;

    internal bool canLink = false;

    private bool _lastMouseLeft = false;
    private bool _mouseLeft = false;
    private short? _hasPillar = null;
    private HashSet<short> _connections = [];
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

    public override void ResetEffects()
    {
        canLink = true;

        _lastMouseLeft = _mouseLeft;
        _mouseLeft = Main.mouseLeft;

        if (LinkCount >= 5)
        {
            _timeLeft++;

            if (_timeLeft > 60 * 10)
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
                            Vector2 pos = other.position + new Vector2(Main.rand.NextFloat(other.width), Main.rand.NextFloat(other.height));
                            Dust.NewDustPerfect(pos, DustID.PurpleTorch, Main.rand.NextVector2Circular(6, 6), Scale: Main.rand.NextFloat(1, 3)).noGravity = true;
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
        if (Main.netMode != NetmodeID.Server && canLink && _mouseLeft && !_lastMouseLeft)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                bool inHitbox = npc.Hitbox.Contains(Main.MouseWorld.ToPoint());
                bool isPillar = npc.type == NPCID.LunarTowerNebula;
                bool validOrPillar = _hasPillar.HasValue || isPillar;
                bool close = npc.DistanceSQ(Player.Center) < MathF.Pow(isPillar ? LinkDistance * 2 : LinkDistance, 2);
                
                if (close && npc.TryGetGlobalNPC(out NebulaLinkNPC neb) && inHitbox && validOrPillar && !neb.invalid)
                {
                    if (Main.netMode == NetmodeID.SinglePlayer)
                        AddConnection(npc, neb);
                    else
                        new SyncNebulaLink((byte)Player.whoAmI, (short)npc.whoAmI).Send();
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
