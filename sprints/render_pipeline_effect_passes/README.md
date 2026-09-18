# Render Pipeline: Ordered Pre / Process / Post Passes with First-Class `Effect` Support

Feature branch: `feature/render-pipeline-effect-passes` (from `development`)
Base version: `0.20.1` → **`0.21.0`**

## Problem

CoreEssentials has no way to apply shaders — neither per-sprite (glow, dissolve, outline on a
target) nor screen-space post-processing (radiation vignette, kill flash, bloom). `EffectAsset`
already loads a compiled MonoGame `Effect`, but nothing in the render path ever applies one.

This feature introduces an **ordered pre / process / post pass pipeline** with `Effect` as a
first-class concept, covering both per-sprite and screen-space shaders under one model. The
unifying primitive is **"a pass that carries an optional `Effect`"**; only the *input* differs per
stage.

## The crux (why it's a batching concern)

In MonoGame an `Effect` is set at `SpriteBatch.Begin(...)`, **not** per-`Draw`. So "a shader on one
specific sprite" is only possible by opening a separate Begin/End with that effect for just that
sprite (or group of sprites sharing it). CE already splits into Begin/End blocks per **z-layer** and
per **texture**, so threading a per-sprite `Effect` through means adding **"effect" as a third
grouping key**: entities batch by `(zLayer, texture, effect)`, each distinct group getting its own
Begin with that effect. Entities with no effect keep the current default-effect batching — **no perf
or rendering regression** for games that don't opt in.

## Sprint breakdown

| Sprint | Scope | Status |
| ------ | ----- | ------ |
| [Sprint 0](./Sprint_0_Plan.md) | Plan, version bump, branch | ✅ Done |
| [Sprint 1](./Sprint_1_PerSpriteEffects.md) | Process pass: per-sprite `Effect` + effect as a batching key | ⬜ |
| [Sprint 2](./Sprint_2_PostPasses.md) | Post pass: static `RenderPipeline` registry + full-screen quad passes after scene+GUI | ⬜ |
| [Sprint 3](./Sprint_3_PrePassAndRT.md) | Pre pass + render-to-`RenderTarget2D` opt-in + sampling-based post passes | ⬜ |
| [Sprint 4](./Sprint_4_DemoDocs.md) | Playground demo + documentation + final verification | ⬜ |

Each sprint is an independent, buildable, testable increment. A commit lands after each sprint so
the branch stays in a releasable state.

## Acceptance criteria (from the feature request)

1. An XML entity can declare `EffectAsset="Effects/glow.xml"` on its `SpriteComponent` and render
   with that shader, still benefiting from z-layer/texture batching (grouped by effect).
2. Code can set `spriteComp.Effect = myEffect` directly.
3. Post passes can be registered to draw a full-screen quad through an `Effect` **after** the scene
   and GUI — without subclassing `MainGame`.
4. Additive post overlays work with no render target; sampling-based post passes work when the pre
   pass is configured to render the scene into an RT.
5. Leaving everything unset keeps current behavior exactly (default SpriteBatch effect, straight-to
   backbuffer) — **no perf or rendering regression** for games that don't opt in.

## Non-goals

- Not a general 3D pipeline / GPU compute. This is 2D sprite + full-screen passes.
- Shader authoring/tooling is out of scope; effects are compiled `.fx` via the existing Content
  Pipeline (`EffectAsset`).
