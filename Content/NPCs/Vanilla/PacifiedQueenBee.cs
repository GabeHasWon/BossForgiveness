using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class PacifiedQueenBee : ModNPC
{
    private ref float Timer => ref NPC.ai[0];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.QueenBee];

        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.QueenBee);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.noTileCollide = true;
        NPC.netAlways = true;

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = 152 * ((int)(NPC.frameCounter / 3f % 8) + 4);
    }

    public override bool PreAI()
    {
        bool discussing = NPC.IsBeingTalkedTo();

        if (!discussing)
            Timer++;

        if (NPC.homeless)
        {
            int floor = NPC.GetFloor();

            if (!discussing)
            {
                NPC.velocity.Y = MathF.Sin(Timer * 0.4f) - ((NPC.Center.Y / 16 - floor + 20) * 0.25f);
                NPC.velocity.X = MathF.Sin(Timer * 0.05f) * 8;
            }
            else
                NPC.velocity *= 0.85f;
        }
        else
        {
            if (!discussing)
            {
                var rot = new Vector2(0, 300).RotatedBy(Timer * 0.05f) * new Vector2(1.5f, 0.35f);
                var home = new Vector2(NPC.homeTileX, NPC.homeTileY).ToWorldCoordinates() - new Vector2(0, 400) + rot;
                NPC.velocity += NPC.DirectionTo(home) * 0.7f;
                NPC.velocity = Vector2.Clamp(NPC.velocity, new(-12), new(12));
            }
            else
                NPC.velocity *= 0.85f;
        }

        NPC.direction = NPC.spriteDirection = Math.Sign(NPC.velocity.X);
        return false;
    }

    public override List<string> SetNPCNameList() => [Lang.GetNPCName(NPCID.QueenBee).Value];
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.QueenBee." + Main.rand.Next(5));
    public override ITownNPCProfile TownNPCProfile() => new QueenBeeProfile();

    public class QueenBeeProfile : ITownNPCProfile
    {
        public int RollVariation() => 0;
        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => TextureAssets.Npc[NPCID.QueenBee];
        public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("BossForgiveness/Content/NPCs/Vanilla/PacifiedQueenBee_Head");
    }
}
