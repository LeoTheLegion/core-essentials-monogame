# Sprint 2 — Core Enforcement + Migration 🚦

**Points:** 7 | **Status:** ⬜ Not started | **Goal:** The core enforces the manifest: `SceneManager` cannot load a scene that isn't registered in it. Migrate the playground and every existing test to supply a manifest, so the suite stays green with enforcement on.

> **Same branch as Sprint 1** (`feature/scene-manifest`, off `feature/scene-as-data`). Enforcement and the migration land together — an enforcing core with unmigrated tests/playground would break the build and E2E.

## Why This Sprint

Code-as-data consistency: scenes are data, so *the list of scenes* must be data too — and the core should not allow a scene to exist at runtime that was never declared. If `scenes.xml` is missing or unresolvable, boot errors out loudly (non-zero exit), which also makes the smoke-run harness fail closed.

## Design

- **`SceneManager` requires a manifest.** A new initialization step (e.g. `SceneManager.Initialize(SceneManifest)`) must complete before any scene load. `LoadScene(name)` throws if `name` is not in `<GameScenes>`; `SetLoadingScene(name)` throws if not in `<LoadingScenes>`. The manifest's first game scene is the startup scene.
- **Deferred resolution.** The manifest file lives in `Content/`, so it must be parsed *after* `AssetManager` init (the smoke-run harness learned this the hard way: eager parsing at `Program.cs` time threw `InvalidOperationException`). `Program.cs` registers the asset name; the core resolves it during load.
- **Per-scene loading screens.** Transitioning into scene X uses X's `LoadingScreen` attribute, else the `Default="true"` entry, else none — resolved by the core during transitions (replaces the single hardcoded loading scene).

## Tasks

- [ ] T1 ⭐ `SceneManager`: manifest init requirement + load-time validation (unregistered game scene / loading screen → descriptive exception naming the offending name and the registered list).
- [ ] T2 ⭐ Core transition path uses per-scene loading-screen resolution (attribute → default → none) instead of a single hardcoded loading scene.
- [ ] T3 🔒 Tests: unregistered-scene load throws, unregistered loading screen throws, missing/late manifest init throws, per-scene loading-screen resolution (explicit attribute, default fallback, none).
- [ ] T4 🔁 Add a small manifest-builder helper for test fixtures so each migrated test supplies its scene list in one line.
- [ ] T5 🔁 Migrate every existing test fixture that constructs a `SceneManager` and loads scenes to supply a manifest (mechanical, wide — the known cost of this sprint).
- [ ] T6 🔁 Migrate the playground: `Program.cs` registers `scenes.xml`; add `CoreEssentials.Playground/Content/scenes.xml` listing all current scenes + `loading.xml` as default loading screen; remove the hardcoded `HomeScene.xml` / `loading.xml` strings. Missing or malformed file → clear console error + non-zero exit.
- [ ] T7 🔁 Build + full suite green; smoke-run at least two scenes (one with a loading-screen transition) to confirm clean boot and E2E behavior.

## Acceptance Criteria

- A `SceneManager` without a manifest cannot load any scene; an unregistered scene name throws with a descriptive message.
- The playground boots its first listed scene and errors out (non-zero exit) when `scenes.xml` is missing or malformed.
- All existing tests migrated; build clean, full suite green.

## Notes & Risks

- **This is a breaking change for library consumers** — bump the package version and call it out in the release notes / migration section of `docs/SceneManagement.md` (doc update itself lands in Sprint 3).
- **Wide test migration (T5) is the risk.** Mitigated by the T4 fixture helper; if it balloons past the point budget, split T5 into its own follow-up commit on the same branch.
- **Existing tests pass scene files straight to `SceneManager.LoadScene(string)`** — under enforcement those strings must match manifest entries exactly (including the `.xml` suffix); the fixture helper normalizes that.
- **No issue/PR numbers in code or docs** — repo convention.
