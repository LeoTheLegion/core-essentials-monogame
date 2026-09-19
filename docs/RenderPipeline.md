# Render Pipeline: Ordered Pre / Process / Post Passes with First-Class `Effect` Support

CoreEssentials renders a frame in three ordered stages, all orchestrated by `MainGame.Draw`:

1. **Pre pass** — optional setup actions that run after the backbuffer is cleared and before the scene draws (e.g. camera setup).
2. **Process pass** — the scene + GUI render. Each entity's `SpriteComponent` can carry a per-sprite shader (`Effect`), applied at `SpriteBatch.Begin`. Optionally the whole frame renders into a `RenderTarget2D` instead of straight to the backbuffer.
3. **Post pass** — one or more full-screen shader quads drawn after the scene + GUI (vignette, color grade, bloom, blur, …).

Everything is **opt-in**. When nothing is configured — no pre passes, no per-sprite effects, render-to-target off, no post passes — the frame renders exactly as it always has (straight to the backbuffer, one default batch), so there is no perf or rendering regression for games that don't use the feature.

```
GraphicsDevice.Clear
   └─ Pre pass        RenderPipeline.DrawPrePasses(...)          [opt-in]
      └─ Process pass  SceneManager.Draw + GUIManager.Draw        [always]
         │            (per-sprite Effect applied at Begin)
         │            (optionally into a RenderTarget2D)         [opt-in]
         └─ Post pass   RenderPipeline.DrawPostPasses(...)       [opt-in]
```

The pipeline state lives in the static `CoreEssentials.Rendering.RenderPipeline` class. It is global and game-thread only; scenes are loaded sequentially, so a scene that registers passes should remove them on unload (see the demo below).

---

## Per-Sprite `Effect`

A `SpriteComponent` can render its sprite through a MonoGame shader. In MonoGame an effect is applied at `SpriteBatch.Begin`, not per draw, so the entity system **groups entities by their effective effect** and opens a dedicated `Begin`/`End` for each distinct effect. Entities with no effect stay in the default batch — when *every* entity has no effect this is exactly one `Begin(null)`/`End`, i.e. byte-for-byte the previous behavior.

There are two ways to assign an effect:

| Property | Type | How it's set | Precedence |
|----------|------|-------------|------------|
| `Effect` | `Effect?` | Directly in code (e.g. a loader component). | Wins when non-null. |
| `EffectAsset` | `string` | A declarative XML property naming an effect asset, e.g. `"Effects/Glow"`. Resolved once in `OnAttach` via the `AssetManager`. | Used only when `Effect` is null. |

The effective shader is `SpriteComponent.EffectiveEffect`, which is what the render pipeline uses as its grouping key (exposed per-entity through `Entity.GetRenderEffect()`).

> **Tuning a shader's uniforms** — to control an effect's "vars" (e.g. make a glow weaker/stronger, animate an intensity), use the [`EffectParametersComponent`](ShaderUniforms.md). It owns and controls the uniforms from XML (`<EffectParameter>`) and code, and the pipeline pushes them onto the effect before each batch's `Begin`.

### Usage (code)

```csharp
// Assign a shader directly — e.g. in a component's OnAttach:
spriteComponent.Effect = AssetManager.LoadAsset<EffectAsset>("Effects/Glow").Effect;
```

### Usage (declarative XML)

This is the data-driven path — a plain string property, no loader component needed:

```xml
<Component Type="SpriteComponent">
    <Properties>
        <Property Name="Origin" Value="0.5,0.5" />
        <Property Name="SpriteAsset" Value="Sprites/ball_sprite.xml" />
        <!-- The per-sprite shader, resolved on attach and applied at Begin. -->
        <Property Name="EffectAsset" Value="Effects/Glow" />
    </Properties>
</Component>
```

If the named effect asset is missing, the failure is logged and swallowed — the sprite simply renders without a shader (same behavior as a missing `SpriteAsset`).

### The projection convention

A custom effect **owns its own screen→clip projection**: SpriteBatch's camera/transform matrix only feeds MonoGame's internal default `SpriteEffect`, so it does *not* apply to your shader. To keep shaders usable from XML without per-effect C# code, the pipeline auto-syncs a matrix parameter named **`Projection`** whenever an effect exposes one:

- For per-sprite effects, `EntitySystem` calls `RenderPipeline.SyncEffectProjection(...)` before each effect run.
- For post passes, `DrawPostPasses` does the same before drawing each pass.

The matrix is a screen-space orthographic projection built with the exact convention MonoGame's own `SpriteEffect` uses (`CreateOrthographicOffCenter(0, width, height, 0, 0, -1)`), rebuilt only when the viewport size changes. An effect that does **not** declare a `Projection` parameter is left untouched — so existing shaders keep working unchanged.

A minimal per-sprite shader following the convention (see `CoreEssentials.Playground/Content/Effects/Glow.fx`):

```hlsl
float4x4 Projection;          // auto-synced by the pipeline
sampler GlowTex;              // SpriteBatch binds the sprite texture to sampler slot 0

struct VSInput  { float3 Position : POSITION; float4 Color : COLOR; float2 UV : TEXCOORD0; };
struct VSOutput { float4 Position : POSITION; float4 Color : COLOR; float2 UV : TEXCOORD0; };

VSOutput MainVS(VSInput i)
{
    VSOutput o;
    o.Position = mul(float4(i.Position, 1.0), Projection); // screen px -> clip space
    o.Color = i.Color;
    o.UV = i.UV;
    return o;
}

float4 MainPS(VSOutput i) : COLOR
{
    float4 tex = tex2D(GlowTex, i.UV);   // sample the sprite texture
    // ... your per-pixel processing ...
    return tex * i.Color;
}

technique GlowTechnique
{
    pass P0
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader  = compile ps_4_0_level_9_1 MainPS();
    }
}
```

> **Vertex stream note:** SpriteBatch draws with `VertexPositionColorTexture`, whose layout is `Position` (Vector3), `Color` (packed BGRA8), `TextureCoordinate` (Vector2). Declare your VS input to match (`float3 Position : POSITION`, `float4 Color : COLOR`, `float2 UV : TEXCOORD0`) and promote the position to a 4-component vector before the matrix multiply.

> **Shader profiles:** on WindowsDX the content pipeline compiles effects at SM 4.0 level 9.1 — use `vs_4_0_level_9_1` / `ps_4_0_level_9_1`. The XNA-era keyword is `PixelShader` (not `FragmentShader`).

---

## Post Passes

A post pass draws a **full-screen quad** through an effect after the scene + GUI, in registration order. It's ideal for screen-space effects: vignette, kill-flash, color grade (additive), or bloom/blur/chromatic-aberration (sampling).

Register passes on the static pipeline:

```csharp
using CoreEssentials.Rendering;

// Additive overlay — drawn over the composited frame, no render target required.
RenderPipeline.AddPostPass(vignetteEffect);

// Sampling pass — reads the clean pre-composited scene frame (requires render-to-target).
RenderPipeline.AddPostPass(bloomEffect, samplesSceneTarget: true);
```

| Method | Description |
|--------|-------------|
| `AddPostPass(Effect effect, bool samplesSceneTarget = false, string? inputParameterName = null)` | Registers a pass. Order of registration = render order. Duplicate effects are allowed (drawn twice). |
| `RemovePostPass(Effect effect)` | Removes the first pass whose effect matches (by reference). Returns whether one was removed. |
| `ClearPostPasses()` | Removes all passes, restoring the default no-op state. |
| `PostPassCount` / `PostPasses` | Introspection of the registered passes. |

### Additive vs. sampling

- **Additive** (`samplesSceneTarget: false`, the default) — drawn over the current frame with alpha blending. No render target needed. Use for vignette, kill flash, color grade.
- **Sampling** (`samplesSceneTarget: true`) — the pipeline feeds the clean scene frame (the process-pass `RenderTarget2D`) into the shader's input-texture parameter before drawing. **Requires render-to-target to be enabled**; otherwise `DrawPostPasses` throws at draw time so the misconfiguration is loud rather than silently black. Use for bloom, blur, chromatic aberration, dissolve-the-whole-frame.

The `inputParameterName` (default `"Texture2D"`) names the shader parameter that receives the scene target for sampling passes. Per-pass parameters are set directly on the `Effect` by game code (e.g. `effect.Parameters["Intensity"].SetValue(0.5f)`), since the pipeline holds the effect by reference and re-draws it every frame.

---

## Render-to-`RenderTarget2D` Opt-In

Sampling post passes need the scene to be available as a texture. Enable render-to-target so the process pass draws into a full-screen `RenderTarget2D` instead of straight to the backbuffer:

```csharp
RenderPipeline.EnableRenderToTarget(true);
```

- When **disabled** (the default), the process pass renders straight to the backbuffer — exactly the original behavior.
- When **enabled**, the pipeline lazily creates a `RenderTarget2D` matching the viewport, draws the scene + GUI into it, then blits it to the backbuffer so additive overlays and the final image composite correctly. It is automatically recreated on resize and disposed by `ResetForTesting`.

Additive overlays do **not** require render-to-target; only sampling passes do.

---

## Pre Passes

An ordered set of setup actions that run after `GraphicsDevice.Clear` and before the scene draws — the natural home for camera setup or other per-frame state:

```csharp
RenderPipeline.AddPrePass(gameTime => { /* e.g. update camera */ });
```

| Method | Description |
|--------|-------------|
| `AddPrePass(Action<GameTime> action)` | Registers an ordered setup action (throws on null). |
| `RemovePrePass(Action<GameTime> action)` | Removes the first matching action. |
| `ClearPrePasses()` | Removes all pre passes. |

No-op when none are registered.

---

## Worked Example (Playground)

`CoreEssentials.Playground/Content/Scenes/RenderPipelineDemoScene.xml` demonstrates both features, fully data-driven:

- A **plain ball** (`SpriteComponent` with no effect) and an identical **glowing ball** whose `EffectAsset = "Effects/Glow"` — the only difference is that one string property. The vignette darkens the frame edges so the two are easy to compare.
- The glowing ball's glow **pulses weaker/stronger** over time: an [`EffectParametersComponent`](ShaderUniforms.md) owns the `GlowStrength` uniform (seeded from XML) and a `PulsingGlowComponent` drives it each frame with a ping-pong tween.
- A `RenderPipelineDemoComponent` shell registers an additive **vignette** post pass (`Effects/Vignette`) on attach and removes it on detach, so unloading the scene restores the pipeline's default state.

Run it with the smoke-run harness:

```powershell
./scripts/run-all-scenes.ps1 -Scenes RenderPipelineDemoScene.xml
```

---

## Related

- [Sprite System](SpriteSystem.md) — the unified sprite type and its batching model (effect is now an additional grouping key).
- [Z-Order Render Layers](ZOrderRenderLayers.md) — how entities are ordered before effect partitioning.
- [Scene Management](SceneManagement.md) — data-driven scenes, which is how the demo declares its effects.
