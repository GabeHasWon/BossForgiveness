using System.IO;
using Terraria.ModLoader;

namespace BossForgiveness;

public class BossForgiveness : Mod
{
    public override void PostSetupContent() => NetEasy.NetEasy.Register(this);
    public override void HandlePacket(BinaryReader reader, int whoAmI) => NetEasy.NetEasy.HandleModule(reader, whoAmI);
}