struct VSInput
{
    float4 Position : POSITION;
    float4 Color : COLOR;
};

struct PSInput
{
    float4 Position : SV_POSITION; // interpolated vertex position (system value)
    float4 Color : COLOR; // interpolated diffuse color
};

cbuffer params : register(b1) {
    float4x4 viewProjection2;
};

PSInput VSMain(VSInput input) { // VSMain is the default function name (can be also named main or somethin else...)
    PSInput result;
    result.Position = mul(viewProjection2, input.Position);
    result.Color = input.Color;
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET {
    return input.Color;
}
