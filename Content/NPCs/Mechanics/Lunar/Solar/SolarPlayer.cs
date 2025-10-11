using BossForgiveness.Content.NPCs.Mechanics.MoonLord;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar.Solar;

internal class SolarPlayer : ModPlayer
{
    internal class SolarShieldDrawInfo
    {
        public bool MidFrame => Offset.LengthSquared() < 40 * 40;

        public bool FlatState = false;
        public Vector2 Offset = Vector2.Zero;
        public float Rotation = 0f;

        public Rectangle GetHitbox(Vector2 center)
        {
            center += Offset;

            if (FlatState)
                return new Rectangle((int)center.X - 17, (int)center.Y - 19, 34, 38);
            else if (MidFrame)
                return new Rectangle((int)center.X - 15, (int)center.Y - 15, 30, 30);
            
            return new Rectangle((int)center.X - 10, (int)center.Y - 10, 20, 20);
        }
    }

    private readonly static HashSet<int> SolarEnemyIds = [NPCID.SolarCorite, NPCID.SolarDrakomire, NPCID.SolarSpearman, NPCID.SolarDrakomireRider, 
        NPCID.SolarSolenian, NPCID.SolarSpearman, NPCID.SolarSroller];

    public bool HasSolarShield = true;
    public float ShieldOpacity = 0f;

    internal SolarShieldDrawInfo DrawInfo = new();
    internal int ParryTime = 0;
    internal int ParryCooldown = 0;

    public override void ResetEffects()
    {
        ParryTime--;
        ParryCooldown--;

        HasSolarShield = NPC.FindFirstNPC(NPCID.LunarTowerSolar) is { } slot and not -1 && Main.npc[slot].DistanceSQ(Player.Center) < 2400 * 2400
            && Main.npc[slot].TryGetGlobalNPC(out SolarPillarPacificationNPC solar) && !solar.Invalid;
        ShieldOpacity = MathHelper.Lerp(ShieldOpacity, HasSolarShield ? 1 : 0, HasSolarShield ? 0.02f : 0.06f);

        DrawInfo.Offset = Vector2.Lerp(DrawInfo.Offset, Player.SafeDirectionTo(Main.MouseWorld) * MathF.Min(Player.Distance(Main.MouseWorld), 60), 0.06f);
        DrawInfo.FlatState = DrawInfo.Offset.LengthSquared() < 20 * 20;
        DrawInfo.Rotation = DrawInfo.Offset.ToRotation();
    }

    public override void PostUpdate()
    {
        if (!HasSolarShield)
            return;

        if (Main.myPlayer == Player.whoAmI && Main.mouseRight && Main.mouseRightRelease && ParryTime <= 0 && ParryCooldown <= 0)
        {
            ParryTime = 30;
            ParryCooldown = 5 * 60;
            
            SpawnShieldBreakEffects(15);
        }

        Rectangle shieldHitbox = DrawInfo.GetHitbox(Player.Center);

        if (ParryTime > 0)
        {
            foreach (NPC npc in Main.ActiveNPCs)
            {
                if (npc.Hitbox.Intersects(shieldHitbox) && SolarEnemyIds.Contains(npc.type))
                {
                    ParryCooldown = -1;
                    ParryTime = -1;

                    Player.SetImmuneTimeForAllTypes(60);

                    Pacify(npc, false);
                    break;
                }
            }
        }

        if (ParryCooldown == 0)
        {
            SpawnShieldBreakEffects(8);
        }
    }

    private void SpawnShieldBreakEffects(int count)
    {
        var vel = Vector2.Normalize(DrawInfo.Offset);

        for (int i = 0; i < count; ++i)
        {
            Vector2 position = GetShieldPosition();
            Vector2 velocity = vel * Main.rand.NextFloat(2, 5);

            Dust.NewDust(position, 1, 1, Main.rand.Next(3) switch
            {
                0 => DustID.Torch,
                1 => DustID.SolarFlare,
                _ => DustID.Obsidian,
            }, velocity.X, velocity.Y);
        }
    }

    private Vector2 GetShieldPosition()
    {
        if (DrawInfo.FlatState)
        {
            Vector2 size = new(34, 38);
            return Player.Center - size / 2 + new Vector2(Main.rand.NextFloat(size.X), Main.rand.NextFloat(size.Y)) + DrawInfo.Offset;
        }
        else
            return Player.Center + DrawInfo.Offset + Vector2.Normalize(DrawInfo.Offset).RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-21, 17);
    }

    internal static void Pacify(NPC npc, bool fromNet)
    {
        for (int i = 0; i < 20; ++i)
        {
            int type = Main.rand.Next(3) switch
            {
                0 => DustID.Torch,
                1 => DustID.SolarFlare,
                _ => DustID.Obsidian,
            };

            Dust.NewDust(npc.position, npc.width, npc.height, type, npc.velocity.X * 0.33f, npc.velocity.Y * 0.33f - Main.rand.NextFloat(6, 9));
        }

        if (Main.netMode == NetmodeID.MultiplayerClient && !fromNet)
            new SyncSolarPacifyModule((byte)npc.whoAmI, false).Send(runLocally: false);
        else
        {
            npc.active = false;
            npc.netUpdate = true;

            if (Main.netMode != NetmodeID.MultiplayerClient)
                UpdateNearbySolarPillar(false);
        }
    }

    internal static void UpdateNearbySolarPillar(bool notifyInvalid, NPC npc = null)
    {
        NPC pillar;

        if (npc is not null)
            pillar = npc;
        else
        {
            int solar = NPC.FindFirstNPC(NPCID.LunarTowerSolar);

            if (solar == -1)
                return;

            pillar = Main.npc[solar];
        }

        SolarPillarPacificationNPC pac = pillar.GetGlobalNPC<SolarPillarPacificationNPC>();

        if (notifyInvalid)
            pac.Invalid = true;
        else if (pac.Count++ > SolarPillarPacificationNPC.MaxPacification)
        {
            pillar.active = false;
            MoonLordPacificationTracker.LunarTowersPacified.Add(NPCID.LunarTowerSolar);
        }

        pillar.netUpdate = true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        if (SolarEnemyIds.Contains(target.type) || target.type == NPCID.LunarTowerSolar)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                UpdateNearbySolarPillar(true, target);
            else
                new SyncSolarPacifyModule((byte)target.whoAmI, true).Send(runLocally: false);
        }
    }
}

public class SolarShieldLayer : PlayerDrawLayer
{
    private static Asset<Texture2D> Shield = null;

    public override void Load() => Shield = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/Lunar/Solar/SolarShield");

    public override Position GetDefaultPosition() => new AfterParent(PlayerDrawLayers.FrontAccFront);

    protected override void Draw(ref PlayerDrawSet drawInfo)
    {
        SolarPlayer solar = drawInfo.drawPlayer.GetModPlayer<SolarPlayer>();

        if (drawInfo.shadow != 0 || solar.ShieldOpacity <= 0.01f)
            return;

        SolarPlayer.SolarShieldDrawInfo shieldInfo = solar.DrawInfo;
        Rectangle src = shieldInfo.FlatState ? new(18, 0, 34, 38) : GetAngledShape(shieldInfo);
        float opacity = Utils.GetLerpValue(2 * 60, 0, solar.ParryCooldown, true) * solar.ShieldOpacity;
        Vector2 position = drawInfo.Center + shieldInfo.Offset - Main.screenPosition;
        SpriteEffects effect = shieldInfo.Offset.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
        float rotation = shieldInfo.Offset.X < 0 ? shieldInfo.Rotation + MathHelper.Pi : shieldInfo.Rotation;
        var data = new DrawData(Shield.Value, position, src, Color.White * opacity, shieldInfo.FlatState ? 0 : rotation, src.Size() / 2f, 1f, effect);
        drawInfo.DrawDataCache.Add(data);

#if DEBUG
        Rectangle hitbox = shieldInfo.GetHitbox(drawInfo.Center);
        hitbox.X -= (int)Main.screenPosition.X;
        hitbox.Y -= (int)Main.screenPosition.Y;
        drawInfo.DrawDataCache.Add(new DrawData(TextureAssets.MagicPixel.Value, hitbox, Color.Red * 0.6f));
#endif
    }

    private static Rectangle GetAngledShape(SolarPlayer.SolarShieldDrawInfo shieldInfo) => shieldInfo.MidFrame ? new(54, 0, 26, 38) : new(0, 0, 14, 38);
}