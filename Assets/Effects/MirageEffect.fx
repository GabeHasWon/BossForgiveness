float strength : register(c0);
float timer : register(c1);
float2 resolution : register(c2);
float effectStrength : register(c3);
float white : register(c4);
float3 fadeColor : register(c5);
float bleedEffect : register(c6);

sampler uImage0 : register(s0);

texture palette;

sampler2D paletteSampler = sampler_state
{
    Filter = MIN_MAG_MIP_LINEAR;
    Texture = <palette>;
    AddressU = wrap;
    AddressV = wrap;
};

float seed;

float4 Main(float2 coords : TEXCOORD0, float4 originalColor : COLOR0) : COLOR0
{
    float2 offset = coords * resolution;
    // coords = coords + float2(sin(coords.x + timer) / resolution.x, sin(coords.y + timer) / resolution.y);
    float2 uvAdj = lerp((offset + float2(sin(timer + offset.x * bleedEffect) * 8, cos(timer + offset.y * bleedEffect) * 8)) / resolution, coords, effectStrength);
    float4 color = tex2D(uImage0, uvAdj);
    float originalA = color.a;
    color = lerp(color, float4(fadeColor.r, fadeColor.g, fadeColor.b, originalA), white);
    
    return color;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 Main();
    }
};