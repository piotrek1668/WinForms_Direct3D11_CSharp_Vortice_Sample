struct VSInput
{
    float4 inPosition : POSITION;
    float2 inTexCoord : TEXCOORD;
};

struct PSInput
{
    float4 outPosition : SV_POSITION;
    float2 outTexCoord : TEXCOORD;
};

Texture2D objTexture : TEXTURE : register(t0);
SamplerState objSamplerState : SAMPLER : register(s0);

PSInput VSMain(VSInput input) {
    PSInput result;
    result.outPosition = input.inPosition;
    result.outTexCoord = input.inTexCoord;
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    float3 pixelColor = objTexture.Sample(objSamplerState, input.outTexCoord);
    return float4(pixelColor, 1.0f);
}
