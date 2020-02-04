struct PS_IN
{
    float4 pos : SV_POSITION; // x: [0, screen width) y: [0, screen height) z: depth: [0.0f, 1.0f]
    float4 col : COLOR;
};

float4 main(PS_IN input) : SV_Target
{
    return input.col;
}
