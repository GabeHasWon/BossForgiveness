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
public class PacifiedEoW : ModNPC
{
    private ref float Timer => ref NPC.ai[0];

    private readonly List<Segment> segments = new();

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;

        NPCID.Sets.IsTownPet[Type] = true;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.EaterofWorldsHead);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.hide = false;
        NPC.netAlways = true;

        Music = -1;
        AnimationType = -1;
    }

    public override bool PreAI()
    {
        Timer++;

        if (!NPC.IsBeingTalkedTo())
        {
            if (NPC.homeless)
            {
                NPC.velocity.X += MathF.Sin(Timer * 0.03f) * 0.15f;

                if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    NPC.velocity.Y -= 0.2f;
                else
                    NPC.velocity.Y += 0.2f;
            }
            else
            {
                if (NPC.homeTileX + (MathF.Sin(Timer * 0.03f) * 25) > NPC.Center.X / 16f)
                    NPC.velocity.X += 0.2f;
                else
                    NPC.velocity.X -= 0.2f;

                if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height) && NPC.homeTileY * 16 < NPC.Center.Y)
                    NPC.velocity.Y -= 0.2f;
                else
                    NPC.velocity.Y += 0.2f;
            }
        }
        else
        {
            NPC.velocity.X *= 0.94f;

            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                NPC.velocity.Y *= 0.94f;
            else
                NPC.velocity.Y += 0.2f;
        }

        NPC.velocity.X = Math.Clamp(NPC.velocity.X, -6, 6);
        NPC.velocity.Y = Math.Clamp(NPC.velocity.Y, -6, 6);
        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;

        foreach (var segment in segments)
            segment.Update();
        return false;
    }

    public override void FindFrame(int frameHeight) => NPC.frame.Y = 0;

    internal void SpawnBody(Span<Vector2> positions)
    {
        Segment lastSegment = null;

        for (int i = 0; i < positions.Length; ++i)
        {
            Entity parent = lastSegment ?? (Entity)NPC;
            Segment segment = new(positions[i], parent, i == positions.Length - 1);
            segments.Add(segment);
            lastSegment = segment;
        }
    }

    public override void SaveData(TagCompound tag) => tag.Add(nameof(segments), segments.Count);

    public override void LoadData(TagCompound tag)
    {
        int count = tag.GetInt(nameof(segments));
        Span<Vector2> positions = stackalloc Vector2[count];

        for (int i = 0; i < count; ++i)
            positions[i] = NPC.Center;

        segments.Clear();
        SpawnBody(positions);
    }

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        foreach (var segment in segments)
            segment.Draw(screenPos);

        var tex = TextureAssets.Npc[NPCID.EaterofWorldsHead].Value;
        Main.EntitySpriteDraw(tex, NPC.Center - screenPos, null, drawColor, NPC.rotation, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
        return false;
    }

    public override List<string> SetNPCNameList() => [Lang.GetNPCName(NPCID.EaterofWorldsHead).Value];
    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.EoW." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => new EoWProfile();

    public class EoWProfile : ITownNPCProfile
    {
        public int RollVariation() => 0;
        public string GetNameForVariant(NPC npc) => npc.getNewNPCName();
        public Asset<Texture2D> GetTextureNPCShouldUse(NPC npc) => TextureAssets.Npc[NPCID.EaterofWorldsHead];
        public int GetHeadTextureIndex(NPC npc) => ModContent.GetModHeadSlot("BossForgiveness/Content/NPCs/Vanilla/PacifiedEoW_Head");
    }

    public class Segment : Entity
    {
        private readonly Entity _parent;
        private readonly bool _isTail;

        private float _rot = 0;

        public Segment(Vector2 pos, Entity parent, bool isTail)
        {
            position = pos;
            _parent = parent;
            _isTail = isTail;
        }

        public void Update()
        {
            const float EoWLength = 22.5f;

            if (DistanceSQ(_parent.Center) > EoWLength * EoWLength)
                Center += DirectionTo(_parent.Center) * (Distance(_parent.Center) - EoWLength);

            if (_parent.Center != Center)
                _rot = AngleTo(_parent.Center) + MathHelper.PiOver2;
        }

        public void Draw(Vector2 screenPos)
        {
            var tex = TextureAssets.Npc[_isTail ? NPCID.EaterofWorldsTail : NPCID.EaterofWorldsBody].Value;
            var col = Lighting.GetColor(Center.ToTileCoordinates());
            Main.EntitySpriteDraw(tex, Center - screenPos, null, col, _rot, tex.Size() / 2f, 1f, SpriteEffects.None, 0);
        }
    }
}
