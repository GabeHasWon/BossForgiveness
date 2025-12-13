namespace BossForgiveness.Common;

internal class StaticNoise : ILoadable
{
    public enum NoiseType
    {
        Perlin,
        WebCellular
    }

    private static FastNoiseLite _noise = null;
    private static FastNoiseLite _webNoise = null;

    public static float GetNoise(float x, float y, NoiseType type = NoiseType.Perlin) => (type switch
    {
        NoiseType.WebCellular => _webNoise,
        NoiseType.Perlin or _ => _noise,
    }).GetNoise(x, y);

    public void Load(Mod mod)
    {
        _noise = new(1200);

        _webNoise = new FastNoiseLite(8945789);
        _webNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        _webNoise.SetCellularJitter(1.040f);
        _webNoise.SetFractalType(FastNoiseLite.FractalType.Ridged);
    }

    public void Unload() => _noise = null;
}
