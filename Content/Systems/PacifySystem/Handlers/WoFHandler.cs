using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.Systems.PacifySystem.Handlers;

internal class WoFHandler : PacifiedNPCHandler
{
    private static bool Pacifying = false;

    public override int Type => NPCID.WallofFlesh;

    public override bool CanPacify(NPC npc) => npc.GetGlobalNPC<PacifiedGlobalNPC>().unhitTime > 1 * 60 * 3 && Main.hardMode;

    public override void Load(Mod mod)
    {
        base.Load(mod);

        On_NPC.CreateBrickBoxForWallOfFlesh += StopBrickBoxForMercy;
    }

    private void StopBrickBoxForMercy(On_NPC.orig_CreateBrickBoxForWallOfFlesh orig, NPC self)
    {
        if (!Pacifying)
            orig(self);
    }

    public override void OnPacify(NPC npc)
    {
        npc.playerInteraction[Main.myPlayer] = true;

        Pacifying = true;
        npc.NPCLoot();
        Pacifying = false;

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC eye = Main.npc[i];

            if (eye.active && eye.type == NPCID.WallofFleshEye)
                OreifyNPC(eye);
        }

        OreifyNPC(npc);
        int orePosX = (int)(npc.position.X / 16f) - (6 * Math.Sign(npc.velocity.X));
        OreifyArea(orePosX, Main.maxTilesY - 210, new(orePosX, Main.maxTilesY - 105), 10, 200, true, byte.MaxValue);

        SoundEngine.PlaySound(SoundID.NPCDeath10, npc.Center);
        SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

        npc.active = false;
    }

    private static void OreifyNPC(NPC npc)
    {
        int baseX = (int)(npc.position.X / 16f);
        int baseY = (int)(npc.position.Y / 16f);
        byte paintId = npc.type == NPCID.WallofFleshEye ? PaintID.WhitePaint : PaintID.DeepRedPaint;
        OreifyArea(baseX, baseY, npc.Center / 16f, npc.width / 16, npc.height / 16, false, paintId);
    }

    private static void OreifyArea(int baseX, int baseY, Vector2 center, int width, int height, bool replaceFullWall, byte paint)
    {
        int halfWidth = width / 2;

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                int x = i + baseX;
                int y = j + baseY;

                if (replaceFullWall || Vector2.DistanceSquared(new(x, y), center) < halfWidth * halfWidth)
                {
                    int tileType = SelectTileType(x, y);

                    if (tileType != -1)
                        WorldGen.PlaceTile(x, y, tileType, true, true);

                    if (paint != byte.MaxValue)
                        WorldGen.paintTile(x, y, paint);
                }
            }
        }
    }

    /// <summary>
    /// Modifies the tile placed at x, y to give the feeling of random while being deterministic.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static int SelectTileType(int x, int y)
    {
        int sine = (int)(MathF.Sin(x + y * 4) * 2);

        if (y > Main.maxTilesY - 165 + sine && y < Main.maxTilesY - 156 + sine)
            return TileID.FleshBlock;

        int mod = x + y + sine;

        if (mod % 5 == 0 || mod % 8 == 0 || x - y % 3 == 0 || x % 5 == (sine % 5) || y % 5 == (sine % 5))
            return TileID.FleshBlock;

        if ((y - sine) % 30 < 6)
            return TileID.FleshBlock;

        int oreType = mod % 30;

        if (oreType < 5)
            return TileID.Palladium;
        else if (oreType < 10)
            return TileID.Mythril;
        else if (oreType < 15)
            return TileID.Orichalcum;
        else if (oreType < 20)
            return TileID.Adamantite;
        else if (oreType < 25)
            return TileID.Titanium;

        return TileID.Cobalt;
    }
}
