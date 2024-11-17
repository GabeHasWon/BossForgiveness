using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Vanilla.Enemies;

[AutoloadHead]
public class TimPacified : ModNPC
{
    public bool CanEnchant => EnchantTimer <= 0;

    private ref float Timer => ref NPC.ai[0];
    private ref float EnchantTimer => ref NPC.ai[1];
    private ref float EnchantAnim => ref NPC.ai[3];

    private int _buffPlayer = 0;

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;
        NPCID.Sets.NoTownNPCHappiness[Type] = true;

        this.HideFromBestiary();
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Tim);
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

    public override bool PreAI()
    {
        if (!NPC.IsBeingTalkedTo())
            Timer++;

        EnchantTimer--;
        EnchantAnim--;

        if (EnchantAnim > 0 && Main.rand.NextBool(2))
        {
            Vector2 vel = new Vector2(0, 4).RotatedByRandom(MathHelper.PiOver4);
            Dust.NewDustPerfect(NPC.Top - vel * 20, DustID.Shadowflame, vel * 1.8f, 100);

            if (EnchantAnim == 1)
            {
                for (int i = 0; i < 30; i++)
                {
                    vel = new Vector2(0, 1).RotatedByRandom(MathHelper.Pi);
                    Dust.NewDustPerfect(Main.player[_buffPlayer].Center, DustID.Shadowflame, vel * Main.rand.NextFloat(4, 10), 100);
                }

                int[] types = [BuffID.MagicPower, BuffID.ManaRegeneration, BuffID.Summoning];
                Main.LocalPlayer.AddBuff(Main.rand.Next(types), 60 * 60 * 4, false);
            }
        }

        if (Timer >= 650 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            Player nearest = Main.player[Player.FindClosest(NPC.position, NPC.width, NPC.height)];
            int targetTileX = (int)NPC.Center.X / 16;
            int targetTileY = (int)NPC.Center.Y / 16;

            if (!NPC.homeless)
            {
                targetTileX = NPC.homeTileX;
                targetTileY = NPC.homeTileY;
            }
            else if (nearest.active && !nearest.dead && nearest.DistanceSQ(NPC.Center) > 1200 * 1200)
            {
                targetTileX = (int)nearest.Center.X / 16;
                targetTileY = (int)nearest.Center.Y / 16;
            }

            Vector2 chosenTile = Vector2.Zero;

            if (NPC.AI_AttemptToFindTeleportSpot(ref chosenTile, targetTileX, targetTileY))
            {
                Timer = 0;
                
                TeleportEffect();

                NPC.position.X = chosenTile.X * 16f - NPC.width / 2f + 8f;
                NPC.position.Y = chosenTile.Y * 16f - NPC.height;

                TeleportEffect();
            }
        }

        return false;
    }

    private void TeleportEffect()
    {
        for (int i = 0; i < 25; ++i)
        {
            int dustSlot = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0f, 0f, 100, default, Main.rand.Next(1, 3));
            Dust dust = Main.dust[dustSlot];
            dust.velocity *= 3f;

            if (dust.scale > 1f)
                dust.noGravity = true;
        }
    }

    public override void FindFrame(int frameHeight) => NPC.frame.Y = EnchantAnim > 0 ? frameHeight : 0;

    public override void SetChatButtons(ref string button, ref string button2) => button = Language.GetTextValue("Mods.BossForgiveness.Dialogue.Tim.Enchant.Button");

    public override void OnChatButtonClicked(bool firstButton, ref string shopName)
    {
        if (firstButton)
        {
            if (CanEnchant)
            {
                _buffPlayer = Main.myPlayer;
                EnchantTimer = 60 * 60 * 15;
                EnchantAnim = 60;
                NPC.netUpdate = true;

                Main.npcChatText = Language.GetTextValue("Mods.BossForgiveness.Dialogue.Tim.Enchant." + Main.rand.Next(4));
            }
            else
                Main.npcChatText = Language.GetTextValue("Mods.BossForgiveness.Dialogue.Tim.RejectEnchant." + Main.rand.Next(4));
        }
    }

    public override string GetChat() => Language.GetTextValue("Mods.BossForgiveness.Dialogue.Tim." + Main.rand.Next(4));
    public override ITownNPCProfile TownNPCProfile() => this.DefaultProfile();

    public override void SaveData(TagCompound tag) => tag.Add("enchantTimer", EnchantTimer);
    public override void LoadData(TagCompound tag) => EnchantTimer = tag.GetFloat("enchantTimer");
}
