# Sprint 5a — Foundation: Data-Driven Booting 🚀

**Points:** 2 | **Status:** Done ✅ | **Goal:** Prove the risky pipeline end-to-end — the game boots purely from files (data-driven loading screen + first scene), with no C# scene subclass involved.

## Why This Sprint

Sprint 5 (migrate all playground scenes) is large and high-risk. 5a isolates the single riskiest, most foundational slice: **booting from files**. Everything else (scene migrations) depends on this working. By proving it first we de-risk the rest and keep each subsequent sprint small and independently verifiable.

This sprint is **additive only** — no C# scene subclasses are deleted. The existing `LoadingScene` and all demo scenes stay intact; we just add a data-driven loading screen and a minimal placeholder first-scene file, then wire `Program.cs` to boot from them.

## What Already Exists (do not rebuild)

- `SceneManager.SetLoadingScene(string)` and `LoadScene(string)` overloads already wrap a name in `new DataDrivenScene(SceneParser.LoadFromAsset(name))`.
- `TransitionProgressComponent` (built-in) mirrors `SceneManager.TransitionProgress` into `Progress` each frame, auto-syncs an owner's `LabelComponent` text as a percentage, and raises `ProgressChanged`.
- The strict scene format is proven by existing tests (`DataDrivenSceneTests`: full transition through a data-driven loading screen completes; progress component tracks 0→1).

## Tasks

- [x] T1 ⭐ Create `CoreEssentials.Playground/Content/loading.xml` — a data-driven loading screen in the **strict** format: `<Scene>` → `<GameSystems>` → `<System Type="EntitySystem">` with an entity carrying `CanvasComponent` + `LabelComponent` ("Loading…", center-aligned) + `TransitionProgressComponent`. *(Kept flat on purpose — see Bug #2; a nested label couldn't find its canvas until that was fixed.)*
- [x] T2 ⭐ Create a minimal placeholder first-scene file (`HomeScene.xml`) in the strict format — a canvas root with two anchored, labeled children (idiomatic nested form) — so 5a does not depend on any of the hard scene migrations (5b–5d).
- [x] T3 ⭐ Add `/copy:loading.xml` and `/copy:HomeScene.xml` entries to `CoreEssentials.Playground/Content/Content.mgcb`.
- [x] T4 ⭐ Update `Program.cs`: `SetLoadingScene("loading.xml")` + `LoadScene("HomeScene.xml")` string overloads — remove `new LoadingScene(...)` and the `LabelAlignmentDemoScene` instance. Keep the 1280×720 backbuffer set once here.
- [x] T5 🔁 Integration test: parse `loading.xml` from disk, assert it is a valid strict `SceneDefinition` whose entity carries both a `LabelComponent` and a `TransitionProgressComponent`; run a full transition through it (like the existing test) to confirm the real file — not just an inline fixture — boots. *(See `BootFromFilesTests`.)*

## Acceptance Criteria

- The game boots from files only: `Program.cs` contains no `LoadingScene`/scene-subclass references.
- `loading.xml` and the placeholder scene parse in the strict format and load as a `DataDrivenScene`.
- Build clean, all tests passing; running the playground shows the data-driven loading screen then the placeholder scene.

## Bugs Found & Fixed (SWE practice: failing test first, then fix)

Booting real GUI scenes from files surfaced **two genuine library bugs**. Per SWE practice each was first reproduced by a **failing regression test**, then fixed at the root cause (no XML workarounds).

### Bug #1 — `CanvasComponent` had no parameterless constructor

`CanvasComponent` only declared an optional-parameter ctor: `CanvasComponent(bool isScreenSpace = true)`. C# does **not** synthesize a parameterless constructor from that, so the prefab loader's `Activator.CreateInstance(type)` threw `MissingMethodException`, which was swallowed and logged as `[Prefab] Skipping creation of 'CanvasComponent' - no parameterless constructor.` — silently dropping the canvas for **every** data-driven GUI scene.

- **Regression test:** `CanvasComponentTests.Constructor_ViaReflection_UsesTrueParameterlessCtor` (`Activator.CreateInstance(typeof(CanvasComponent))`).
- **Fix:** explicit `public CanvasComponent() : this(true) { }` in `CanvasComponent.cs`.

### Bug #2 — nested scene `<Children>` attached components before the parent link existed

`DataDrivenScene.InstantiateDefinition` instantiated each scene-level `<Children>` entity **independently** via `EntityPrefabLoader.Instantiate`, which self-attaches that child's components immediately, and only then called `AddChild`. So a child's `LabelComponent.OnAttach()` → `CanvasComponent.RequireCanvas(Owner)` walked the parent chain while `Parent` was still null → threw *"No CanvasComponent found"*. The prefab loader avoids this with its two-phase design (build+link the whole subtree first, then attach parents-before-children); scene definitions never routed nested children through that path.

- **Regression tests:** `DataDrivenSceneNestedGuiTests` — `NestedCanvasRootAndLabelChild_Loads_WithoutThrowing` (canvas root + anchored label child) and `DeeplyNestedGuiChild_Loads_WithoutThrowing` (3 levels: canvas → panel → label). Both failed with *"No CanvasComponent found"* before the fix.
- **Fix:** `DataDrivenScene` now builds the **entire nested definition tree as a single `Prefab`** (`BuildCombinedPrefab`, recursive) and instantiates it in one `EntityPrefabLoader.Instantiate` call, so linking happens before any attach. Registered-prefab roots are resolved via a new `EntitySystem.TryGetPrefab`. Ids/binds are then applied recursively (`ApplyIdsAndBinds`).
- **Position note:** nested children now inherit the root's position (matching `EntityPrefabLoader.BuildSubtree`, which passes the same `position` down). Acceptable for GUI scenes because `AnchorComponent` drives per-frame positioning.

---

## Notes & Risks

- **First-scene choice:** 5a uses a trivial placeholder so it is independent of 5b–5d. Once 5b lands a real migrated scene, `Program.cs` can repoint to it (that repointing belongs to that sprint).
- **Strict format only:** the old flat XML (e.g. current `GuiAnchorDemo.xml`) has no `<GameSystems>` wrapper and will NOT parse here — write both files from scratch in the strict format.
- Keep this small: if anything here is flaky, stop and surface it before touching scene migrations.

---
*Created: 2026-09-01 | Part of Scene-as-Data Project (Sprint 5 split)*
