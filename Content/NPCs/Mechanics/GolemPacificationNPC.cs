using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using BossForgiveness.Content.Items.ForVanilla;
using Terraria.Localization;

namespace BossForgiveness.Content.NPCs.Mechanics;

public class GolemPacificationNPC : GlobalNPC, ICustomBarNPC
{
    public override bool InstancePerEntity => true;

    public const int MaxTaser = 5;

    internal int taserCount = 0;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.GolemHead || entity.type == NPCID.GolemHeadFree
        || entity.type == NPCID.Golem || entity.type == NPCID.GolemFistLeft || entity.type == NPCID.GolemFistRight;

    public override bool PreAI(NPC npc)
    {
        taserCount = (int)MathHelper.Clamp(taserCount, 0, npc.type == NPCID.Golem ? 20 : 6);

        if (npc.type == NPCID.GolemHead)
        {
            if (taserCount > 1)
                npc.ai[0] = 1f;

            if ((taserCount > 5 || Main.npc[NPC.golemBoss].GetGlobalNPC<GolemPacificationNPC>().taserCount > 10) && Main.netMode != NetmodeID.MultiplayerClient)
            {
                npc.Transform(NPCID.GolemHeadFree);
                npc.netUpdate = true;
            }
        }
        else if (npc.type == NPCID.GolemHeadFree)
        {
            if (taserCount > 5)
            {
                npc.damage = 0;
                npc.velocity.Y += 0.2f;
                npc.rotation += npc.velocity.X * 0.02f;
                npc.noTileCollide = false;

                return false;
            }
        }
        else if (npc.type == NPCID.Golem)
        {
            if (taserCount == 20)
            {
                if (Main.netMode != NetmodeID.Server)
                    SoundEngine.PlaySound(SoundID.Item14, npc.Center);

                npc.SimpleStrikeNPC(1, 0, false, 0, null, false, 0, true);
                npc.NPCLoot();
                npc.active = false;
                npc.netUpdate = true;

                for (int i = 0; i < 80; ++i)
                    SpawnGoldFlames(npc);

                for (int j = 0; j < 12; ++j)
                    SpawnSmoke(npc);
            }
        }

        npc.GetGlobalNPC<SpeedUpBehaviourNPC>().behaviourSpeed += taserCount * 0.1f;
        npc.defense = (int)(npc.defDefense * (1 + taserCount * 0.5f));

        return true;
    }

    public override void OnKill(NPC npc)
    {
        for (int i = npc.whoAmI; i < Main.maxNPCs; ++i)
        {
            NPC nextNPC = Main.npc[i];

            if (nextNPC.active && nextNPC.type == NPCID.GolemHeadFree && nextNPC.GetGlobalNPC<GolemPacificationNPC>().taserCount > 5)
            {
                for (int j = 0; j < 60; ++j)
                    SpawnGoldFlames(nextNPC);

                for (int j = 0; j < 12; ++j)
                    SpawnSmoke(nextNPC);

                nextNPC.active = false;
            }
        }
    }

    public override void DrawEffects(NPC npc, ref Color drawColor)
    {
        if (taserCount == 0)
            return;

        if (Main.rand.NextBool(Math.Max(65 - taserCount * 7, 2)))
            SpawnGoldFlames(npc);

        if (taserCount > 10 && Main.rand.NextBool(Math.Max(30 - (taserCount - 10) * 2, 3)))
            SpawnSmoke(npc);
    }

    private static void SpawnSmoke(NPC npc)
    {
        var pos = npc.position + new Vector2(Main.rand.Next(npc.width), Main.rand.Next(npc.height));
        Gore.NewGorePerfect(npc.GetSource_FromAI(), pos, new Vector2(0, Main.rand.NextFloat(2, 5)).RotatedByRandom(MathHelper.TwoPi), GoreID.Smoke1);
    }

    private static void SpawnGoldFlames(NPC npc)
    {
        int dust = Dust.NewDust(npc.position, npc.width, npc.height, DustID.GoldFlame, 0, 0);
        Main.dust[dust].fadeIn = 0.2f;
        Main.dust[dust].scale = Main.rand.NextFloat(1.25f, 1.75f);
        Main.dust[dust].velocity = new Vector2(0, Main.rand.NextFloat(2, 3)).RotatedByRandom(MathHelper.TwoPi);
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (npc.type == NPCID.GolemHeadFree && taserCount > 5)
        {
            var tex = TextureAssets.Npc[npc.type].Value;
            var col = npc.GetAlpha(Lighting.GetColor(npc.Center.ToTileCoordinates()));

            Main.EntitySpriteDraw(tex, npc.Center - screenPos, npc.frame, col, npc.rotation, npc.frame.Size() / 2f, 1f, SpriteEffects.None, 0);
            return false;
        }

        return true;
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = taserCount;
        barMax = MaxTaser + 1;

        if (npc.type == NPCID.GolemHeadFree && taserCount > 5)
            CustomBarEdit.OverrideText = Language.GetTextValue("Mods.BossForgiveness.BarLines.Golem");
        else if (npc.type == NPCID.Golem && !NPC.AnyNPCs(NPCID.GolemHeadFree) && !NPC.AnyNPCs(NPCID.GolemHead))
            barMax = 20;

        return npc.life == npc.lifeMax;
    }
}