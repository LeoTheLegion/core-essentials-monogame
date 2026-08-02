# Sprint 1 — Namespace Fixes & Test Updates 📋

**Points:** 7  
**Status:** ✅ Complete  
**Prerequisite:** Sprint 0 must be completed first  
**Sprint Goal:** Fix namespace inconsistencies that require updating `using` statements across all three projects, including test files.

---

## Notes

- **Namespace name adjustment:** The planned rename `CoreEssentials.SceneManagement` → `CoreEssentials.Scene` caused a naming conflict (namespace `Scene` clashes with type `Scene` inside it). Changed to `CoreEssentials.Scenes` (plural) instead — this avoids the ambiguity while keeping folder name as `Scene/`.
- **CameraEntity** uses `using CoreEssentials.Camera;` + qualified type `Camera.Camera` to disambiguate namespace from type.
- **CanvasWorldSpaceTests** required fully-qualified `CoreEssentials.Camera.Camera` references throughout (alias didn't resolve reliably).

---

## Tasks

### T1: Rename `scene/` → `Scene/` and fix namespace to `Scenes` (2 pts) ✅
- Move folder from `CoreEssentials/src/scene/` to `CoreEssentials/src/Scene/`
- **Rename namespace** from `CoreEssentials.SceneManagement` → `CoreEssentials.Scenes` in all source files:
  - `Scene.cs`, `SceneManager.cs`, `LoadingScene.cs`
- Update `using CoreEssentials.SceneManagement;` → `using CoreEssentials.Scenes;` in affected CoreEssentials files:
  - `src/MainGame.cs` (also fixed XML doc cref)
  - `src/GameSystems/GameSystem.cs`

### T1b: Update Playground After Scene Rename (1 pt) ✅ 🔁
- Update `using CoreEssentials.SceneManagement;` → `using CoreEssentials.Scenes;` in:
  - `CameraScene.cs`, `CharacterScene.cs`, `PhysicsEntityScene.cs`, `Program.cs`, `XmlLayoutScene.cs`

### T1c: Update Tests After Scene Rename (1 pt) ✅ 🔁
- Update `using CoreEssentials.SceneManagement;` → `using CoreEssentials.Scenes;` in test files:
  - `CoreEssentials.Tests/SceneManagement/SceneLoadingTests.cs`
  - `CoreEssentials.Tests/SceneManagement/SceneManagerTests.cs`
  - `CoreEssentials.Tests/GameSystems/GameSystemOnStartTests.cs`
  - `CoreEssentials.Tests/GameSystems/GameSystemTests.cs`

### T2: Fix `Camera/` namespace pluralization (1 pt) ✅
- **Rename namespace** from `CoreEssentials.Cameras` → `CoreEssentials.Camera` in `Camera.cs`
- Update `using CoreEssentials.Cameras;` → `using CoreEssentials.Camera;` in affected CoreEssentials files:
  - `src/GUI/Canvas.cs` (note: GUI folder renamed in Sprint 0)

### T2b: Update Playground & Tests After Camera Rename (1 pt) ✅ 🔁
- Update `using CoreEssentials.Cameras;` → `using CoreEssentials.Camera;` in:
  - **Playground:** `CameraEntity.cs`, `CameraScene.cs`
  - **Tests:** `CoreEssentials.Tests/Camera/CameraTests.cs`, `CoreEssentials.Tests/GUI/CanvasWorldSpaceTests.cs`

### T2c: Fix Additional Camera References (bonus) ✅
- Update bare `Cameras.Camera` → `Camera.Camera` in `EntitySystem.cs` (added using statement)
- Update `CoreEssentials.Cameras.Camera` → `CoreEssentials.Camera.Camera` in `CanvasImpl.cs`

### T3: Final Build & Test Validation (1 pt) ✅ ⭐ Critical
- Run `dotnet build core-essentials-monogame.sln -c Release` — **0 errors** ✅
- Run `dotnet test CoreEssentials.Tests/ --no-build` — **364 passed, 2 skipped, 0 failed** ✅
- Verify Playground project still compiles ✅

---

## Acceptance Criteria

- [x] Folder renamed: `Scene/` (PascalCase)
- [x] Namespace `CoreEssentials.Camera` (singular) replaces `CoreEssentials.Cameras`
- [x] Namespace `CoreEssentials.Scenes` (plural to avoid type conflict) replaces `CoreEssentials.SceneManagement`
- [x] All test files updated with new namespace imports (`Scenes`, `Camera`)
- [x] Solution builds cleanly with zero errors
- [x] All 364 tests pass (2 skipped — same as baseline)
- [x] No broken references in Playground or Tests projects

---

## Deliverables

- Namespace fixes for Scene and Camera subsystems
- Updated `using` statements across all three projects (CoreEssentials, Playground, Tests)
- Clean build and passing full test suite

## Affected File Summary

| Change | Source Files | Playground Files | Test Files |
|--------|-------------|------------------|------------|
| `scene/` → `Scene/` + namespace | 3 files (`Scene.cs`, `SceneManager.cs`, `LoadingScene.cs`) + 2 internal usings (`MainGame.cs`, `GameSystem.cs`) | 5 files (`CameraScene.cs`, `CharacterScene.cs`, `PhysicsEntityScene.cs`, `Program.cs`, `XmlLayoutScene.cs`) | 4 files (`SceneLoadingTests.cs`, `SceneManagerTests.cs`, `GameSystemOnStartTests.cs`, `GameSystemTests.cs`) |
| `Cameras` → `Camera` namespace | 1 file (`Camera.cs`) + 2 internal usings (`Canvas.cs`, `EntitySystem.cs`, `CanvasImpl.cs`) | 2 files (`CameraEntity.cs`, `CameraScene.cs`) | 2 files (`CameraTests.cs`, `CanvasWorldSpaceTests.cs`) |

## Unexpected Issues Fixed

| Issue | Resolution |
|-------|-----------|
| Namespace `CoreEssentials.Scene` conflicts with type `Scene` inside it (CS0118) | Renamed to `CoreEssentials.Scenes` (plural) instead |
| `CameraEntity.cs` uses `Cameras.Camera` qualified type references | Updated to `Camera.Camera` |
| `CanvasWorldSpaceTests.cs` — bare `Camera` type ambiguous after alias | Used fully-qualified `CoreEssentials.Camera.Camera` throughout |
| `EntitySystem.cs` bare `Cameras.Camera` reference missing using | Added `using CoreEssentials.Camera;` + qualified to `Camera.Camera` |
| `CanvasImpl.cs` references `CoreEssentials.Cameras.Camera` | Updated to `CoreEssentials.Camera.Camera` |
| `MainGame.cs` XML doc cref `SceneManagement.SceneManager` unresolved | Fixed to `<see cref="SceneManager"/>` |
