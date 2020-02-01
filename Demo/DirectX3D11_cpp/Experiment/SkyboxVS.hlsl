cbuffer cbPerObject
{
    float4x4 mvp;
};

struct VS_IN
{
    float4 pos : POSITION;
    float4 col : COLOR;
};

struct PS_IN
{
    float4 posH : SV_POSITION;
    float4 posL : POSITION;
};

PS_IN main(VS_IN input)
{
    PS_IN output;

    output.posH = mul(input.pos, mvp).xyww;
    output.posL = input.pos;

    return output;
}
