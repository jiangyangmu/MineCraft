struct PS_IN
{
    float4 pos : SV_POSITION;
    float4 tex : TEXCOORD;
};

Texture2D texCube;
SamplerState samCube;

float4 main(PS_IN input) : SV_Target
{
    float4 texColor;
    texColor = texCube.Sample(samCube, input.tex.xy);
    return texColor * 0.5f;
}
