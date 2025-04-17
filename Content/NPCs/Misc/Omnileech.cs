using BossForgiveness.Content.Items.ForVanilla;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NPCUtils;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Misc;

public class Omnileech : ModNPC
{
    public override void SetStaticDefaults() => Main.npcCatchable[NPC.type] = true;

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.damage = 1;
        NPC.Size = new Vector2(40);
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
        NPC.velocity.Y += 0.1f;

        if (Math.Abs(NPC.velocity.Y) > 0.11f)
            NPC.rotation += 0.05f;
        else
            NPC.rotation = 0;
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;
        bool airborne = NPC.velocity.Y != 0;
        Rectangle source = airborne ? new Rectangle(0, 0, 42, 56) : new Rectangle(0, 58, 86, 28);
        Vector2 off = airborne ? Vector2.Zero : new Vector2(0, 7);
        Vector2 position = NPC.Center - screenPos + off;
 
        Main.EntitySpriteDraw(tex, position, source, drawColor, NPC.rotation, source.Size() / 2f, 1f, SpriteEffects.None, 0);
        return false;
    }
}
