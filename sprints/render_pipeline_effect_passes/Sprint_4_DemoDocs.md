# Sprint 4 — Playground Demo + Documentation + Final Verification

## Goal
Demonstrate the feature end-to-end in the playground, document it for users, and confirm the whole
branch builds, tests green, and runs (smoke-run harness) with no regressions.

## Changes
- **Playground demo** (`CoreEssentials.Playground`):
  - A scene/component that shows a per-sprite effect on an entity (e.g. a glowing target via
    `EffectAsset`).
  - A post-pass example registered through `RenderPipeline` (e.g. a vignette or kill-flash overlay).
  - Keep it minimal and data-driven where possible to prove the XML path works.
- **Documentation** (`docs/`):
  - New `docs/RenderPipeline.md` — covers the ordered pre/process/post model, per-sprite `Effect` /
    `EffectAsset` (usage, parameters, example), post-pass registration, and the render-to-target opt-in
    for sampling passes. Includes a worked example mirroring the playground scene.
  - Update `docs/README.md` index to link the new page.
  - Update `docs/SpriteSystem.md` "Instanced Rendering / Batching" section to note effect as an
    additional grouping key.

## Verification
- [x] `dotnet build` clean (library, playground, tests).
- [x] Full test suite green: run via `scripts/test.sh`.
- [x] Smoke-run harness (`scripts/run-all-scenes.ps1`) passes for all scenes including the new demo.
- [x] No perf/rendering regression for games that don't opt in (default path unchanged).

## Definition of done
- [x] All acceptance criteria from the feature request met and demonstrated.
- [x] Docs added/updated and linked.
- [x] Branch ready to open a PR against `development`.
