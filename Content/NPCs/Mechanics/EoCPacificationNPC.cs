using BossForgiveness.Content.NPCs.Vanilla;
using BossForgiveness.Content.Systems.PacifySystem.BossBarEdits;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics;

internal class EoCPacificationNPC : GlobalNPC, ICustomBarNPC
{
    public const int MaxDiscontent = 5;

    public static Asset<Texture2D> AngerMark = null;

    public override bool InstancePerEntity => true;

    public bool IsContent => _discontentness == 0;

    private float _discontentness = 5;
    private bool _wet = false;
    private float _angerMarkOpacity = 0;
    private bool? _canPacify = null;

    public override void Load() => AngerMark = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Mechanics/EoCAngerMark");

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.EyeofCthulhu;

    public override bool PreAI(NPC npc)
    {
        _canPacify ??= NPC.AnyNPCs(ModContent.NPCType<EyePacified>());

        if (!_canPacify.Value)
            return true;

        if (npc.life < npc.lifeMax)
            return true;

        if (Collision.WetCollision(npc.position, npc.width, npc.height))
        {
            if (!_wet)
                _discontentness--;

            _wet = true;
        }
        else
            _wet = false;

        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        _angerMarkOpacity = MathHelper.Lerp(_angerMarkOpacity, npc.life < npc.lifeMax ? 0 : _discontentness / 5f, 0.05f);

        if (_angerMarkOpacity == 0)
            return;

        float sineSpeed = _discontentness / 5f * 1.5f;
        Color color = Color.Lerp(drawColor, Color.White, 0.4f) * _angerMarkOpacity;
        float scale = MathF.Max(1, MathF.Pow(MathF.Sin(Main.GlobalTimeWrappedHourly * 8 * sineSpeed), 2) + 0.3f);
        Vector2 origin = npc.Size / (2f);
        spriteBatch.Draw(AngerMark.Value, npc.Center - screenPos, null, color, npc.rotation, origin, scale, SpriteEffects.None, 0);
    }

    public bool ShowOverlay(NPC npc, out float barProgress, out float barMax)
    {
        barProgress = MaxDiscontent - _discontentness;
        barMax = MaxDiscontent;
        return true;
    }
}
