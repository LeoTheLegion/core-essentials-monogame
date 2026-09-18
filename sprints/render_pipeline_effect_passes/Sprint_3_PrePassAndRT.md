# Sprint 3 — Pre Pass + Render-to-`RenderTarget2D` Opt-In + Sampling Post Passes

## Goal
Complete the ordered pipeline with a **pre pass** (camera setup hook) and an opt-in **"render the
scene into a `RenderTarget2D`"** mode, so **sampling-based** post passes (bloom, blur, chromatic
aberration, dissolve-the-whole-frame) can read the rendered frame. Additive overlays (Sprint 2) keep
working with no RT; this sprint adds the sampling path.

## Design
- `RenderPipeline` gains:
  - `EnableRenderToTarget(bool)` — when true, the process pass renders into a fullscreen
    `RenderTarget2D` instead of straight to the backbuffer. When false (default), behavior is exactly
    as today (straight-to-backbuffer) — **no regression**.
  - A pre-pass hook: an ordered set of actions that run after `GraphicsDevice.Clear` and before the
    scene (camera setup lives here). No-op when empty.
  - Post passes can be marked **sampling** vs **additive**:
    - *Additive* — drawn over the frame, no RT required (Sprint 2 behavior).
    - *Sampling* — bound to the pre-bound RT as the shader's input texture; requires the RT opt-in.
- `MainGame.Draw` orchestration becomes:
  1. `GraphicsDevice.Clear`
  2. **Pre pass** — run registered pre actions (camera setup).
  3. If RT enabled → set `RenderTarget2D`; else backbuffer.
  4. **Process pass** — `SceneManager.Draw` + `GUIManager.Draw` (Sprint 1 per-sprite effects apply here).
  5. If RT enabled → unbind RT, blit RT to backbuffer.
  6. **Post pass** — `RenderPipeline.DrawPostPasses(...)` (additive over frame; sampling passes read the RT).

## Changes
- `RenderPipeline`:
  - Pre-pass registry (`AddPrePass(Action<...>)`, ordered) + `DrawPrePasses(...)`.
  - `EnableRenderToTarget(bool)` + internal `RenderTarget2D` creation/disposal tied to backbuffer size.
  - Post pass mode (additive vs sampling) and RT binding for sampling passes.
- `MainGame.Draw`: wire the six-step orchestration above; ensure the RT is (re)created on resize.

## Correctness / non-regression
- With everything unset: no pre actions, RT disabled, no post passes → byte-for-byte the current
  loop. (Acceptance criterion #5.)
- RT is only allocated when opted in and disposed/recreated on backbuffer size change.

## Tests
- Pre-pass registry ordering + no-op when empty.
- `EnableRenderToTarget` toggling; sampling post passes require RT (throw or no-op clearly when absent).
- Additive passes unaffected by RT mode.
  (Graphics-dependent paths validated via the smoke-run harness / playground, not unit tests.)

## Definition of done
- [x] Library + test project build clean.
- [x] New tests pass; existing suite green (full suite 1278 passed, 0 failed).
- [x] XML doc comments added.
- [x] Committed on the feature branch.
