using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Animations;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace BossForgiveness.Content.NPCs.Vanilla;

[AutoloadHead]
public class DestroyerPacified : ModNPC, IAdditionalHoverboxes
{
    public override string Texture => $"Terraria/Images/NPC_{NPCID.TheDestroyer}";
    public override string HeadTexture => "Terraria/Images/NPC_Head_Boss_25";

    internal readonly List<Segment> segments = [];

    private ref float Timer => ref NPC.ai[0];
    private ref float DigTimer => ref NPC.ai[1];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 1;
        NPCID.Sets.IsTownPet[Type] = true;
        NPCID.Sets.MustAlwaysDraw[Type] = true;
        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.TheDestroyer);
        NPC.aiStyle = NPCAIStyleID.Passive;
        NPC.damage = 0;
        NPC.boss = false;
        NPC.townNPC = true;
        NPC.friendly = true;
        NPC.homeless = true;

        Music = -1;
        AnimationType = -1;
    }

    public override bool PreAI()
    {
        if (segments.Count == 0) // Populate segments if empty, saves on loading data
            PopulateSegments();

        NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
        NPC.breath = NPC.breathMax;

        foreach (var segment in segments)
            segment.Update();

        bool discussing = NPC.IsBeingTalkedTo();

        if (NPC.homeless)
        {
            if (!discussing)
            {
                NPC.velocity.X = MathF.Sin(Timer * 0.02f) * 8f;
                LaunchingBehaviour(60);
            }
            else
            {
                if (CanDig())
                    NPC.velocity *= 0.9f;
                else
                    NPC.velocity.Y += 0.1f;
            }
        }
        else
        {
            if (!discussing)
            {
                if (CanDig())
                {
                    NPC.velocity.X += MathF.Sign((NPC.homeTileX * 16) - NPC.Center.X) * 0.1f;
                    NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -12, 12);
                }

                if (NPC.Center.Y > NPC.homeTileY * 16)
                    LaunchingBehaviour(NPC.homeTileY < Main.worldSurface ? 60 : 40);
                else
                    NPC.velocity.Y += 0.1f;
            }
            else
            {
                if (CanDig())
                    NPC.velocity *= 0.9f;
                else
                    NPC.velocity.Y += 0.1f;
            }
        }

        return false;
    }

    private void LaunchingBehaviour(float timeDelay)
    {
        if (CanDig())
            DigTimer++;

        if (CanDig())
        {
            if (DigTimer > timeDelay)
            {
                NPC.velocity.Y -= 0.4f;
                Timer++;
            }
        }
        else
        {
            NPC.velocity.Y += 0.1f;
            DigTimer = 0;
        }

        NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -12, 12);
    }

    private bool CanDig() => Collision.SolidCollision(NPC.position, NPC.width, NPC.height) || Collision.WetCollision(NPC.position, NPC.width, NPC.height);

    private void PopulateSegments()
    {
        int count = NPC.GetDestroyerSegmentsCount();
        Segment parent = null;

        for (int i = 0; i < count; ++i)
        {
            var seg = new Segment(NPC.Center, parent ?? (Entity)NPC, i == count - 1);
            segments.Add(seg);
            parent = seg;
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

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        if (!NPC.IsABestiaryIconDummy && NPC.DistanceSQ(Main.LocalPlayer.Center) < MathF.Pow(80 * 80, 2))
        {
            foreach (var segment in segments)
                segment.Draw(screenPos);
        }

        var tex = TextureAssets.Npc[NPCID.TheDestroyer].Value;
        Main.EntitySpriteDraw(tex, NPC.Center - screenPos, null, drawColor, NPC.rotation, tex.Size() / 2f, 1.25f, SpriteEffects.None, 0);
        return false;
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Destroyer." + Main.rand.Next(8));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

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

            Size = ContentSamples.NpcsByNetId[_isTail ? NPCID.TheDestroyerTail : NPCID.TheDestroyerBody].Size;
        }

        public void Update()
        {
            float bodyLength = _parent is NPC ? 42 : 50;

            if (DistanceSQ(_parent.Center) > bodyLength * bodyLength)
                Center += this.SafeDirectionTo(_parent.Center) * (Distance(_parent.Center) - bodyLength);

            if (_parent.Center != Center)
                _rot = AngleTo(_parent.Center) + MathHelper.PiOver2;

            if (!Collision.SolidCollision(position, width, height))
                Lighting.AddLight(Center, new Vector3(0.3f, 0.1f, 0.05f));
        }

        public void Draw(Vector2 screenPos)
        {
            var tex = TextureAssets.Npc[_isTail ? NPCID.TheDestroyerTail : NPCID.TheDestroyerBody].Value;
            var glow = TextureAssets.Dest[_isTail ? 2 : 1].Value;
            var col = Lighting.GetColor(Center.ToTileCoordinates());
            Rectangle? src = _isTail ? null : tex.Frame(1, 2, 0, 0, 0, 0);
            Main.EntitySpriteDraw(tex, Center - screenPos, src, col, _rot, tex.Size() / (_isTail ? new Vector2(2) : new Vector2(2, 4)), 1.25f, SpriteEffects.None, 0);
            Main.EntitySpriteDraw(glow, Center - screenPos, src, Color.White with { A = 170 }, _rot, tex.Size() / (_isTail ? new Vector2(2) : new Vector2(2, 4)), 1.25f, SpriteEffects.None, 0);
        }
    }
}
