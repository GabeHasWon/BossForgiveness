using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class PacifiedEoW : ModNPC, IAdditionalHoverboxes
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.EaterofWorldsHead}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_2";

    private ref float Timer => ref NPC.ai[0];
    private ref float NetTimer => ref NPC.ai[1];

    private readonly List<Segment> segments = new();

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;

        NPCID.Sets.IsTownPet[Type] = true;

        this.HideFromBestiary();
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
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override bool PreAI()
    {
        Timer++;

        if (NetTimer % 300 == 0 && CanDig())
            SoundEngine.PlaySound(SoundID.WormDigQuiet with { Volume = 0.1f, Pitch = -0.05f }, NPC.Center);

        if (NetTimer >= 600)
        {
            NPC.netUpdate = true;
            NetTimer = 0;
        }

        if (!NPC.IsBeingTalkedTo())
        {
            if (NPC.homeless)
            {
                NPC.velocity.X += MathF.Sin(Timer * 0.03f) * 0.15f;

                if (CanDig())
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

                if (CanDig() && NPC.homeTileY * 16 < NPC.Center.Y)
                    NPC.velocity.Y -= 0.2f;
                else
                    NPC.velocity.Y += 0.2f;
            }
        }
        else
        {
            NPC.velocity.X *= 0.94f;

            if (CanDig())
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

    private bool CanDig() => Collision.SolidCollision(NPC.position, NPC.width, NPC.height) || Collision.WetCollision(NPC.position, NPC.width, NPC.height);

    public override void FindFrame(int frameHeight) => NPC.frame.Y = 0;

    internal void SpawnBody(List<Vector2> positions)
    {
        Segment lastSegment = null;

        for (int i = 0; i < positions.Count; ++i)
        {
            Entity parent = lastSegment ?? (Entity)NPC;
            Segment segment = new(positions[i], parent, i == positions.Count - 1, NPC.scale);
            segments.Add(segment);
            lastSegment = segment;
        }
    }

    public List<Hoverbox> GetAdditionalHoverboxes()
    {
        List<Hoverbox> list = new(segments.Count);

        foreach (var item in segments)
            list.Add(new(NPC.whoAmI, item.Hitbox, null));

        return list;
    }

    public override void SetChatButtons(ref string button, ref string button2) => button = "";

    public override void SaveData(TagCompound tag) => tag.Add(nameof(segments), segments.Count);

    public override void LoadData(TagCompound tag)
    {
        int count = tag.GetInt(nameof(segments));
        List<Vector2> positions = new(count);

        for (int i = 0; i < count; ++i)
            positions.Add(NPC.Center);

        segments.Clear();
        SpawnBody(positions);
    }

    public override void SendExtraAI(BinaryWriter writer) => writer.Write((byte)segments.Count);

    public override void ReceiveExtraAI(BinaryReader reader)
    {
        int count = reader.ReadByte();

        if (segments.Count == count)
            return;

        List<Vector2> positions = new(count);

        for (int i = 0; i < count; ++i)
            positions.Add(NPC.Center);

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

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.EoW." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public class Segment : Entity
    {
        private readonly Entity _parent;
        private readonly bool _isTail;

        private float _rot = 0;
        private float _scale = 0f;

        public Segment(Vector2 pos, Entity parent, bool isTail, float scale)
        {
            position = pos;
            _parent = parent;
            _isTail = isTail;
            _scale = scale;

            Size = new Vector2(40, 40);
        }

        public void Update()
        {
            const float EoWLength = 40f;

            if (DistanceSQ(_parent.Center) > EoWLength * EoWLength)
                Center += this.SafeDirectionTo(_parent.Center) * (Distance(_parent.Center) - EoWLength);

            if (_parent.Center != Center)
                _rot = AngleTo(_parent.Center) + MathHelper.PiOver2;
        }

        public void Draw(Vector2 screenPos)
        {
            var tex = TextureAssets.Npc[_isTail ? NPCID.EaterofWorldsTail : NPCID.EaterofWorldsBody].Value;
            var col = Lighting.GetColor(Center.ToTileCoordinates());
            Main.EntitySpriteDraw(tex, Center - screenPos, null, col, _rot, tex.Size() / 2f, _scale, SpriteEffects.None, 0);
        }
    }
}
