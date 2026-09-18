// Per-sprite glow effect for the render-pipeline demo.
//
// Applied to a SpriteComponent via its declarative EffectAsset (see RenderPipelineDemoScene.xml),
// this proves the data-driven per-sprite shader path: MonoGame applies an Effect at
// SpriteBatch.Begin, so the entity system groups entities by effect and opens a dedicated
// Begin/End for this shader while plain sprites keep the default batch.
//
// Convention (see docs/RenderPipeline.md): the render pipeline auto-syncs a `Projection`
// matrix whenever an effect exposes one, using the same orthographic convention MonoGame's own
// SpriteEffect uses. The sprite texture is bound by SpriteBatch to sampler slot 0, so we sample
// it explicitly from register(s0).
float4x4 Projection;

sampler GlowTex;

struct VSInput
{
    float3 Position : POSITION;
    float4 Color : COLOR;
    float2 UV : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION;
    float4 Color : COLOR;
    float2 UV : TEXCOORD0;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    // Promote the 3-component stream position to a full clip-space vector (w = 1, no perspective).
    output.Position = mul(float4(input.Position, 1.0), Projection);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}

float4 MainPS(VSOutput input) : COLOR
{
    float4 tex = tex2D(GlowTex, input.UV);

    // The sprite texture is premultiplied-alpha. Gate the final color by the source alpha so fully
    // transparent pixels stay exactly (0,0,0,0) — otherwise a constant glow term would bleed a faint
    // box around the sprite's bounding rectangle.
    float a = tex.a * input.Color.a;
    float presence = tex.a;                       // 0..1: how much of the sprite is at this pixel
    float3 base = tex.rgb * input.Color.rgb;      // original (premultiplied) color

    // Strong warm glow: blend hard toward a saturated orange and add an emissive lift so the sprite
    // reads as clearly "on fire" compared with its un-effected neighbors. Driven by `presence` so it
    // only affects real sprite content, never the transparent background.
    float3 tint = float3(1.0, 0.45, 0.08);
    float3 c = base * 0.3 + tint * (0.55 + 0.45 * presence);

    return float4(c * a, a);
}

technique GlowTechnique
{
    pass GlowPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MainPS();
    }
}
