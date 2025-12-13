global using Terraria.ModLoader;
global using Terraria;
global using Microsoft.Xna.Framework;

using System.IO;

namespace BossForgiveness;

public class BossForgiveness : Mod
{
    public override void Load() => NPCUtils.NPCUtils.TryLoadBestiaryHelper();
    public override void Unload() => NPCUtils.NPCUtils.UnloadBestiaryHelper();
    public override void PostSetupContent() => NetEasy.NetEasy.Register(this);
    public override void HandlePacket(BinaryReader reader, int whoAmI) => NetEasy.NetEasy.HandleModule(reader, whoAmI);
}