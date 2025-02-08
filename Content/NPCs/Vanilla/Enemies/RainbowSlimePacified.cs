using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla.Enemies;

[AutoloadHead]
public class RainbowSlimePacified : ModNPC
{
    public override string Texture => "Terraria/Images/NPC_" + NPCID.RainbowSlime;
    public override string HeadTexture => "BossForgiveness/Content/NPCs/Vanilla/Enemies/RainbowSlimePacified_Head";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.RainbowSlime];
        NPCID.Sets.IsTownPet[Type] = true;
        NPCID.Sets.IsTownSlime[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.RainbowSlime);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = NPCID.RainbowSlime;
    }

    public override bool PreAI()
    {
        NPC.AI_001_SetRainbowSlimeColor();
        return true;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetText("Mods.BossForgiveness.Dialogue.RainbowSlime." + Main.rand.Next(4)).Format(Main.DiscoColor.Hex3());
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
