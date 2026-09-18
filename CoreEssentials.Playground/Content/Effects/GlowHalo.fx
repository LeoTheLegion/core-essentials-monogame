// Procedural radial "radiating glow" halo for the render-pipeline demo.
//
// Applied to a *larger* sprite drawn behind the glowing ball (see RenderPipelineDemoScene.xml). A
// per-sprite Effect only runs over its own quad, so this shader paints a soft warm falloff from the
// center of its (enlarged) quad outward — producing a halo that radiates past the ball's edges. It
// ignores the bound texture entirely and derives everything from UV, so any sprite can serve as the
// carrier quad; only its size matters. The falloff reaches zero at the quad's edge, so there is no
// box artifact.
float4x4 Projection;

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
    // Distance from the quad center: 0 at center, ~1 at the edge of the square (corners > 1).
    float d = length(input.UV - 0.5) * 2.0;

    // Soft quadratic falloff: bright core fading to nothing at the edge -> a radiating halo.
    float t = saturate(1.0 - d);
    float intensity = t * t;

    // Warm glow color, scaled by intensity. Output is premultiplied (color already carries the
    // falloff) so AlphaBlend composites it cleanly over whatever is behind.
    float3 glow = float3(1.0, 0.52, 0.12) * intensity;

    return float4(glow, intensity);
}

technique GlowHaloTechnique
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader  = compile ps_4_0_level_9_1 MainPS();
    }
}
