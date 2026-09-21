# Shader Uniforms: Controlling a Shader's "Vars" from XML and Components

A per-sprite [Effect](RenderPipeline.md) is only half the story — you usually want to **tune its shader uniforms** ("vars"): make a glow weaker or stronger, shift a tint, animate an intensity over time. In CoreEssentials the shader and its vars live together on the **`ShaderComponent`**, which **owns and controls** both for its entity.

The `ShaderComponent` holds named uniform values and the render pipeline pushes them onto the effect immediately before each sprite batch's `SpriteBatch.Begin`. Values can come from two sources:

- **Data-driven XML** — a base value declared in the scene with `<EffectParameter Name="X" Value="Y"/>`. No type hints needed; the target type is read from the effect's own parameter declaration at apply time.
- **Code** — typed setters (`SetFloat`, `SetColor`, …) that store a ready-typed value, typically driven each frame (e.g. by a [tween](Coroutines.md)).

A code-set value for a name always wins over an XML-set value for the same name (last write wins by call order).

```
ShaderComponent (owns the effect AND its uniforms)
   │  SetFloat / SetColor / …   and/or   <EffectParameter> from XML
   ▼
render pipeline (per effect run, before SpriteBatch.Begin)
   └─ component.ApplyTo(effect)  →  effect.Parameters[name].SetValue(...)
```

---

## The `ShaderComponent`

A sprite's shader lives on a companion `ShaderComponent`, **not** on the `SpriteComponent`. A `SpriteComponent` guarantees such a sibling exists — at attach it looks for one and, if missing, auto-creates a basic one (no effect = the SpriteBatch default batch) while logging a warning. To set an effect or tune its vars you get the entity's `ShaderComponent`:

```csharp
var shader = entity.GetComponent<ShaderComponent>();
shader.EffectAsset = "Effects/Glow";      // or shader.Effect = ...; in code
shader.SetFloat("GlowStrength", 1.0f);
```

The component owns two things: the **effect** (see [Render Pipeline](RenderPipeline.md#per-sprite-effect) for `Effect` / `EffectAsset` / `EffectiveEffect`) and the **uniforms** documented below. It does not render anything itself — it just holds the values and hands them to the pipeline.

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
var shader = entity.GetComponent<ShaderComponent>();   // auto-created by the SpriteComponent if absent
shader.SetFloat("GlowStrength", 1.0f);                 // base value, updated every frame below
shader.SetColor("Tint", Color.OrangeRed);
```

---

## Data-Driven XML

Declare a base value inside the `<Component Type="ShaderComponent">` element — a sibling of `<Properties>`:

```xml
<Component Type="ShaderComponent">
    <Properties>
        <Property Name="EffectAsset" Value="Effects/Glow" />
    </Properties>
    <EffectParameter Name="GlowStrength" Value="1.0" />
    <EffectParameter Name="Tint"         Value="255,140,30" />
</Component>
```

- `Name` is required and must match a parameter on the effect; `Value` is optional (defaults to empty).
- No type hints in XML — the value is parsed against the effect parameter's own declared shape at apply time.
- The loader resolves the component by short name (`ShaderComponent`) exactly like any other built-in component.
- `<EffectParameter>` elements are **siblings of `<Properties>`**, not nested inside it, and a `ShaderComponent` may declare only uniforms with no `<Properties>` element at all.
- Data-driven base values are honored whether the component comes from a **prefab template** (`<Prefab>`) or an **inline scene entity** — both parse paths collect `<EffectParameter>` and seed them onto the component before attach.

A scene that declares only this component renders a **fixed** glow (the base value). Add a controller component to animate it (below). A sprite with **no** `ShaderComponent` at all gets an auto-created basic one (no effect) — the pipeline then logs a warning reminding you to declare one in XML.

---

## How the Render Pipeline Uses It

The entity system partitions render order into contiguous runs that share the same **(effect, uniform signature)** pair:

- **Same effect + same values** → coalesce into one run (one `Begin`/`End`).
- **Same effect + different values** → split into separate runs so each gets its own `Begin` with its own uniforms.
- **No effect / no vars** → a single `(null, "")` run, i.e. exactly the previous single `Begin(null)`/`End` behavior (no regression).

Before each run's `SpriteBatch.Begin`, the pipeline applies that run's uniforms from its first entity's `ShaderComponent` (`ApplyTo`). Because a shared/cached `Effect` instance is written immediately before its own `Begin`, the last-written-before-`Begin` wins per run — so many entities can safely share one effect object while each run carries different values.

This is also why two sprites using the same shader but with different glow strengths render correctly: they land in different runs, each with its own `GlowStrength`.

---

## Worked Example: A Pulsing Glow (Tween-Driven)

The playground's [Render Pipeline demo](./RenderPipeline.md#worked-example-playground) shows a ball whose glow **pulses weaker/stronger** over time. The shader (`Effects/Glow.fx`) declares the uniform:

```hlsl
float4x4 Projection;   // auto-synced by the pipeline
float GlowStrength;    // owned + controlled by the ShaderComponent
sampler GlowTex;
// ...
float3 base = tex.rgb * input.Color.rgb;
float3 glow = base * 0.3 + tint * (0.55 + 0.45 * presence);
float3 c   = base + (glow - base) * GlowStrength;   // 0 → plain, 1 → full, >1 → hotter
return float4(c * a, a);
```

The scene pairs the glowing ball's `SpriteComponent` with a `ShaderComponent` (which owns both the effect and the uniform) and a controller:

```xml
<EntityDefinition Type="...GameObjectEntity" Id="glowBall">
    <Components>
        <Component Type="SpriteComponent">
            <Properties>
                <Property Name="Origin" Value="0.5,0.5" />
                <Property Name="SpriteAsset" Value="Sprites/ball_sprite.xml" />
            </Properties>
        </Component>

        <!-- Owns the shader: the effect (EffectAsset) AND the GlowStrength uniform. -->
        <Component Type="ShaderComponent">
            <Properties>
                <Property Name="EffectAsset" Value="Effects/Glow" />
            </Properties>
            <EffectParameter Name="GlowStrength" Value="1.0" />   <!-- base (fixed glow with no controller) -->
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

The controller does **not** own the uniform — it writes into the sibling `ShaderComponent` every frame. This is the "a component that owns the shader vars controls the vars" split: ownership and control are separate, so you can swap controllers (tween, input, AI) without touching the uniform plumbing.

```csharp
public class PulsingGlowComponent : EntityComponent
{
    public string ParameterName { get; set; } = "GlowStrength";
    public float MinStrength { get; set; } = 0.25f;
    public float MaxStrength { get; set; } = 1.6f;
    public float Duration { get; set; } = 1.4f;

    private TweenFloat? _tween;
    private ShaderComponent? _shader;

    public override void OnAttach()
    {
        _shader = Owner.GetComponent<ShaderComponent>();   // the owning component
        if (_shader == null) return;

        _tween = new TweenFloat(MinStrength, MaxStrength, Duration, t => 0.5f - (float)Math.Cos(t * Math.PI));
        _tween.Loop = true;
        _tween.Reverse = true;                             // ping-pong: min → max → min → ...
    }

    public override void Update(GameTime gameTime)
    {
        if (_shader == null || _tween == null) return;

        _tween.Advance((float)gameTime.ElapsedGameTime.TotalSeconds);
        if (_tween.IsComplete) _tween.ToggleDirection();   // flip at each end of the sweep

        _shader.SetFloat(ParameterName, _tween.GetValue());
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
- [Sprite System](SpriteSystem.md) — the unified sprite type and its batching model (the shader is an additional grouping key).
- [Coroutines](Coroutines.md) / [Entity Tweening](EntityTweening.md) — tween primitives you can drive a uniform with.
- [Scene Management](SceneManagement.md) — data-driven scenes, which is how the demo declares its uniforms.
