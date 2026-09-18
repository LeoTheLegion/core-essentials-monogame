// Full-screen additive vignette post pass for the render-pipeline demo.
//
// Registered through RenderPipeline.AddPostPass (additive mode — no render target required). The
// pipeline draws a full-screen quad through this effect after the scene + GUI. Because it is an
// overlay, it only needs the screen-space UV to shape the vignette; the 1x1 white quad texture that
// SpriteBatch binds to sampler0 is ignored. Output is black with an alpha that rises toward the
// corners, so AlphaBlend darkens the frame edges while leaving the center untouched.
float4x4 Projection;

sampler VignetteTex;

struct VSInput
{
    float3 Position : POSITION;
    float4 Color : COLOR;
    float2 UV : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION;
    float2 UV : TEXCOORD0;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    // Promote the 3-component stream position to a full clip-space vector (w = 1, no perspective).
    output.Position = mul(float4(input.Position, 1.0), Projection);
    output.UV = input.UV;
    return output;
}

float4 MainPS(VSOutput input) : COLOR
{
    // Distance from screen center in UV space (center is 0.5, 0.5). Normalize so the corner is ~1.
    float2 centered = input.UV - 0.5;
    float dist = length(centered * 2.0);

    // Ease the falloff so the vignette stays subtle in the middle and only darkens the outer edges.
    float edge = saturate(dist - 0.55) / (1.0 - 0.55);
    float alpha = edge * 0.6;

    return float4(0.0, 0.0, 0.0, alpha);
}

technique VignetteTechnique
{
    pass VignettePass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MainPS();
    }
}
