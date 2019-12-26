struct VS_IN
{
    float3 pos : POSITION;
    float3 col : COLOR;
    float2 tex : TEXCOORD;
};

struct PS_IN
{
    float4 pos : SV_POSITION; // x: [0, screen width) y: [0, screen height) z: depth: [0.0f, 1.0f]
    float3 col : COLOR;
    float2 tex : TEXCOORD;
};

float4x4 gViewProjMatrix;

PS_IN VS(VS_IN input)
{
    PS_IN output;

    float4 pos = float4(input.pos, 1.0f);
    output.pos = mul(pos, gViewProjMatrix);
    output.col = input.col;
    output.tex = input.tex;

    return output;
}

Texture2D gTexture;
SamplerState gSampleType;

float4 PS_Texture_Block(PS_IN input) : SV_Target
{
    float4 texColor;
    texColor = gTexture.Sample(gSampleType, input.tex);
    // pickable
    return texColor * 0.5f + float4(input.col, 0.1f) * 0.5f;
}

float4 PS_Transparent_Block(PS_IN input) : SV_Target
{
    float4 texColor;
    texColor = gTexture.Sample(gSampleType, input.tex);
    // pickable
    if (input.col.x != 0.0f && input.col.y == 0.0f && input.col.z == 0.0f)
    {
        texColor.xyz = texColor.xyz * 0.5f + input.col * 0.5f;
    }
    return texColor;
}

float4 PS_Liquid_Block(PS_IN input) : SV_Target
{
    // use input.col.x to control UV shift.
    float u = input.tex.x + input.col.x;
    float v = input.tex.y + input.col.x;
    
    if (u > 1.0f) u -= 1.0f;
    if (v > 1.0f) v -= 1.0f;
    
    float4 texColor = gTexture.Sample(gSampleType, float2(u,v));
    texColor.w = 0.5f;

    return texColor;
}

float4 PS_Line_Block(PS_IN input) : SV_Target
{
    return float4(input.col, 0.5f);
}
