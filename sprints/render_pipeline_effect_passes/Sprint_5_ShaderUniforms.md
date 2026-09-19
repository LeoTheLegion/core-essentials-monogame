# Sprint 5 — Shader Uniforms from XML & Code (with a Tween Showcase)

## Goal
Let users set an effect's shader **uniforms** ("vars") both from data-driven XML and from code,
through a dedicated component that **owns and controls** the vars. Make the values animatable with
the existing `TweenComponent` so a demo can show a glow that pulses weaker/stronger — no new tween
machinery needed.

## The crux (drives the design)
A per-sprite `Effect` is applied at `SpriteBatch.Begin`, and its uniforms are global state on that
effect instance. Two problems fall out of that:

1. **Shared-effect clobbering.** `AssetManager` caches one `Effect` per asset, so two sprites both
   using `Effects/Glow` share *one* effect object and therefore *one* set of uniforms. Today they'd
   land in a single Begin/End run (grouped by effect reference) and the last-written uniform wins for
   both — so "weaker vs stronger glow" on two balls would not work.

2. **Per-run uniform write.** Because we already open one `Begin(effect)/End` per distinct effect run,
   the natural place to push uniforms is **immediately before each run's `Begin`**, right after the
   existing `SyncEffectProjection`. Setting them there means a shared effect instance is safe: each run
   applies its own values in sequence.

**Design:** a new `EffectParametersComponent` owns the vars (typed bag + a canonical signature). The
render partition key becomes `(effect, paramSignature)` so sprites with the same effect but different
var values split into separate runs, while identical-var sprites still batch together. The render pass
applies the run's vars onto the effect right before `Begin`.

> This deliberately keeps uniforms on a *separate* component rather than bolting them onto
> `SpriteComponent`: it owns the vars, exposes a clean settable API for code/tweens, and leaves
> `SpriteComponent` focused on sprite + which effect.

## Changes
- **`SerializationUtils`**: add `Vector3` and `Vector4` to `ParseValue` (scalar → uniform, "x,y[,z,w]"
  → components), matching the existing `Vector2`/`Color` helpers.
- **New `EffectParametersComponent : EntityComponent`** (`.../Components/BuiltIn/Visuals/`):
  - Typed setters: `SetFloat`, `SetInt`, `SetBool`, `SetColor`, `SetVector2/3/4`, plus a generic
    `SetValue(name, object)`. A `Get<T>(name)` accessor and an `IReadOnlyDictionary<string,object> Values`.
  - `InitializeFromStrings(Dictionary<string,string>)` — eager path for XML-sourced values (stored as raw
    strings, parsed against the effect parameter type at apply time so XML needs no type hints).
  - `string Signature { get; }` — canonical, order-stable string of current `(name → value)` pairs; used
    as the batching key so identical-var sprites coalesce and different-var sprites split.
  - `void ApplyTo(Effect effect)` — writes each var onto the matching `effect.Parameters[name]`; skips (with
    a one-time warning) names the effect doesn't declare, so a typo never throws mid-render.
- **`Entity.GetRenderEffectParameters()`** — returns the owning `EffectParametersComponent?` (mirrors
  `GetRenderEffect`).
- **`SceneParser`**: accept `<EffectParameter Name="X" Value="Y"/>` as a child of `<Component>`; store into a
  new `Dictionary<string,string> EffectParameters` on `Prefab.ComponentDefinition`. Strictness preserved
  (unknown element/attribute → `FormatException`; missing `Name` → error).
- **`EntityPrefabLoader`**: when instantiating an `EffectParametersComponent`, call
  `InitializeFromStrings` with the parsed bag.
- **`EntitySystem`**: extend `PartitionByEffect`'s key to `(Effect?, string paramSignature)`; in
  `RenderEffectRuns`, after `SyncEffectProjection`, call the run's representative component's `ApplyTo(effect)`.
  Regression guard: entities with no effect *and* no params keep a single `(null, "")` run = one
  `Begin(null)/End`, byte-for-byte the previous behavior.
- **Playground showcase**: a ball whose glow strength pulses — an `EffectParametersComponent` (XML-declared
  base values) driven each frame by `TweenComponent.TweenToFloat(...)` → `.GetValue()` → `SetFloat(...)`.
  Demonstrates XML vars + code control + tweening in one entity. Update the demo scene.

## Tests
- `SerializationUtils`: Vector3/Vector4 parse (scalar, full tuple, malformed → zero / warning).
- `EffectParametersComponent`: setters store typed values; `Get<T>` round-trips; `Signature` is stable and
  order-independent, changes when a value changes; `ApplyTo` writes to matching effect params and skips
  unknown names without throwing (verified with a real compiled `Effect` or a stub); `InitializeFromStrings`
  parses lazily against the param type.
- `Entity.GetRenderEffectParameters()`: returns component, null when absent.
- `SceneParser`: `<EffectParameter>` parses into the bag; multiple params; missing `Name` throws; unknown
  child element still rejected; flat-attribute path unaffected.
- `EntityPrefabLoader`: parsed bag is applied to the component on instantiation.
- `PartitionByEffect`: no-param regression (single run); same effect + different vars → split; same effect +
  same vars → coalesce; interleaved distinct-var runs preserve order.

## Definition of done
- [x] Library + test project build clean.
- [x] New tests pass; existing batching/effect tests still green (no regression). Full suite green.
- [x] XML doc comments added for all new public members.
- [x] Playground showcase renders a pulsing glow (XML base value + tween-driven).
- [x] Docs updated: uniforms component, `<EffectParameter>` XML, and the tween showcase example; linked from `docs/README.md`.
- [x] Smoke-run harness passes for all scenes including the updated demo.
- [x] Committed on the feature branch (`feature/render-pipeline-effect-passes`).
