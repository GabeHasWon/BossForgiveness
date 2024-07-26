using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla.Enemies;

[AutoloadHead]
public class PinkyPacified : ModNPC
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.BlueSlime}";
    public override string HeadTexture => "BossForgiveness/Content/NPCs/Vanilla/Enemies/PinkyPacified_Head";

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;

        NPCID.Sets.IsTownSlime[Type] = true;
        NPCID.Sets.IsTownPet[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Pinky);
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

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = 26 * (int)(NPC.frameCounter / 10f % 2);

        if (NPC.velocity.Y != 0)
            NPC.frame.Y = 0;
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Pinky." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();
}
