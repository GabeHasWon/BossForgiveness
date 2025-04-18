﻿using BossForgiveness.Common.Camera;
using BossForgiveness.Content.Items.ForVanilla;
using BossForgiveness.Content.NPCs.Mechanics.WoF;
using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.Misc;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using BossForgiveness.Content.Systems.Syncing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Generation;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;
using Terraria.WorldBuilding;

using static BossForgiveness.Content.NPCs.Mechanics.QueenBeePacificationNPC;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class QueenBeePacificationNPC : GlobalNPC, ICustomBarNPC
{
    public class QueenBeeDreamTE : ModTileEntity
    {
        internal QueenBeeRequirements requirements = new(false);

        public override bool IsTileValidForEntity(int x, int y) => Main.tile[x, y].TileType == TileID.Larva;

        public static int AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            var tileData = TileObjectData.GetTileData(type, style, alternate);
            int topLeftX = i - tileData.Origin.X;
            int topLeftY = j - tileData.Origin.Y;

            return ModContent.GetInstance<QueenBeeDreamTE>().Place(topLeftX, topLeftY);
        }

        public override void SaveData(TagCompound tag) => requirements.SaveData(tag);
        public override void LoadData(TagCompound tag) => requirements = QueenBeeRequirements.LoadData(tag);

        public void DrawDream(Vector2 pos)
        {
            pos -= new Vector2(80, 30);
            int iteration = 0;

            foreach (var item in requirements.desiredDatas)
            {
                Color light = Lighting.GetColor((pos + Main.screenPosition).ToTileCoordinates()) * 0.75f;
                Main.instance.LoadItem(item.ItemId);
                Texture2D tex = TextureAssets.Item[item.ItemId].Value;
                float off = MathF.Sin((Main.GameUpdateCount + iteration++ * 60) * 0.02f) * 10;
                Main.spriteBatch.Draw(tex, pos + new Vector2(0, off), null, light, 0f, tex.Size() * new Vector2(0, 0.5f), 1f, SpriteEffects.None, 0);
                pos.X += tex.Width + 4;
            }
        }

        public override void NetSend(BinaryWriter writer) => requirements.NetSend(writer);
        public override void NetReceive(BinaryReader reader) => requirements = QueenBeeRequirements.NetRecieve(reader);
    }
    
    public class LarveDrawDream : GlobalTile
    {
        public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            if (TileEntity.ByPosition.TryGetValue(new Point16(i, j), out var te) && te is QueenBeeDreamTE dream)
            {
                storedRequirements = dream.requirements;
                dream.Kill(i, j);
            }
        }
    }

    public static bool CheckingQueenBeeHousing = false;

    public static List<TileData> data = [];
    public static QueenBeeRequirements storedRequirements = null;

    public override bool InstancePerEntity => true;

    private QueenBeeRequirements requirements = null;
    private int _dramaticPauseTimer = 0;
    private bool _checkedHouseBee = false;

    public override void Load() 
    {
        IL_WorldGen.QuickFindHome += AddQueenBeeSpecificationsToHome;
        IL_WorldGen.CheckRoom += AddMoreChecksToHome;

        var data = TileObjectData.GetTileData(TileID.Larva, 0, 0);
        data.HookPostPlaceMyPlayer = new PlacementHook(QueenBeeDreamTE.AfterPlacement, -1, 0, false);
    }

    public override void OnSpawn(NPC npc, IEntitySource source)
    {
        if (npc.type == NPCID.QueenBee && storedRequirements != null)
        {
            requirements = storedRequirements;
            storedRequirements = null;
        }
    }

    private void AddMoreChecksToHome(ILContext il)
    {
        ILCursor c = new(il);

        if (!c.TryGotoNext(x => x.MatchLdsfld<WorldGen>(nameof(WorldGen.houseTile))))
            return;

        c.Emit(OpCodes.Ldarg_0);
        c.Emit(OpCodes.Ldarg_1);
        c.EmitDelegate(AddContext);
    }

    public static void AddContext(int x, int y) => data.Add(new TileData(Main.tile[x, y]));

    private void AddQueenBeeSpecificationsToHome(ILContext il)
    {
        ILCursor c = new(il);

        if (!c.TryGotoNext(MoveType.After, x => x.MatchCall<WorldGen>(nameof(WorldGen.RoomNeeds))))
            return;

        c.Emit(OpCodes.Ldarg_0);
        c.EmitDelegate(BeSpecificAboutQueensHouse);
    }

    public static void BeSpecificAboutQueensHouse(int who)
    {
        if (Main.npc[who].type != NPCID.QueenBee)
        {
            return;
        }

        if (Main.netMode == NetmodeID.Server && Main.npc[who].GetGlobalNPC<QueenBeePacificationNPC>()._checkedHouseBee)
            new SyncQBCheckedHouseModule(who).Send();
        else
            SetChecked(who);
    }

    public static void SetChecked(int who)
    {
        WorldGen.canSpawn = Main.npc[who].GetGlobalNPC<QueenBeePacificationNPC>().requirements.RequirementsSatisfied(data);
        Main.npc[who].GetGlobalNPC<QueenBeePacificationNPC>()._checkedHouseBee = true;
    }

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.QueenBee;

    public override bool PreAI(NPC npc)
    {
        requirements ??= new(false);

        if (!_checkedHouseBee)
        {
            Point center = Main.player[npc.target].Center.ToTileCoordinates();
            (npc.homeTileX, npc.homeTileY) = (center.X, center.Y);
            WorldGen.QuickFindHome(npc.whoAmI);
        }
        else
        {
            if (_dramaticPauseTimer++ < 300)
            {
                if (Main.netMode != NetmodeID.Server && _dramaticPauseTimer == 1 && Main.LocalPlayer.DistanceSQ(npc.Center) < 900 * 900)
                {
                    var modifier = new ZoomModifier(npc, 200, 100, 30, Main.GameZoomTarget);
                    Main.instance.CameraModifiers.Add(modifier);
                }

                npc.velocity *= 0.9f;
                npc.Center = Vector2.Lerp(npc.Center, new Vector2(npc.homeTileX, npc.homeTileY - 10).ToWorldCoordinates(), 0.05f);

                if (_dramaticPauseTimer == 299)
                {
                    if (!npc.homeless)
                    {
                        npc.Pacify<PacifiedQueenBee>();
                        WorldGen.QuickFindHome(npc.whoAmI);
                    }
                }

                return false;
            }
        }

        return true;
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = 0;
        barMax = 1;

        CustomBarEdit.TextOffsetX = 96;
        CustomBarEdit.OverrideText = Language.GetTextValue("Mods.BossForgiveness.BarLines.QueenBee");

        for (int i = 0; i < requirements.desiredDatas.Count; ++i)
        {
            var data = requirements.desiredDatas[i];
            CustomBarEdit.OverrideText += $"[i:{data.ItemId}] ";
        }

        return npc.life >= npc.lifeMax;
    }
}

public class LarvaeAddTESystem : ModSystem
{
    public override void ModifyWorldGenTasks(List<GenPass> tasks, ref double totalWeight)
        => tasks.Insert(tasks.Count - 1, new PassLegacy("BossForgiveness: Add TEs to Larvae", (_, _) =>
        {
            for (int i = 0; i < Main.maxTilesX; i++)
            {
                for (int j = (int)Main.worldSurface - 20; j < Main.maxTilesY - 200; j++)
                {
                    Tile tile = Main.tile[i, j];

                    if (tile.TileType != TileID.Larva || tile.TileFrameX != 0 || tile.TileFrameY != 0)
                        continue;

                    if (!TileEntity.ByPosition.ContainsKey(new Point16(i, j)))
                        ModContent.GetInstance<QueenBeeDreamTE>().Place(i, j);
                }
            }
        }));
}