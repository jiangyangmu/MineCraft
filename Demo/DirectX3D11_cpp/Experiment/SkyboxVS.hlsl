cbuffer cbPerObject
{
    float4x4 mvp;
    float4 cbPos;
};

struct VS_IN
{
    float4 pos : POSITION;
    float4 col : COLOR;
};

struct PS_IN
{
    float4 pixPos : SV_POSITION;
    float4 texPos : POSITION;
};

PS_IN main(VS_IN input)
{
    PS_IN output;

    output.pixPos = mul(input.pos + cbPos, mvp).xyww;
    output.texPos = input.pos;

    return output;
}
