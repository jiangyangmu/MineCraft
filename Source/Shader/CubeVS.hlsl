cbuffer cbPerObject
{
    float4x4 mvp;
};

struct VS_IN
{
    // per vertex
    float4 pos : POSITION;
    float4 tex : TEXCOORD;
    // per instance
    float4 trans : TRANSLATION;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 tex : TEXCOORD;
};

PS_IN main(VS_IN input)
{
    PS_IN output;

    output.pos = mul(input.pos + input.trans, mvp);
    output.tex = input.tex;

    return output;
}
