using BossForgiveness.Content.NPCs.Vanilla.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Mechanics.Enemies;

internal class RainbowSlimePacificationNPC : GlobalNPC
{
    public const int MaxCount = 60 * 4;
    public const int MaxDismay = 60 * 2;

    private static Asset<Texture2D> Face = null;

    public override bool InstancePerEntity => true;

    public bool canPacify = true;
    public bool pacifying = false;

    private float _relativeDistance = 0;
    private int _partner = -1;
    private int _countTime = 0;
    private Vector2 _visualScale = Vector2.One;

    public override bool AppliesToEntity(NPC entity, bool lateInstantiation) => entity.type == NPCID.RainbowSlime;

    public override void SetStaticDefaults() => Face = ModContent.Request<Texture2D>("BossForgiveness/Content/NPCs/Vanilla/Enemies/RainbowSlimePacified_Face");

    public override bool PreAI(NPC npc)
    {
        if (Math.Abs(npc.velocity.X) > 0.01f)
            npc.direction = npc.spriteDirection = Math.Sign(npc.velocity.X);

        if (!canPacify)
            npc.GetGlobalNPC<SpeedUpBehaviourNPC>().behaviourSpeed += 0.25f;

        if (npc.life < npc.lifeMax || !canPacify)
            return true;

        if (pacifying)
        {
            ref float dismayTimer = ref npc.ai[0];
            ref float state = ref npc.ai[1];

            Dance(npc, ref dismayTimer, ref state);
            npc.AI_001_SetRainbowSlimeColor();

            if (npc.collideY)
                npc.velocity.X *= 0.75f;

            return false;
        }

        _countTime = (int)MathHelper.Clamp(_countTime - 1, 0, MaxCount);

        foreach (Player player in Main.ActivePlayers)
        {
            if (player.DistanceSQ(npc.Center) < 80 * 80)
            {
                _countTime += 2;

                if (_countTime > MaxCount)
                {
                    pacifying = true;
                    _partner = player.whoAmI;
                    _relativeDistance = npc.Distance(player.Center);
                    _countTime = 0;

                    npc.ai[0] = 0;
                    npc.ai[1] = 0;

                    for (int i = 0; i < 20; ++i)
                    {
                        int num9 = Dust.NewDust(npc.position, npc.width, npc.height, DustID.RainbowTorch, 0f, 0f, 100, Main.DiscoColor, 2.5f);
                        Main.dust[num9].noGravity = true;
                    }
                }

                npc.AI_001_SetRainbowSlimeColor();

                if (npc.collideY)
                    npc.velocity.X *= 0.75f;

                return false;
            }
        }

        return true;
    }

    private void Dance(NPC npc, ref float dismayTimer, ref float state)
    {
        Player partner = Main.player[_partner];
        float delta = Math.Abs(partner.Distance(npc.Center) - _relativeDistance);

        if (delta > 100)
        {
            dismayTimer++;

            if (dismayTimer > MaxDismay)
            {
                pacifying = false;
                canPacify = false;
            }
        }
        else
            dismayTimer = MathF.Max(dismayTimer - 1, 0);

        _countTime++;

        if (_countTime % 160 == 0)
        {
            npc.velocity.Y -= 9;
            npc.velocity.X = state switch
            {
                0 or 4 or 7 => -6,
                1 or 3 or 6 => 6,
                _ => 0,
            };

            state++;

            if (state > 8)
                npc.Pacify<RainbowSlimePacified>();
        }
    }

    public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (pacifying)
        {
            GetScaling();

            Texture2D tex = TextureAssets.Npc[npc.type].Value;
            Vector2 position = npc.Center - screenPos + new Vector2(0, 26);
            Color color = Lighting.GetColor(npc.Center.ToTileCoordinates(), npc.color) with { A = (byte)npc.alpha };
            Color faceColor = color * (npc.ai[0] / MaxDismay);
            SpriteEffects effect = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(tex, position, npc.frame, color, 0f, npc.frame.Size() / new Vector2(2, 1), _visualScale, SpriteEffects.None);
            Main.EntitySpriteDraw(Face.Value, position, npc.frame, faceColor with { A = 220 }, 0f, npc.frame.Size() / new Vector2(2, 1), _visualScale, effect);
            return false;
        }

        return true;
    }

    public override void PostDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!canPacify)
        {
            Vector2 position = npc.Center - screenPos + new Vector2(0, 26);
            Color color = Lighting.GetColor(npc.Center.ToTileCoordinates(), npc.color) with { A = 220 };
            SpriteEffects effect = npc.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            Main.EntitySpriteDraw(Face.Value, position, npc.frame, color, 0f, npc.frame.Size() / new Vector2(2, 1), _visualScale, effect);
        }
    }

    private void GetScaling()
    {
        int time = _countTime % 160;

        if (time < 110)
            _visualScale = Vector2.Lerp(_visualScale, Vector2.One, 0.1f);
        else 
        {
            _visualScale.X += 0.01f;
            _visualScale.Y -= 0.01f;
        }
    }
}
