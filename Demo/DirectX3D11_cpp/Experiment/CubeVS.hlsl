cbuffer cbPerObject
{
    float4x4 mvp;
};

struct VS_IN
{
    // per vertex
    float4 pos  : POSITION;
    float4 col  : COLOR;
    // per instance
    float4 ipos : TEXCOORD1;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
};

PS_IN main(VS_IN input)
{
    PS_IN output;

    output.pos = mul(input.pos + input.ipos, mvp);
    output.col = input.col;

    return output;
}
