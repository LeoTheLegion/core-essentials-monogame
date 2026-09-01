# Sprint 2 — DataDrivenScene & Loading Screen 🎬

**Points:** 5 | **Status:** ✅ Done (2026-08-31) | **Goal:** Run a full scene from a data file, including the loading screen.

## Tasks

- [x] T1 ⭐ `DataDrivenScene(SceneDefinition)` : `Scene` — systems reflected from definition; prefabs registered (idempotent path); entities loaded via existing serializer
- [x] T2 ⭐ `SceneManager.LoadScene(string sceneAssetName)` overload
- [x] T3 ⭐ `SceneManager.SetLoadingScene(string loadingSceneAssetName)` overload — loading screen becomes a data scene file
- [x] T4 🔒 Built-in small updater component wiring `<Bind>` progress to `TransitionProgress` for the loading bar
- [x] T5 🔁 Tests: DataDrivenScene load order (systems → prefabs → entities); full transition through a data-driven loading screen completes and swaps scenes; loading progress reflects `TransitionProgress`

## Acceptance Criteria

- A scene with no C# subclass loads systems, prefabs, and entities correctly
- Transition via data-driven loading screen works end-to-end in tests
- Build clean, all tests passing

## Notes

- **Files:** new `Scene/DataDrivenScene.cs` and `Components/BuiltIn/TransitionProgressComponent.cs`; `SceneManager` gained string overloads for `LoadScene`/`SetLoadingScene` plus a `LoadingScene` property; `GameSystem.Scene` and `Scene.SceneManagerOrNull` were added so components can reach the manager without `MainGame`; `SceneParser` model refinements — `Rotation`/`Sort`/`Active` became nullable (absent → null) and `<Components>` now build full `Prefab.ComponentDefinition` objects into `DeclaredComponents` so plain-class definitions carry their component properties.
- **Instantiation paths:** `Source=` definitions go through `EntitySystem.Instantiate(name, position, ResolvedOverrides)` (prefab already registered in the same pass); `Type=` definitions build an ad-hoc `Prefab` from the definition and route through the same `EntityPrefabLoader.Instantiate` path — both share attachment, override-application, bind, and child semantics. `Id=` is applied post-instantiation via `entity.SetId`, binds are deep-copied into a wrapper element and run through `CommandBindings.ApplyBindings`, children recurse with `AddChild`.
- **Reference resolution:** after all entities exist, `<Reference Name TargetId>` entries resolve against the id map — entity property first, then component property/field of an Entity-assignable type, exactly mirroring `EntitySerializer.SetReference`.
- **Transition progress:** `SceneManager.TransitionProgress` now reports 1.0 during the final frame after load completes but before the swap (previously dropped to 0), so progress bars don't visibly regress. `TransitionProgressComponent` clamps and only fires `ProgressChanged` past a 0.0001 delta, updating an optional `LabelComponent` with a percentage.
- **Tests:** 3 new integration tests in `DataDrivenSceneTests.cs` (load order + overrides + references; full transition through a data-driven loading screen; progress component mirroring). Full suite: **1035 passed / 0 failed / 3 skipped** (baseline 1032, +3).

---
*Created: 2026-08-31 | Part of Scene-as-Data Project*
