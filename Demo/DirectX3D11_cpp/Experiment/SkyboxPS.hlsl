TextureCube texSkybox;
SamplerState samTriLinear;

struct PS_IN
{
    float4 pixPos : SV_POSITION;
    float4 texPos : POSITION;
};

float4 main(PS_IN input) : SV_Target
{
    return texSkybox.Sample(samTriLinear, input.texPos.xyz);
}
