struct VS_IN
{
    float4 pos : POSITION;
    float4 col : COLOR;
};

struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 col : COLOR;
};

float4x4 m;

PS_IN VS(VS_IN input)
{
    PS_IN output;

    output.pos = mul(input.pos, m);
    output.col = input.col;

    return output;
}

float4 PS(PS_IN input) : SV_Target
{
    return input.col;
}
