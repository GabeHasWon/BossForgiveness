using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace BossForgiveness.Content.NPCs.Mechanics.WoF;

internal class GuideLocketSystem : ModSystem
{
    public bool locketGiven = false;

    public override void SaveWorldData(TagCompound tag) => tag.Add(nameof(locketGiven), locketGiven);
    public override void LoadWorldData(TagCompound tag) => locketGiven = tag.GetBool(nameof(locketGiven));
}
