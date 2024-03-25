using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class GolemHeadPacified : ModNPC
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.GolemHeadFree}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_5";

    private ref float Timer => ref NPC.ai[0];
    private ref float NetTimer => ref NPC.ai[2];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;
        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.BrainofCthulhu);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;

        if (!NPC.IsABestiaryIconDummy && NPC.IsBeingTalkedTo())
            NPC.frameCounter = 0;

        NPC.frame.Y = frameHeight * (int)(NPC.frameCounter / 28f % 2);
    }

    public override bool PreAI()
    {
        Timer++;

        if (NetTimer++ > 600) // Sync occasionally to be sure
        {
            NPC.netUpdate = true;
            NetTimer = 0;
        }

        float floor = NPC.homeless ? (NPC.GetFloor(80, true) - 15) * 16 : (NPC.homeTileY - 12) * 16;
        float yVel = (floor - NPC.Center.Y) / 300f;

        if (Math.Abs(yVel) < 0.5f)
            NPC.velocity.Y *= 0.8f;
        else
            NPC.velocity.Y += MathHelper.Clamp(yVel, -8, 8);

        if (NPC.homeless)
            NPC.velocity.X = (int)MathF.Round(MathF.Sin(Timer * 0.02f) * 2f) / 2 * 4;
        else
        {
            int homeX = (NPC.homeTileX * 16) + (int)MathF.Round(MathF.Sin(Timer * 0.005f) * 2f) / 2 * 300;
            int centerX = (int)NPC.Center.X;

            if (Math.Abs(homeX - centerX) > 30)
                NPC.velocity.X = Math.Sign(homeX - centerX) * 6;
            else
                NPC.velocity.X *= 0.6f;

            var tpLocation = new Vector2(NPC.homeTileX, NPC.homeTileY - 10).ToWorldCoordinates();

            if (NPC.DistanceSQ(tpLocation) > 4000 * 4000 && NPC.CanTeleport(tpLocation))
                NPC.Teleport(tpLocation, 0);
        }

        if (NPC.IsBeingTalkedTo())
            NPC.velocity *= 0;

        NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.X * 0.05f, 0.15f);
        NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -8, 8);
        
        DustSpam();
        return false;
    }

    private void DustSpam()
    {
        NPC.position += NPC.netOffset;

        int side = Main.rand.NextBool(2) ? -1 : 1;
        Vector2 sidePos = NPC.Center - new Vector2(26 * side, -44 + Main.rand.NextFloat(6)).RotatedBy(NPC.rotation);
        Vector2 sideVal = new Vector2(-4 * side, 0).RotatedBy(NPC.rotation);
        Dust sideDust = Dust.NewDustPerfect(sidePos, DustID.GoldFlame, sideVal, 0);
        sideDust.noGravity = true;
        sideDust.velocity += NPC.velocity;

        Vector2 bottomPos = NPC.Center + new Vector2(Main.rand.NextFloat(-4, 4f), 58).RotatedBy(NPC.rotation); 
        Vector2 bottomVel = Vector2.UnitY * (2f + Main.rand.NextFloat());
        Dust bottomDust = Dust.NewDustPerfect(bottomPos, DustID.GoldFlame, bottomVel, 0);
        bottomDust.fadeIn = 0f;
        bottomDust.scale = 0.7f + Main.rand.NextFloat() * 0.5f;
        bottomDust.noGravity = true;
        bottomDust.velocity += NPC.velocity;

        NPC.position -= NPC.netOffset;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Main.instance.LoadNPC(NPCID.Golem);

        var tex = TextureAssets.Npc[Type].Value;
        var col = NPC.GetAlpha(Lighting.GetColor(NPC.Center.ToTileCoordinates()));
        var glowColor = Color.Lerp(new Color(Main.mouseTextColor, Main.mouseTextColor, Main.mouseTextColor, 0), Color.OrangeRed * 0.5f, MathF.Pow(MathF.Sin(Main.GameUpdateCount * 0.03f), 2) * 0.5f);

        Main.EntitySpriteDraw(tex, NPC.Center - screenPos, NPC.frame, col, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);

        var bottomLights = TextureAssets.Extra[ExtrasID.GolemLights4].Value;
        int frame = (int)MathF.Floor(NPC.frame.Y / (float)tex.Height * Main.npcFrameCount[Type]);
        var bottomSrc = bottomLights.Frame(1, 8, 0, frame + (int)(Main.GameUpdateCount * 0.4f % 4) * 2, 0, 0);

        if (frame == 1)
            bottomSrc.Y--;

        Main.EntitySpriteDraw(bottomLights, NPC.Center - screenPos, bottomSrc, glowColor, NPC.rotation, NPC.frame.Size() / 2f, 1f, SpriteEffects.None, 0);

        var eyes = TextureAssets.Golem[1].Value;
        var eyeSrc = new Rectangle(0, 0, eyes.Width, eyes.Height / 2);
        var eyeOrigin = eyeSrc.Size() / 2f;
        var eyePos = new Vector2(NPC.Center.X - 20f + NPC.velocity.X * 1.5f, NPC.Center.Y - 17f) - screenPos + eyeOrigin;
        Main.spriteBatch.Draw(eyes, eyePos, eyeSrc, glowColor, NPC.rotation, eyeOrigin, 1f, 0, 0f);
        return false;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Golem." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
