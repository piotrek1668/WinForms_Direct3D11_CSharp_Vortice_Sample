struct VSInput
{
    float4 Position : POSITION;
    float4 Color : COLOR;
};

struct PSInput
{
    float4 Position : SV_POSITION;  // interpolated vertex position (system value)
    float4 Color : COLOR;           // interpolated diffuse color
};

cbuffer ModelViewProjectionConstantBuffer : register(b0)
{
    matrix mWorld;                  // world matrix for object
    matrix mView;                   // view matrix
    matrix mProjection;             // projection matrix
};

PSInput VSMain(VSInput input) {     // VSMain is the default function name (can be also named main or somethin else...)
    
    PSInput result;
    
    // Transform the position from object space to homogeneous projection space
    float4 pos = input.Position;
    pos = mul(pos, mWorld);
    pos = mul(pos, mView);
    pos = mul(pos, mProjection);
    result.Position = pos;
    
    // Just pass through the color data
    result.Color = input.Color;
    
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET {
    return input.Color;
}
