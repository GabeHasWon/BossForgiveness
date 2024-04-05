using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class KingSlimePacified : ModNPC
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.KingSlime}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_7";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 6;

        NPCID.Sets.IsTownSlime[Type] = true;
        NPCID.Sets.IsTownPet[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.KingSlime);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.netAlways = true;
        NPC.homeless = true;
        NPC.scale = 1.1f;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = 120 * (int)(NPC.frameCounter / 10f % 4);

        if (!NPC.IsABestiaryIconDummy && !Collision.SolidCollision(NPC.BottomLeft, NPC.width, 6) && NPC.velocity.Y > 0)
            NPC.frame.Y = 120 * 5;
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.KingSlime." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D crown = TextureAssets.Extra[ExtrasID.KingSlimeCrown].Value;
        Vector2 drawPos = NPC.Center;
        int currentFrame = NPC.frame.Y / (TextureAssets.Npc[Type].Height() / Main.npcFrameCount[Type]);

        float offset = currentFrame switch
        {
            0 => 2f,
            1 => -6,
            2 => 2f,
            3 => 10f,
            4 => 2f,
            5 => 0,
            _ => 0
        };

        drawPos.Y += NPC.gfxOffY - (60 - offset) * NPC.scale;
        spriteBatch.Draw(crown, drawPos - screenPos, null, drawColor, 0f, crown.Size() / 2f, 1f, SpriteEffects.None, 0f);
    }
}
