float strength : register(c0);
float timer : register(c1);
float2 resolution : register(c2);
float effectStrength : register(c3);
float white : register(c4);
float3 fadeColor : register(c5);
float bleedEffect : register(c6);
float sineStrength : register(c7);
float sineStrengthTotal : register(c8);
float alphaStrength : register(c9);
float alphaSize : register(c10);
float2 noiseMod : register(c11);
float2 noiseSpeed : register(c12);
float2 noiseZoom : register(c13);

sampler uImage0 : register(s0);

texture noise;

sampler2D noiseSampler = sampler_state
{
    Filter = MIN_MAG_MIP_LINEAR;
    Texture = <noise>;
    AddressU = wrap;
    AddressV = wrap;
};

float luminosity(float3 rgb)
{
    return 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
}

float4 Main(float2 coords : TEXCOORD0, float4 originalColor : COLOR0) : COLOR0
{
    float2 offset = coords * resolution;
    // coords = coords + float2(sin(coords.x + timer) / resolution.x, sin(coords.y + timer) / resolution.y);
    float2 uvAdj = lerp((offset + float2(sin(timer + offset.x * bleedEffect) * 8, cos(timer + offset.y * bleedEffect) * 8)) / resolution, coords, effectStrength);
    float4 color = tex2D(uImage0, uvAdj + float2(sin(timer + coords.x * sineStrength), sin(timer + coords.y * sineStrength)) * sineStrengthTotal);
    float originalA = color.a;
    float2 noiseUv = tex2D(noiseSampler, offset * noiseMod + float2(timer, timer) * noiseSpeed).rb;
    float noise = tex2D(noiseSampler, noiseUv * float2(1, 1.5) * noiseZoom + float2(timer, timer) * noiseSpeed).rb * luminosity(color.rgb);
    color = lerp(color, float4(fadeColor.r, fadeColor.g, fadeColor.b, originalA * (noise * alphaStrength)), white);
    
    return color;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_3_0 Main();
    }
};