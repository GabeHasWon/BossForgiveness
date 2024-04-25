using BossForgiveness.Content.NPCs.Mechanics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class QueenSlimePacified : ModNPC
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.QueenSlimeBoss}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_38";

    NPC _drawNPCDummy = null;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 16;

        NPCID.Sets.IsTownSlime[Type] = true;
        NPCID.Sets.IsTownPet[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.QueenSlimeBoss);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.netAlways = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.QueenSlime." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (NPC.IsABestiaryIconDummy)
            return false;

        if (_drawNPCDummy is null)
            SetDummy();

        _drawNPCDummy.Center = NPC.Center;
        _drawNPCDummy.velocity = NPC.velocity;
        _drawNPCDummy.FindFrame();
        _drawNPCDummy.GetGlobalNPC<QueenSlimePacificationNPC>().crystalOffset = new Vector2(-100000);
        Main.instance.DrawNPCDirect(Main.spriteBatch, _drawNPCDummy, false, screenPos);
        return false;
    }

    private void SetDummy()
    {
        _drawNPCDummy = new();
        _drawNPCDummy.SetDefaults(NPCID.QueenSlimeBoss);
    }
}
