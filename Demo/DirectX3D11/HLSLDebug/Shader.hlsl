struct VS_IN
{
    float3 pos : POSITION;
    float3 col : COLOR;
    float2 tex : TEXCOORD;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float3 col : COLOR;
    float2 tex : TEXCOORD;
};

float4x4 gViewProjMatrix;

Texture2D gTextureMap : register(t0);
Texture2D gTextureMapB : register(t2);
Texture2D gTextureMapC : register(t1);
SamplerState mySampler0;
//{
//    Filter = MIN_MAG_MIP_LINEAR;
//};

PS_IN VS(VS_IN input)
{
    PS_IN output;

    float4 pos = float4(input.pos, 1.0f);
    output.pos = mul(pos, gViewProjMatrix);
    output.col = input.col;
    output.tex = input.tex;

    return output;
}

float4 PS(PS_IN input) : SV_Target
{
    float4 texColor = gTextureMap.Sample(mySampler0, input.tex);
    float4 texColorB = gTextureMapB.Sample(mySampler0, input.tex);
    return
        0.1 * texColorB +
        0.1 * texColor +
        0.5 * float4(input.col, 1.0);
    // return float4(input.col, 1.0f);
}

float4 PS(PS_IN input) : SV_Target
{
    float4 texColor;
    if (input.pos.z > 0.97f)
        texColor = gTexture.Sample(gSampleType, input.tex);
    else
        texColor = gTexture2.Sample(gSampleType, input.tex);
    return 0.5f * texColor + 0.5f * float4(input.col, 1.0f);
}
