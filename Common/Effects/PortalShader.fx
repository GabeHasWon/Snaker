sampler uImage0 : register(s0);

texture palette;

sampler2D paletteSampler = sampler_state
{
    Texture = <palette>;
    AddressU = wrap;
    AddressV = wrap;
};

float2 texSize;
float2 maskSize;
float timer;
float paletteSize;

float4 Main(float2 coords : TEXCOORD0, float4 ocolor : COLOR0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    float red = floor(color.r * 255 / 10 + (sin(timer * 0.2) * 0.2));
    float2 paletteCoords = float2((((red / paletteSize) + timer) % paletteSize) / paletteSize, 0);
    float4 col = tex2D(paletteSampler, paletteCoords);
    return col * color.a;
}

technique BasicColorDrawing
{
    pass WhiteSprite
    {
        PixelShader = compile ps_2_0 Main();
    }
};