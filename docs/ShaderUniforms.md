# Shader Uniforms: Controlling Effect "Vars" from XML and Components

A per-sprite [Effect](RenderPipeline.md) is only half the story — you usually want to **tune its shader uniforms** ("vars"): make a glow weaker or stronger, shift a tint, animate an intensity over time. CoreEssentials gives shader vars first-class support through the `EffectParametersComponent`, which **owns and controls** the uniforms for its entity's effect.

The component holds named uniform values and the render pipeline pushes them onto the effect immediately before each sprite batch's `SpriteBatch.Begin`. Values can come from two sources:

- **Data-driven XML** — a base value declared in the scene with `<EffectParameter Name="X" Value="Y"/>`. No type hints needed; the target type is read from the effect's own parameter declaration at apply time.
- **Code** — typed setters (`SetFloat`, `SetColor`, …) that store a ready-typed value, typically driven each frame (e.g. by a [tween](Coroutines.md)).

A code-set value for a name always wins over an XML-set value for the same name (last write wins by call order).

```
EffectParametersComponent (owns the uniforms)
   │  SetFloat / SetColor / …   and/or   <EffectParameter> from XML
   ▼
render pipeline (per effect run, before SpriteBatch.Begin)
   └─ component.ApplyTo(effect)  →  effect.Parameters[name].SetValue(...)
```

---

## The `EffectParametersComponent`

Attach it **alongside** a `SpriteComponent` that declares an effect. It does not render anything itself — it just holds the values and hands them to the pipeline.

### Supported uniform types

| HLSL type | Parameter shape (MonoGame) | XML example | Code setter |
|-----------|---------------------------|-------------|-------------|
| `float`   | Scalar / Single           | `Value="1.5"` | `SetFloat(name, 1.5f)` |
| `int`     | Scalar / Int32            | `Value="42"` | `SetInt(name, 42)` |
| `bool`    | Scalar / Bool             | `Value="true"` | `SetBool(name, true)` |
| `float2`  | Vector / Single / cols=2  | `Value="1.5,2.5"` | `SetVector2(name, new Vector2(1.5f, 2.5f))` |
| `float3`  | Vector / Single / cols=3  | `Value="0,1,0"` | `SetVector3(name, new Vector3(0, 1, 0))` |
| `float4`  | Vector / Single / cols=4  | `Value="1,2,3,4"` or a color `"255,140,30"` | `SetVector4(…)` / `SetColor(name, Color.Coral)` |

A 4-component vector also accepts an `R,G,B` (or `R,G,B,A`) **color** in 0–255, since glow tints are commonly authored as colors but live in a `float4`.

### API

| Member | Description |
|--------|-------------|
| `SetFloat / SetInt / SetBool / SetColor / SetVector2 / SetVector3 / SetVector4 (string name, …)` | Store a typed uniform by name (code path). |
| `SetValue(string name, object? value)` | Store a boxed value of any supported type. |
| `Get<T>(string name)` | Read the current value as `T`. An XML (raw-string) value is parsed to `T` on demand. Returns `default(T)` when absent. |
| `InitializeFromStrings(IReadOnlyDictionary<string,string>? values)` | Seed raw string values from a data-driven scene (called automatically by the loader). |
| `ApplyTo(Effect? effect)` | Write every stored uniform onto the matching parameter of `effect`. Called by the pipeline — you normally don't call this yourself. |
| `Signature` | A canonical, order-stable string of the current values. Used as part of the render pipeline's batching key (see below). |
| `Values` | The current name → value map (read-only view). |
| `Clear()` | Remove all stored uniforms. |

### Usage (code)

```csharp
var params = entity.AddComponent<EffectParametersComponent>();
params.SetFloat("GlowStrength", 1.0f);   // base value, updated every frame below
params.SetColor("Tint", Color.OrangeRed);
```

---

## Data-Driven XML

Declare a base value inside the `<Component>` element that owns the uniforms — a sibling of `<Properties>`:

```xml
<Component Type="EffectParametersComponent">
    <EffectParameter Name="GlowStrength" Value="1.0" />
    <EffectParameter Name="Tint"         Value="255,140,30" />
</Component>
```

- `Name` is required and must match a parameter on the effect; `Value` is optional (defaults to empty).
- No type hints in XML — the value is parsed against the effect parameter's own declared shape at apply time.
- The loader resolves the component by short name (`EffectParametersComponent`) exactly like any other built-in component.

A scene that declares only this component renders a **fixed** glow (the base value). Add a controller component to animate it (below).

---

## How the Render Pipeline Uses It

The entity system partitions render order into contiguous runs that share the same **(effect, uniform signature)** pair:

- **Same effect + same values** → coalesce into one run (one `Begin`/`End`).
- **Same effect + different values** → split into separate runs so each gets its own `Begin` with its own uniforms.
- **No effect / no params component** → a single `(null, "")` run, i.e. exactly the previous single `Begin(null)`/`End` behavior (no regression).

Before each run's `SpriteBatch.Begin`, the pipeline applies that run's uniforms from its first entity's component (`ApplyTo`). Because a shared/cached `Effect` instance is written immediately before its own `Begin`, the last-written-before-`Begin` wins per run — so many entities can safely share one effect object while each run carries different values.

This is also why two sprites using the same shader but with different glow strengths render correctly: they land in different runs, each with its own `GlowStrength`.

---

## Worked Example: A Pulsing Glow (Tween-Driven)

The playground's [Render Pipeline demo](./RenderPipeline.md#worked-example-playground) shows a ball whose glow **pulses weaker/stronger** over time. The shader (`Effects/Glow.fx`) declares the uniform:

```hlsl
float4x4 Projection;   // auto-synced by the pipeline
float GlowStrength;    // owned + controlled by an EffectParametersComponent
sampler GlowTex;
// ...
float3 base = tex.rgb * input.Color.rgb;
float3 glow = base * 0.3 + tint * (0.55 + 0.45 * presence);
float3 c   = base + (glow - base) * GlowStrength;   // 0 → plain, 1 → full, >1 → hotter
return float4(c * a, a);
```

The scene wires three components onto the glowing ball:

```xml
<EntityDefinition Type="...GameObjectEntity" Id="glowBall">
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="EffectAsset" Value="Effects/Glow" />
                <!-- ... Origin, SpriteAsset ... -->
            </Properties>
        </Component>

        <!-- Owns GlowStrength. The XML value is the base (fixed glow with no controller). -->
        <Component Type="EffectParametersComponent">
            <EffectParameter Name="GlowStrength" Value="1.0" />
        </Component>

        <!-- Controls GlowStrength over time: a smooth ping-pong tween between min and max. -->
        <Component Type="CoreEssentials.Playground.Components.PulsingGlowComponent">
            <Properties>
                <Property Name="MinStrength" Value="0.25" />
                <Property Name="MaxStrength" Value="1.6" />
                <Property Name="Duration"    Value="1.4" />
            </Properties>
        </Component>
    </Components>
</EntityDefinition>
```

The controller does **not** own the uniform — it writes into the sibling `EffectParametersComponent` every frame. This is the "a component that owns the shader vars controls the vars" split: ownership and control are separate, so you can swap controllers (tween, input, AI) without touching the uniform plumbing.

```csharp
public class PulsingGlowComponent : EntityComponent
{
    public string ParameterName { get; set; } = "GlowStrength";
    public float MinStrength { get; set; } = 0.25f;
    public float MaxStrength { get; set; } = 1.6f;
    public float Duration { get; set; } = 1.4f;

    private TweenFloat? _tween;
    private EffectParametersComponent? _params;

    public override void OnAttach()
    {
        _params = Owner.GetRenderEffectParameters();      // the owning component
        if (_params == null) return;

        _tween = new TweenFloat(MinStrength, MaxStrength, Duration, t => 0.5f - (float)Math.Cos(t * Math.PI));
        _tween.Loop = true;
        _tween.Reverse = true;                            // ping-pong: min → max → min → ...
    }

    public override void Update(GameTime gameTime)
    {
        if (_params == null || _tween == null) return;

        _tween.Advance((float)gameTime.ElapsedGameTime.TotalSeconds);
        if (_tween.IsComplete) _tween.ToggleDirection();  // flip at each end of the sweep

        _params.SetFloat(ParameterName, _tween.GetValue());
    }
}
```

Run it with the smoke-run harness:

```powershell
./scripts/run-all-scenes.ps1 -Scenes RenderPipelineDemoScene.xml
```

---

## Related

- [Render Pipeline](RenderPipeline.md) — per-sprite `Effect` support, the projection convention, and post passes.
- [Sprite System](SpriteSystem.md) — the unified sprite type and its batching model (effect + uniforms are additional grouping keys).
- [Coroutines](Coroutines.md) / [Entity Tweening](EntityTweening.md) — tween primitives you can drive a uniform with.
- [Scene Management](SceneManagement.md) — data-driven scenes, which is how the demo declares its uniforms.
