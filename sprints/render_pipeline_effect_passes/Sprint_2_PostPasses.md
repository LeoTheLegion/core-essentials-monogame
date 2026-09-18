# Sprint 2 — Post Pass: Full-Screen Quad Passes After Scene + GUI

## Goal
Let game code register one or more **full-screen post passes** (each a quad drawn through an
`Effect`) that run **after** the scene and GUI — without subclassing `MainGame`. Covers additive
overlays (vignette, kill flash, color grade) that need no render target.

## Design
- New static registry: `CoreEssentials.RenderPipeline` (chosen over per-instance `MainGame` methods so
  it is simple and data-driven-friendly).
  - `AddPostPass(Effect effect, ...)` / `RemovePostPass(...)` / `ClearPostPasses()`.
  - A post pass draws a full-screen quad through its `Effect`, additive over the current frame.
  - Ordering: registration order is render order (first registered drawn first).
- `MainGame.Draw` hook: after `SceneManager.Draw` + `GUIManager.Draw`, call
  `RenderPipeline.DrawPostPasses(gameTime, _spriteBatch)`. This is a **no-op when no passes are
  registered**, so the default game loop is untouched (acceptance criterion #5).

## Changes
- New file `CoreEssentials/src/Rendering/RenderPipeline.cs`:
  - `RenderPipeline` static class: ordered list of post passes, add/remove/clear, and
    `DrawPostPasses(GameTime, SpriteBatch)` that opens one Begin/End with the pass's effect and draws
    a full-screen quad.
- `MainGame.Draw`: call `RenderPipeline.DrawPostPasses(...)` after GUI.

## Full-screen quad
A unit quad (two triangles) drawn through the pass's `Effect`. The effect is responsible for sampling
the backbuffer / producing output; CE only sets up the Begin with the effect and submits the quad.
(For additive overlays the shader samples `Texture2D` of the frame or simply outputs a color.)

## Tests
- Registry: add preserves order, remove by reference, clear empties, duplicate effects allowed.
- `DrawPostPasses`: no-op when empty (no Begin/End), one run per pass in registration order.
  (Verified headlessly with the `FakeEffect` from `EffectAssetTests` — no real device needed.)

## Definition of done
- [ ] Library + test project build clean.
- [ ] New tests pass; existing suite green.
- [ ] XML doc comments added.
- [ ] Committed on the feature branch.
