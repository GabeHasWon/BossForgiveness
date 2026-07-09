float strength : register(c0);
float timer : register(c1);
float2 resolution : register(c2);
float effectStrength : register(c3);
float white : register(c4);
float3 fadeColor : register(c5);
float bleedEffect : register(c6);
float skyNoiseSpeed : register(c7);
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

texture skyNoise;

sampler2D skyNoiseSampler = sampler_state
{
    Filter = MIN_MAG_MIP_LINEAR;
    Texture = <skyNoise>;
    AddressU = wrap;
    AddressV = wrap;
};

float luminosity(float3 rgb)
{
    return 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
}

float pixelize(float2 coords)
{
    coords.x -= coords.x % 2;
    coords.y -= coords.y % 2;
    return coords;
}

float4 Main(float2 coords : TEXCOORD0, float4 originalColor : COLOR0) : COLOR0
{
    float2 offset = coords * resolution;
    pixelize(offset);
    coords = offset / resolution;
    
    float3 noiseUv = tex2D(noiseSampler, coords * noiseMod + float2(timer, timer) * noiseSpeed).rgb;
    float2 uvAdj = lerp((offset + float2(sin(timer + noiseUv.x + offset.x * bleedEffect) * 8, cos(timer + noiseUv.y + offset.y * bleedEffect) * 8)) / resolution, coords, effectStrength);
    float4 color = tex2D(uImage0, uvAdj);
    float originalA = color.a;
    float4 realFade = float4(fadeColor.r, fadeColor.g, fadeColor.b, originalA);
    float3 sky = tex2D(skyNoiseSampler, coords * float2(timer, timer) + float2(sin(timer) * skyNoiseSpeed, sin(timer) * skyNoiseSpeed) * skyNoiseSpeed).rgb;
    float4 fade = lerp(realFade, float4(1 - sky, realFade.a), 1 - luminosity(sky.rgb));
    color = lerp(color, fade, white);
    
    return color;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_3_0 Main();
    }
};