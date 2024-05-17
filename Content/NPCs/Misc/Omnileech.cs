using BossForgiveness.Content.Items.ForVanilla;
using Microsoft.Xna.Framework;
using NPCUtils;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Misc;

public class Omnileech : ModNPC
{
    internal static int Self = -1;

    public override void SetStaticDefaults() => Main.npcCatchable[NPC.type] = true;

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.damage = 1;
        NPC.Size = new Vector2(36);
        NPC.lifeMax = 20;
        NPC.defense = 5;
        NPC.noGravity = true;
        NPC.dontCountMe = true;
        NPC.catchItem = (short)ModContent.ItemType<OmnileechItem>();
        NPC.friendly = true;
    }

    public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) => bestiaryEntry.AddInfo(this, "Ocean");

    public override void AI()
    {
        Self = NPC.whoAmI;
        NPC.velocity.Y += 0.1f;

        if (!NPC.collideY)
            NPC.rotation += 0.05f;
    }

    public override void OnKill() => Self = -1;
}

class OmnileechResetSystem : ModSystem
{
    public override void PreUpdateNPCs()
    {
        if (Omnileech.Self != -1 && !NPC.AnyNPCs(ModContent.NPCType<Omnileech>()))
            Omnileech.Self = -1;
    }
}
