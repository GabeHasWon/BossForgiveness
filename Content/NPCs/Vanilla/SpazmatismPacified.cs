using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.BigProgressBar;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class SpazmatismPacified : ModNPC, IAdditionalHoverboxes
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.Spazmatism}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_20";

    private ref float Timer => ref NPC.ai[0];
    private ref float NetTimer => ref NPC.ai[1];

    private NPC _retinazerDummy = null;
    private bool _isRetinazer = false;

    public override void Load() => On_TwinsBigProgressBar.ValidateAndCollectNecessaryInfo += StopPacifiedBar;

    private bool StopPacifiedBar(On_TwinsBigProgressBar.orig_ValidateAndCollectNecessaryInfo orig, TwinsBigProgressBar self, ref BigProgressBarInfo info)
    {
        bool valid = orig(self, ref info);

        if (valid && Main.npc[info.npcIndexToAimAt].type == ModContent.NPCType<SpazmatismPacified>())
            return false; // Force bar to hide if I'm not an actual Spazmatism

        return valid;
    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 6;

        NPCID.Sets.MustAlwaysDraw[Type] = true;
        NPCID.Sets.IsTownPet[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Spazmatism);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.noTileCollide = true;
        NPC.netAlways = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = 200 * (int)(NPC.frameCounter / 10f % 3);
    }

    public override bool PreAI()
    {
        const int LoweredHeight = 16 * 16;

        if (_retinazerDummy is null && !_isRetinazer)
            InitRetinazer(-1);

        if (!_isRetinazer)
            UpdateRetinazer();

        NPC.breath = NPC.breathMax;
        Timer++;
        
        if (NetTimer++ > 600)
        {
            NPC.netUpdate = true;
            NetTimer = 0;
        }

        int y = (!NPC.homeless ? NPC.homeTileY : NPC.GetFloor()) * 16;
        float dist = y - NPC.Center.Y;

        Player player = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];
        float minFloorDist = 16 * 20;
        float maxFloorDist = 16 * 24;
        bool talking = NPC.IsBeingTalkedTo();

        if (NPC.homeless)
        {
            bool nearPlayer = player.active && !player.dead && player.DistanceSQ(NPC.Center) < 700 * 700;

            if (nearPlayer || talking)
            {

                NPC.velocity *= 0.98f;
                NPC.rotation = Utils.AngleLerp(NPC.rotation, player.AngleFrom(NPC.Center) - MathHelper.PiOver2, 0.12f);

                minFloorDist -= LoweredHeight;
                maxFloorDist -= LoweredHeight;
            }
            else
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.12f);

                float diff = MathF.Pow(MathF.Sin(Timer * 0.03f), 2) * 15;
                minFloorDist -= diff;
                maxFloorDist -= diff;

                NPC.velocity.X = MathF.Cos(Timer * 0.03f) * 5f;
            }
        }
        else
        {
            const float Speed = 7;

            bool nearPlayer = player.active && !player.dead && player.DistanceSQ(NPC.Center) < 300 * 300;

            if (!nearPlayer && !talking)
            {
                Vector2 home = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates() + new Vector2(0, 400).RotatedBy(Timer * 0.03f);
                NPC.velocity += NPC.DirectionTo(home) * 0.1f;
                NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.12f);

                if (NPC.velocity.LengthSquared() > Speed * Speed)
                    NPC.velocity = Vector2.Normalize(NPC.velocity) * Speed;
            }
            else
            {
                NPC.rotation = Utils.AngleLerp(NPC.rotation, player.AngleFrom(NPC.Center) - MathHelper.PiOver2, 0.12f);
                NPC.velocity *= 0.9f;

                minFloorDist -= LoweredHeight;
                maxFloorDist -= LoweredHeight;
            }
        }

        if (dist < maxFloorDist)
            NPC.velocity.Y -= 0.1f;
        else if (dist > minFloorDist)
            NPC.velocity.Y += 0.1f;

        return false;
    }

    private void UpdateRetinazer()
    {
        _retinazerDummy.UpdateNPC(200);
        _retinazerDummy.ai[0] = Timer + MathHelper.Pi / 0.03f;
        _retinazerDummy.homeless = NPC.homeless;
        _retinazerDummy.homeTileX = NPC.homeTileX;
        _retinazerDummy.homeTileY = NPC.homeTileY;
    }

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        var tex = TextureAssets.Npc[NPCID.Retinazer].Value;
        var col = Lighting.GetColor(_retinazerDummy.Center.ToTileCoordinates());
        Main.EntitySpriteDraw(tex, _retinazerDummy.Center - screenPos, NPC.frame, col, _retinazerDummy.rotation, NPC.frame.Size() / 2f, 1f, 0, 0);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (_retinazerDummy is null)
            InitRetinazer(-1);

        float scale = _retinazerDummy.scale;
        float x = _retinazerDummy.Center.X;
        float num10 = _retinazerDummy.Center.Y * scale;
        Vector2 drawPos = new(NPC.Center.X, NPC.position.Y * scale);
        float num11 = x - drawPos.X;
        float num12 = num10 - drawPos.Y;
        float rotation = (float)Math.Atan2(num12, num11) - 1.57f;
        float length = (float)Math.Sqrt(num11 * num11 + num12 * num12);

        if (length > 2000f)
            return true;

        float num14 = 40f * scale;

        while (true)
        {
            length = (float)Math.Sqrt(num11 * num11 + num12 * num12);

            if (length < num14)
                break;

            length = TextureAssets.Chain12.Height() * scale / length;
            num11 *= length;
            num12 *= length;
            drawPos.X += num11;
            drawPos.Y += num12;
            num11 = x - drawPos.X;
            num12 = num10 - drawPos.Y;
            Color color = Lighting.GetColor(drawPos.ToTileCoordinates());
            spriteBatch.Draw(TextureAssets.Chain12.Value, drawPos - screenPos, (Rectangle?)new Rectangle(0, 0, TextureAssets.Chain12.Width(), TextureAssets.Chain12.Height()), color, rotation, new Vector2((float)TextureAssets.Chain12.Width() * 0.5f, (float)TextureAssets.Chain12.Height() * 0.5f), scale, (SpriteEffects)0, 0f);
        }

        return true;
    }

    public void InitRetinazer(int ret)
    {
        _retinazerDummy = new NPC();
        _retinazerDummy.SetDefaults(Type);

        if (ret == -1)
            _retinazerDummy.position = NPC.position;
        else
            _retinazerDummy.position = Main.npc[ret].position;

        (_retinazerDummy.ModNPC as SpazmatismPacified)._isRetinazer = true;
    }

    public override bool? CanFallThroughPlatforms() => true;
    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Twins." + Main.rand.Next(3));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public List<Hoverbox> GetAdditionalHoverboxes()
    {
        if (_retinazerDummy is null)
            InitRetinazer(-1);

        return [new Hoverbox(NPC.whoAmI, _retinazerDummy.Hitbox, 15, Lang.GetNPCNameValue(NPCID.Retinazer))];
    }
}
