TextureCube texSkybox;
SamplerState samTriLinear;

struct PS_IN
{
    float4 posH : SV_POSITION;
    float4 posL : POSITION;
};

float4 main(PS_IN input) : SV_Target
{
    return texSkybox.Sample(samTriLinear, input.posL.xyz);
}
