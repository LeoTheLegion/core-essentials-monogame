# Sprint 1 — Process Pass: Per-Sprite `Effect` Support

## Goal
Let a sprite render with a MonoGame shader, both from code (`spriteComp.Effect = myEffect`) and from
data-driven XML (`EffectAsset="Effects/glow.xml"`), while preserving z-layer/texture batching and
causing **zero regression** for sprites without an effect.

## The crux (drives the design)
In MonoGame an `Effect` is applied at `SpriteBatch.Begin(...)`, **not** per-`Draw`. So a "shader on
one sprite" requires its own Begin/End block. CE's batcher already opens one Begin/End per group, so
the correct change is to **partition each render list into contiguous runs that share the same
effect**, and open one `Begin(effect)/End` per run. This is order-preserving and provably no-op when
no effect is set (a single all-null run = one `Begin(null)/End`, exactly as before).

> Note: we deliberately do **not** add a no-op `Effect` parameter to `Sprite.DrawFrame` — the effect
> only takes effect at Begin, so threading it through Draw would be misleading. The batcher is the
> single place effects are applied.

## Key finding from tracing the code
In the data-driven flow, entities with a `SpriteComponent` have `BatchTexture == null`, so they
render through the **"no-texture" path** (not the texture-batched path). Effects must therefore be
applied on **both** render paths (`RenderNoTextureEntities` and `RenderZLayers`).

## Changes
- `SpriteComponent`:
  - `public Effect? Effect { get; set; }` — explicit assignment wins.
  - `public string EffectAsset { get; set; } = ""` — resolved via `AssetManager.LoadAsset<EffectAsset>(name)` in `OnAttach()`, only when `Effect` is not already set.
  - `public Effect? EffectiveEffect => Effect ?? _resolvedEffect;` — single source of truth for the batcher.
  - `OnDetach()` clears the resolved effect so a re-attach can resolve again.
- `Entity.GetRenderEffect()` — returns the owning `SpriteComponent`'s `EffectiveEffect`, or null.
- `EntitySystem`:
  - New `PartitionByEffect(List<Entity>)` → contiguous `(Effect?, List<Entity>)` runs (reference-equality on effect).
  - New `RenderEffectRuns(runs, spriteBatch, cameraView)` — one `Begin(run.Effect)/End` per run.
  - Both `RenderNoTextureEntities` and `RenderZLayers` route through these helpers.

## Tests
- `SpriteComponent`: explicit `Effect` wins over `EffectAsset`; `EffectAsset` resolves on attach;
  missing effect asset is swallowed (leaves null); `OnDetach` clears resolved effect; no-op when unset.
- `Entity.GetRenderEffect()`: returns component effect, null without a sprite component.
- `PartitionByEffect`: single run when all null (regression guard), splits on distinct effects,
  preserves order, coalesces adjacent same-effect entities, handles interleaved effects.

## Definition of done
- [x] Library + test project build clean.
- [x] New tests pass; existing z-order/batching tests still green (no regression). Full suite: 1256 passed / 0 failed / 3 skipped.
- [x] XML doc comments added for all new public members.
- [x] Committed on the feature branch.
