//using Microsoft.Xna.Framework;
//using Terraria;
//using Terraria.ID;
//using Terraria.ModLoader;

//namespace BossForgiveness.Common.Commands;

//#if DEBUG
//internal class SpawnPinkyCommand : ModCommand
//{
//    public override string Command => "pinky";

//    public override CommandType Type => CommandType.Chat;

//    public override void Action(CommandCaller caller, string input, string[] args)
//    {
//        Point pos = Main.LocalPlayer.Center.ToPoint();
//        NPC.NewNPC(null, pos.X, pos.Y, NPCID.Pinky);
//    }
//}
//#endif