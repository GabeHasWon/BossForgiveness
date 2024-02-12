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
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class BoCPacified : ModNPC
{
    /// <summary>
    /// Wrapper to handle controlled Creeper enemies.
    /// </summary>
    /// <param name="npc">The NPC to control.</param>
    private class Creeper(NPC npc)
    {
        private readonly NPC _controlledNPC = npc;

        internal void Update(int brain)
        {
            _controlledNPC.active = true;
            int oldBoss = NPC.crimsonBoss;
            NPC.crimsonBoss = brain; // Needed for the creeper AI to work properly
            int oldNetmode = Main.netMode;
            Main.netMode = NetmodeID.MultiplayerClient; // Workaround for their "attack"

            _controlledNPC.ai[0] = 0;
            _controlledNPC.ai[1] = 0;
            _controlledNPC.UpdateNPC(0);
            _controlledNPC.velocity *= 0.98f;
            _controlledNPC.alpha = Main.npc[brain].alpha;

            Main.netMode = oldNetmode;
            NPC.crimsonBoss = oldBoss;
        }

        internal void Draw() => Main.instance.DrawNPCDirect(Main.spriteBatch, _controlledNPC, false, Main.screenPosition);
    }

    private ref float Timer => ref NPC.ai[0];
    private ref float IdleRotation => ref NPC.ai[1];
    private ref float IdleRotDir => ref NPC.ai[2];

    private readonly List<Creeper> _creepers = [];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 8;

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

        Music = -1;
        AnimationType = -1;
    }

    public override void FindFrame(int frameHeight)
    {
        NPC.frameCounter++;
        NPC.frame.Y = 182 * (int)(NPC.frameCounter / 8f % 4);
    }

    public override bool PreAI()
    {
        foreach (var item in _creepers)
            item.Update(NPC.whoAmI);

        NPC.rotation = NPC.velocity.X / 16f;

        if (NPC.homeless)
        {
            if (IdleRotDir == 0)
                IdleRotDir = 1f;

            if (NPC.IsBeingTalkedTo())
            {
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.8f, 0.05f);
                NPC.velocity *= 0.96f;
            }
            else
            {
                IdleRotation += MathF.Tau / 200f;
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.2f, 0.05f);
                float yVel = (((NPCUtils.GetFloor(NPC) - 10) * 16) - NPC.Center.Y) / 300f;
                NPC.velocity = new Vector2(0, 2).RotatedBy(IdleRotation * IdleRotDir) + new Vector2(0, yVel);
            }

            if (Math.Abs(IdleRotation) > MathF.Tau)
            {
                IdleRotation = 0;
                IdleRotDir *= -1;
            }
        }
        else
        {
            if (NPC.IsBeingTalkedTo())
            {
                NPC.Opacity = MathHelper.Lerp(NPC.Opacity, 0.8f, 0.05f);
                NPC.velocity *= 0.96f;
            }
            else
            {
                IdleRotation += 0.005f;
                var spinLoc = new Vector2(NPC.homeTileX, NPC.homeTileY) * 16 + new Vector2(500, 0).RotatedBy(IdleRotation);
                NPC.velocity = NPC.DirectionTo(spinLoc) * Math.Max((NPC.Distance(spinLoc) - 60) / 240f, 2f);
            }
        }


        return false;
    }

    public void BuildCreepers()
    {
        _creepers.Clear();

        for (int i = 0; i < Main.maxNPCs; ++i)
        {
            NPC npc = Main.npc[i];

            if (npc.CanBeChasedBy() && npc.type == NPCID.Creeper)
            {
                NPC newNPC = new() { position = npc.position, velocity = npc.velocity };
                newNPC.SetDefaults(NPCID.Creeper);
                _creepers.Add(new Creeper(newNPC));
                npc.active = false;
                npc.netUpdate = true;
            }
        }
    }

    public override void SaveData(TagCompound tag) => tag.Add(nameof(_creepers), _creepers.Count);

    public override void LoadData(TagCompound tag)
    {
        int count = tag.GetInt(nameof(_creepers));

        for (int i = 0; i < count; ++i)
        {
            NPC newNPC = new() { position = NPC.position, velocity = new Vector2(0, Main.rand.NextFloat(1, 4)).RotatedByRandom(MathF.Tau) };
            newNPC.SetDefaults(NPCID.Creeper);
            _creepers.Add(new Creeper(newNPC));
        }
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        foreach (var item in _creepers)
            item.Draw();

        return true;
    }

    public override List<string> SetNPCNameList() => [Lang.GetNPCName(NPCID.BrainofCthulhu).Value];
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.BoC." + Main.rand.Next(7));
    public override ITownNPCProfile TownNPCProfile() => new BoCProfile();

    public class BoCProfile : ITownNPCProfile
    {
        public int RollVariation() => 0;
        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => TextureAssets.Npc[NPCID.BrainofCthulhu];
        public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("BossForgiveness/Content/NPCs/Vanilla/BoCPacified_Head");
    }
}
