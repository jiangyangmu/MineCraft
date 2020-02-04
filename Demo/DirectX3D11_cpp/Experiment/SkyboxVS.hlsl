cbuffer cbPerObject
{
    float4x4 mvp;
    float4 cbPos;
};

struct VS_IN
{
    float4 pos  : POSITION;
    float4 norm : NORMAL;
};

struct PS_IN
{
    float4 pixPos : SV_POSITION;
    float4 norm   : NORMAL;
};

PS_IN main(VS_IN input)
{
    PS_IN output;

    output.pixPos = mul(input.pos + cbPos, mvp); // cube
    // output.pixPos = mul(input.pos, mvp).xyww; // sphere
    output.norm = input.norm;

    return output;
}
