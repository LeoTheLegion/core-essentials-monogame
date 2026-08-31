# Sprint 2 — DataDrivenScene & Loading Screen 🎬

**Points:** 5 | **Status:** Not Started | **Goal:** Run a full scene from a data file, including the loading screen.

## Tasks

- [ ] T1 ⭐ `DataDrivenScene(SceneDefinition)` : `Scene` — systems reflected from definition; prefabs registered (idempotent path); entities loaded via existing serializer
- [ ] T2 ⭐ `SceneManager.LoadScene(string sceneAssetName)` overload
- [ ] T3 ⭐ `SceneManager.SetLoadingScene(string loadingSceneAssetName)` overload — loading screen becomes a data scene file
- [ ] T4 🔒 Built-in small updater component wiring `<Bind>` progress to `TransitionProgress` for the loading bar
- [ ] T5 🔁 Tests: DataDrivenScene load order (systems → prefabs → entities); full transition through a data-driven loading screen completes and swaps scenes; loading progress reflects `TransitionProgress`

## Acceptance Criteria

- A scene with no C# subclass loads systems, prefabs, and entities correctly
- Transition via data-driven loading screen works end-to-end in tests
- Build clean, all tests passing

---
*Created: 2026-08-31 | Part of Scene-as-Data Project*
