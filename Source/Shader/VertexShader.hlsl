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
    float4 pos : SV_POSITION; // x: [0, screen width) y: [0, screen height) z: depth: [0.0f, 1.0f]
    float4 col : COLOR;
};

PS_IN main(VS_IN input)
{
    PS_IN output;

    output.pos = mul(input.pos, mvp);
    output.col = input.col;

    return output;
}
