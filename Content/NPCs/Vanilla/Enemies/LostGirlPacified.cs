using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla.Enemies;

[AutoloadHead]
public class LostGirlPacified : ModNPC
{
    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 16;
        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.LostGirl);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.LostGirl." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
