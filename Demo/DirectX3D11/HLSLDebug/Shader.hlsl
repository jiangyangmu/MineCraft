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

Texture2D gTextureMap;
//SamplerState mySampler0
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
    // float4 texColor = gTextureMap.Sample(mySampler0, input.tex);
    // return 0.5 * texColor + 0.5 * input.col;
    return float4(input.col, 1.0f);
}
