using SubworldLibrary;

namespace BossForgiveness.Content.NPCs.Mechanics.MoonLord;

internal class MoonLordDomainBiome : ModBiome
{
    public override SceneEffectPriority Priority => SceneEffectPriority.Environment;
    public override int Music => Main.LocalPlayer.Center.Y / 16f < MoonLordPacificationSubworld.StillnessLayer + 20 && !MoonLordPacificationTracker.SpawnedAlready ? 0 : -1;

    public override bool IsBiomeActive(Player player) => SubworldSystem.Current is MoonLordPacificationSubworld;
}
