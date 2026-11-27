using System.IO;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.Lunar;

internal class PortalAccessSystem : ModSystem
{
    public bool HasPortal = false;

    public override void SaveWorldData(TagCompound tag) => tag.Add("has", HasPortal);
    public override void LoadWorldData(TagCompound tag) => HasPortal = tag.GetBool("has");

    public override void NetSend(BinaryWriter writer) => writer.Write(HasPortal);
    public override void NetReceive(BinaryReader reader) => HasPortal = reader.ReadBoolean();
}
