# Sprint 3 — Navigation API + Tooling 🧭

**Points:** 5 | **Status:** ✅ Done (2026-09-04) | **Goal:** Add ordered navigation directly to `SceneManager` (`NextScene()`/`PreviousScene()`) over the manifest, point the smoke-run harness at the manifest as the authoritative scene list, and finish the docs.

> **Same branch as Sprints 1–2** (`feature/scene-manifest`). Lands after core enforcement is green, so it builds on a stable base.

## Why This Sprint

With the manifest enforced (Sprint 2), "next" and "previous" finally have a well-defined meaning: ±1 through the ordered `<GameScenes>` list. The navigation API lives **on `SceneManager` itself** — no separate navigator class — because the core already holds the manifest and knows the current scene; a wrapper would just relay calls. Use cases it unlocks: level progression, menu Back buttons, tutorials, cutscene chains, attract-mode cycling.

## Design

- **`SceneManager.NextScene()` / `PreviousScene()`** — move ±1 through `<GameScenes>` relative to the *current* scene and load the result, using the normal transition path (so per-scene loading screens apply). Clamped at the ends: calling `NextScene()` on the last scene or `PreviousScene()` on the first is a no-op with a console note.
- **Untracked state** — if the current scene isn't in the manifest (e.g. loaded by name before enforcement, or a non-listed scene), `NextScene()`/`PreviousScene()` are no-ops with a console note until a listed scene becomes current.
- **Events** — `SceneManager` fires navigation events (`SceneAdvanced`, `SceneRetreated`) so systems can react without polling.
- **Harness** — `scripts/run-all-scenes.ps1` derives its scene list from `scenes.xml` (authoritative) instead of globbing; keeps a warning pass for `<Scene>` files on disk that aren't listed, so unregistered scenes are still surfaced.

## Tasks

- [x] T1 ⭐ `SceneManager`: add `NextScene()`/`PreviousScene()` (±1 clamped, untracked no-op) + `SceneAdvanced`/`SceneRetreated` events, reusing the existing transition path.
- [x] T2 🔒 Unit tests: Next/Previous happy paths, clamp at both ends, untracked → tracked behavior, event firing, and that transitions route through the normal loading-screen resolution. No windows, no XML on disk beyond temp fixtures.
- [x] T3 🔁 `scripts/run-all-scenes.ps1`: read the scene list from `scenes.xml` (fall back to an explicit error if the manifest is missing — consistent with core enforcement); keep glob-based warning for unlisted `<Scene>` files.
- [x] T4 📚 Docs: update `docs/SceneManagement.md` (manifest requirement, validation errors, navigation API, migration notes for library consumers), `docs/GettingStarted.md` (create your `scenes.xml`, startup = first entry, next/previous usage with an example), and the smoke-run section (list now comes from the manifest).
- [x] T5 🔁 Build + full suite green; smoke-run **all** scenes via the updated harness.

## Acceptance Criteria

- `SceneManager.NextScene()`/`PreviousScene()` move ±1 through the list, clamped at the ends, and fire their events.
- The runner script launches exactly the scenes listed in `scenes.xml` and warns about unlisted scene files.
- Docs cover the manifest format, enforcement errors, and the navigation API; build clean, full suite green; every scene still smoke-runs PASS.

## Notes & Risks

- **API stays thin.** No history stack, no branching, no gating predicates in v1 — those can layer on later without changing the manifest format or these methods.
- **No issue/PR numbers in code or docs** — repo convention.

## ✅ Completion Notes (2026-09-04)

- **API:** `SceneManager.NextScene()` / `PreviousScene()` resolve the target via `ResolveNavigationTarget(direction)` against the *resolved* manifest (`_manifest` only — navigation never forces the deferred asset parse; if it hasn't been resolved yet, it's a no-op with a console note). Guards: no manifest, transition in progress, current scene not a named `DataDrivenScene`, or name unregistered → no-op with a console note. Clamped at both ends. Navigation reuses `LoadScene(string)`, so per-scene loading screens and enforcement apply unchanged.
- **Events:** `SceneAdvanced` / `SceneRetreated` (`Action<string>`) fire once the transition they started has swapped in the new scene — a `_pendingNavigationEvent` is set before `LoadScene` and fired from both completion points of `RunTransition` via `CompleteNavigation()`. Payload is the asset name (type name for unnamed scenes).
- **Property rename:** the pending-scene property `SceneManager.NextScene` was renamed to `PendingScene` to free the method name — a small breaking change for consumers reading that property (updated in `LoadingScene` and 6 test references).
- **Tests (T2):** 10 new tests in `SceneManagerNavigationTests` — happy paths (next/previous/round-trip), clamping at both ends, no-manifest / untracked-current / while-transitioning no-ops, event firing with the right payload, and per-scene loading-screen routing through navigation. All use minimal zero-system XML fixtures + `MockContentManager`; no windows.
- **Harness (T3):** `run-all-scenes.ps1` now reads `<GameScenes>` from `scenes.xml` in order (missing manifest → abort), keeps the `-Scenes` override, and warns about `<Scene>`-rooted files on disk that are registered nowhere.
- **Docs (T4):** `docs/SceneManagement.md` gained a full Scene Manifest section (format, configuration, enforcement-error table, navigation API + events, migration notes); `docs/GettingStarted.md` gained a "Declaring Your Scenes" section and the smoke-run docs now describe the manifest-driven list.
- **Verification:** full suite 1174 passed / 0 failed / 3 skipped; harness smoke-ran all 7 registered scenes — all PASS.
