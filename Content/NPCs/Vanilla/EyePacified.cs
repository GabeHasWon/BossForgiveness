using BossForgiveness.Content.Items.ForVanilla;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class EyePacified : ModNPC
{
    private ref float Timer => ref NPC.ai[0];

    private bool IsLassoed
    {
        get => NPC.ai[1] == 1f;
        set => NPC.ai[1] = value ? 1f : 0f;
    }

    private int RiderWhoAmI { get => (int)NPC.ai[2]; set => NPC.ai[2] = value; }

    private ref float NetTimer => ref NPC.ai[3];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 6;

        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.EyeofCthulhu);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.noTileCollide = false;
        NPC.netAlways = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = 166 * (int)(NPC.frameCounter / 10f % 3);
    }

    public override bool PreAI()
    {
        if (NetTimer++ > 600)
        {
            NPC.netUpdate = true;
            NetTimer = 0;
        }

        if (NPC.homeTileX == -1 || NPC.homeTileY == -1)
            NPC.homeless = true;

        NPC.rotation = Utils.AngleLerp(NPC.rotation, NPC.velocity.ToRotation() - MathHelper.PiOver2, 0.1f);

        if (IsLassoed) // Stop all behaviour
            return false;

        int y = (!NPC.homeless ? NPC.homeTileY : NPC.GetFloor()) * 16;
        float dist = y - NPC.Center.Y;

        if (dist < 16 * 15)
            NPC.velocity.Y -= 0.05f;
        else if (dist > 16 * 5)
            NPC.velocity.Y += 0.05f;

        if (NPC.homeless)
        {
            NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -6, 6);

            if (AnyNearbyPlayer(500, out var plrPos))
            {
                NPC.velocity.X += (plrPos.X - NPC.Center.X) / 1000f;
                NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5, 5);
            }
            else
            {
                NPC.velocity.X *= 0.99f;
                Timer++;

                if (Timer % 120 == 0)
                {
                    NPC.velocity.X = Main.rand.NextFloat(-5f, 5f);
                    NPC.netUpdate = true;
                }
            }
        }
        else
        {
            float homeX = NPC.homeTileX * 16;

            NPC.velocity.X += (homeX - NPC.Center.X) / 1000f;
            NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -5, 5);
        }

        return false;
    }

    private bool AnyNearbyPlayer(int distance, out Vector2 playerPos)
    {
        playerPos = Vector2.Zero;

        for (int i = 0; i < Main.maxPlayers; ++i)
        {
            Player plr = Main.player[i];

            if (plr.active && !plr.dead && plr.DistanceSQ(NPC.Center) < distance * distance)
            {
                playerPos = plr.Center;
                return true;
            }
        }
        return false;
    }

    internal void Unmount()
    {
        IsLassoed = false;
        RiderWhoAmI = -1;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (RiderWhoAmI != -1 && !NPC.IsABestiaryIconDummy) // Manually draw mounted player
        {
            Main.spriteBatch.End();

            // Set values for player draw
            EoCLeash.EoCLassoPlayer.OverrideDraw = true;
            var plr = Main.player[RiderWhoAmI];
            float originalRotation = plr.fullRotation;
            plr.fullRotation = NPC.rotation + MathHelper.PiOver2;
            Vector2 oldRotOrigin = plr.fullRotationOrigin;
            plr.fullRotationOrigin = plr.Size / 2f;

            // Draw player
            Main.PlayerRenderer.DrawPlayers(Main.Camera, [plr]);

            // Reset values
            EoCLeash.EoCLassoPlayer.OverrideDraw = false;
            plr.fullRotation = originalRotation;
            plr.fullRotationOrigin = oldRotOrigin;

            Main.spriteBatch.Begin();
        }

        // Manually draw to fix dumb vanilla origin
        Texture2D tex = TextureAssets.Npc[Type].Value;
        Main.EntitySpriteDraw(tex, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, new Vector2(55, 104), 1f, SpriteEffects.None, 0);
        return false;
    }

    internal Vector2 GetPlayerCenter() => NPC.Center + NPC.rotation.ToRotationVector2() * 60;

    public override bool CheckConditions(int left, int right, int top, int bottom) => bottom < Main.worldSurface;
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.EoC." + (!IsLassoed ? "Idle" : "Riding") + "." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => new EoCProfile();

    public class EoCProfile : ITownNPCProfile
    {
        public int RollVariation() => 0;
        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();

        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => TextureAssets.Npc[NPCID.EyeofCthulhu];
        public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("BossForgiveness/Content/NPCs/Vanilla/EyePacified_Head");
    }
}
