# Sprint 6 — `ShaderComponent`: Unity-style shader ownership (effect + vars), component-driven rendering

## Goal

Consolidate all shader ownership into a single, Unity-style **`ShaderComponent`** and make rendering
fully **component-driven**:

- The **effect and its asset move off `SpriteComponent`** and live on `ShaderComponent`.
- `ShaderComponent` also **owns and controls the shader vars** (absorbing today's
  `EffectParametersComponent`) — so "to change a var, get the shader component and set it" is literally true.
- The **sprite renderer (`SpriteComponent`) requires a `ShaderComponent`**: at start it looks for one and,
  if missing, **auto-creates a basic one whose effect is `null`** (i.e. today's default SpriteBatch path).
- **`Entity` stops exposing render methods** — `GetRenderEffect()` / `GetRenderEffectParameters()` are removed.
  The render pipeline resolves the shader by looking up the component directly.

Net result: one place owns "the shader" (effect + vars), the sprite renderer just draws its quad and leans
on the sibling `ShaderComponent`, and no render-specific knowledge lives on `Entity`.

## The crux (why this shape)

Today the shader is split across three homes, which is confusing and fights the component model:

| Concern | Today's home | Problem |
|---------|--------------|---------|
| Effect + asset + resolution | `SpriteComponent` (`Effect`, `EffectAsset`, `EffectiveEffect`) | The *sprite* renderer owns *shader* state. |
| Shader vars (uniforms) | `EffectParametersComponent` | A second component that only makes sense next to the effect. |
| "Which effect / which vars does this entity render with?" | `Entity.GetRenderEffect()` / `GetRenderEffectParameters()` | Render logic leaks onto the base `Entity`. |

The fix is to collapse these into **one `ShaderComponent`** (the Unity "material" role: shader ref + its
properties) and move the *lookup* out of `Entity` into the render pipeline, which already operates on a list
of entities and can simply do `entity.GetComponent<ShaderComponent>()`.

> This is deliberately a **merge**, not a rename of one component: the user-facing rule "get the shader
> component to set a var" only holds if the vars live on the same component as the effect.

### The auto-create hazard (must design around)

Scene/prefab instantiation runs inside a **deferred-attach window**: `Entity.BeginDeferringComponentAttach()`
→ components are stored without firing `OnAttach` → `Entity.AttachPendingComponents()` fires every
`OnAttach` in a single `foreach` over `_components.Values`. If `SpriteComponent.OnAttach` naively calls
`Owner.AddComponent(new ShaderComponent())` during that window, it mutates the dictionary **mid-enumeration**
→ `InvalidOperationException: Collection was modified`.

So the "guarantee a sibling `ShaderComponent` exists" step must be an **idempotent ensure** that is safe on
both paths:

- **Code path** (direct `AddComponent`, no deferred window): `SpriteComponent.OnAttach` may create it inline.
- **Prefab/scene path** (deferred window): the creation is deferred to the loader's **finish pass**, which runs
  *after* `AttachPendingComponents()` — i.e. outside the enumeration.

Both paths guard with `TryGetComponent<ShaderComponent>` first, so a user-declared `ShaderComponent` is never
double-created and the auto-create only fills the gap (with a null effect).

#### Lifecycle finding — where the check actually goes

There is **no per-component "XML attach complete" hook** (`EntityComponent` only exposes `OnAttach`, `OnDetach`,
`OnApplicationPause`, `Update`, `LateUpdate`). Two ordering facts pin down the correct check area:

1. In the prefab path, `OnStart()` runs **before** components attach (`BuildSubtree` opens the deferred window →
   calls `entity.OnStart()` → builds children → *then* `AttachPreOrder` creates components and fires `OnAttach`).
   So `OnStart` is **not** a valid check area — siblings may not exist yet.
2. By the time `SpriteComponent.OnAttach` fires, every XML-declared sibling is already in `_components` (all
   created in `ApplyComponentDefinition` before `AttachPendingComponents()` runs). So `OnAttach` can **read**
   siblings safely — but it fires *inside* the `foreach (_components.Values)` loop, so it **cannot add** a new
   component there.

| Path | Safe "check + create" moment | Why |
|------|------------------------------|-----|
| **Code** (`AddComponent` directly) | `SpriteComponent.OnAttach`, inline | Not deferring → adding is safe. |
| **XML/prefab/scene** | A finish pass right after `entity.AttachPendingComponents()` in the loader | All XML siblings present *and* outside the enumeration → adding is safe. |

On the **XML/prefab path**, when we auto-create a missing `ShaderComponent`, we log a **warning** telling the
author to declare `<Component Type="ShaderComponent">` in XML — that is the moment we know it was absent from
the data. The code path does not warn (auto-create there is an expected convenience).

## Changes

### Core library

- **New `ShaderComponent : EntityComponent`** (`.../Components/BuiltIn/Visuals/ShaderComponent.cs`) — the single
  shader owner. It carries, merged from the two old homes:
  - *Effect* (from `SpriteComponent`): `Effect? Effect`, `string EffectAsset`, resolved `_resolvedEffect`, and
    `EffectiveEffect => Effect ?? _resolvedEffect`. `OnAttach` resolves `EffectAsset` via the `AssetManager`
    (explicit `Effect` wins; failures logged + swallowed); `OnDetach` clears the resolved effect.
  - *Vars* (from `EffectParametersComponent`): the `_values` bag, typed setters (`SetFloat/SetInt/SetBool/
    SetColor/SetVector2/3/4`, `SetValue`), `Get<T>`, order-stable `Signature`, `InitializeFromStrings`,
    `ApplyTo(Effect)`, `Values`, `Clear`.
  - A **basic/default shader is `Effect == null`** → `EffectiveEffect` is null → the entity renders in the
    default SpriteBatch batch (byte-for-byte today's no-shader behavior).
- **Delete `EffectParametersComponent.cs`** — fully absorbed into `ShaderComponent`.
- **`SpriteComponent`**: remove `Effect`, `EffectAsset`, `_resolvedEffect`, `EffectiveEffect`, and the effect
  resolution in `OnAttach`/`OnDetach`. Add an idempotent **ensure-step** guaranteeing a sibling `ShaderComponent`
  (see hazard above): inline when not deferring, otherwise flagged for the loader finish pass.
- **`Entity`**: remove `GetRenderEffect()` and `GetRenderEffectParameters()`. No replacement render accessors —
  rendering is component-driven.
- **`EntitySystem`** (`PartitionByEffect`, `RenderEffectRuns`): resolve the shader via
  `entity.GetComponent<ShaderComponent>()` instead of the removed Entity methods:
  - partition key = `(shader?.EffectiveEffect, shader?.Signature ?? "")`;
  - before each run's `Begin`: `run.Entities[0].GetComponent<ShaderComponent>()?.ApplyTo(run.Effect)`.
- **`EntityPrefabLoader`**:
  - `ApplyEffectParameters` now targets `ShaderComponent` (was `EffectParametersComponent`) — the parsed
    `<EffectParameter>` bag is still seeded via `InitializeFromStrings`.
  - Add the finish-pass ensure: after `AttachPendingComponents()`, for each instantiated `SpriteComponent` with
    no sibling `ShaderComponent`, add a basic one (null effect) and attach it.

### Data-driven XML (no schema change)

- `<EffectParameter Name Value/>` remains a child of `<Component>`; the **target component is now
  `ShaderComponent`** instead of `EffectParametersComponent`. The scene declares the shader explicitly:
  ```xml
  <Component Type="SpriteComponent"> ... </Component>          <!-- no EffectAsset anymore -->
  <Component Type="ShaderComponent">
      <Property Name="EffectAsset" Value="Effects/Glow" />
      <EffectParameter Name="GlowStrength" Value="1.0" />
  </Component>
  ```
- A sprite with **only** a `SpriteComponent` gets an auto-created basic `ShaderComponent` (null effect) — the
  plain ball in the demo demonstrates this path.

### Playground

- **`RenderPipelineDemoScene.xml`**: move `EffectAsset` off each `SpriteComponent` onto a sibling
  `ShaderComponent`; fold the glow ball's `<EffectParameter>` into that same `ShaderComponent`. Leave the plain
  ball as `SpriteComponent`-only to exercise auto-create.
- **`PulsingGlowComponent`**: drive vars via `Owner.GetComponent<ShaderComponent>()?.SetFloat(...)` instead of
  the removed `Owner.GetRenderEffectParameters()`.

### Docs

- Update `docs/RenderPipeline.md`, `docs/ShaderUniforms.md`, `docs/SpriteSystem.md`: replace
  `SpriteComponent.Effect`/`EffectAsset`, `Entity.GetRenderEffect()`, and `EffectParametersComponent` with the
  single `ShaderComponent` model; document the requirement + auto-create (basic = null) behavior.

## Tests

- **`ShaderComponentTests`** (replaces `EffectParametersComponentTests` + the effect half of
  `SpriteComponentEffectTests`):
  - Effect precedence: explicit `Effect` wins over `EffectAsset`; `EffectiveEffect` null when neither set.
  - `OnAttach` resolves `EffectAsset` (mock content manager); missing asset logs + leaves null; `OnDetach` clears.
  - All var behavior that was on `EffectParametersComponent`: setters, `Get<T>` over raw strings, `Signature`
    stability/change detection, `InitializeFromStrings`, `Clear`.
  - **Auto-create**: a `SpriteComponent` with no sibling `ShaderComponent` gains one (null effect) — verified for
    both the code path and the prefab finish pass; a user-declared `ShaderComponent` is not duplicated.
- **`EntitySystemTests` / partition tests** (migrate from `SpriteComponentEffectTests`): the fake-effect entities
  now attach a `ShaderComponent` with a fake effect instead of overriding `GetRenderEffect()`. Keep coverage:
  no-shader regression (single run), same-effect-different-vars split, same-effect-same-vars coalesce, order.
- **`EntityPrefabLoader`**: `<EffectParameter>` bag is seeded onto the `ShaderComponent`; a sprite-only prefab
  instantiates with an auto-created basic `ShaderComponent`.
- **`SceneParserTests`**: `<EffectParameter>` parsing is unchanged (still valid); no new parser assertions needed.

## Compatibility / breaking changes

- **Removed public API:** `Entity.GetRenderEffect()`, `Entity.GetRenderEffectParameters()`,
  `SpriteComponent.Effect`, `SpriteComponent.EffectAsset`, `SpriteComponent.EffectiveEffect`, and the entire
  `EffectParametersComponent` type. All replaced by `ShaderComponent`. This is a source-breaking change for any
  external consumer — acceptable on the feature branch, called out here so the PR notes it.

## Definition of done

- [ ] Library + test project build clean.
- [ ] `ShaderComponent` owns effect + vars; `SpriteComponent` no longer references any effect.
- [ ] `Entity` has no `GetRenderEffect*` methods; pipeline resolves via `GetComponent<ShaderComponent>()`.
- [ ] Sprite renderer guarantees a sibling `ShaderComponent`, auto-creating a basic (null-effect) one when missing — safe across the deferred-attach window; on the XML/prefab path this logs a warning to declare it in XML.
- [ ] New + migrated tests pass; full suite green (no regression).
- [ ] Playground demo restructured and still renders the pulsing glow; plain ball uses the auto-created basic shader.
- [ ] Docs updated to the `ShaderComponent` model; linked consistently.
- [ ] Smoke-run harness passes for all scenes including the updated demo.
- [ ] Committed on the feature branch (`feature/render-pipeline-effect-passes`).

## Confirmed decisions

1. **Merge** — `ShaderComponent` absorbs `EffectParametersComponent`; one component owns the effect *and* its vars.
2. **Hold version** — keep `CoreEssentials.csproj` at `0.21.0`; bump only when this branch merges to `development`.
3. **Auto-create on start** — guarantee happens at attach (code path) / prefab finish pass (XML path), *not* first
   render. On the XML/prefab path, a missing shader logs a warning telling the author to declare
   `<Component Type="ShaderComponent">` in XML (the "could be an edge case where not all components are in" concern).
