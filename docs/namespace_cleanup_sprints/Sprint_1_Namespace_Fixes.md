# Sprint 1 — Namespace Fixes & Test Updates 📋

**Points:** 7  
**Status:** Not Started  
**Prerequisite:** Sprint 0 must be completed first  
**Sprint Goal:** Fix namespace inconsistencies that require updating `using` statements across all three projects, including test files.

---

## Tasks

### T1: Rename `scene/` → `Scene/` and fix namespace (2 pts)
- Move folder from `CoreEssentials/src/scene/` to `CoreEssentials/src/Scene/`
- **Rename namespace** from `CoreEssentials.SceneManagement` → `CoreEssentials.Scene` in all source files:
  - `Scene.cs`, `SceneManager.cs`, `LoadingScene.cs`
- Update `using CoreEssentials.SceneManagement;` → `using CoreEssentials.Scene;` in affected CoreEssentials files:
  - `src/MainGame.cs`
  - `src/gameSystems/GameSystem.cs`

### T1b: Update Playground After Scene Rename (1 pt) 🔁
- Update `using CoreEssentials.SceneManagement;` → `using CoreEssentials.Scene;` in:
  - `CameraScene.cs`, `CharacterScene.cs`, `PhysicsEntityScene.cs`, `Program.cs`, `XmlLayoutScene.cs`

### T1c: Update Tests After Scene Rename (1 pt) 🔁
- Update `using CoreEssentials.SceneManagement;` → `using CoreEssentials.Scene;` in test files:
  - `CoreEssentials.Tests/SceneManagement/SceneLoadingTests.cs`
  - `CoreEssentials.Tests/SceneManagement/SceneManagerTests.cs`
  - `CoreEssentials.Tests/GameSystems/GameSystemOnStartTests.cs`
  - `CoreEssentials.Tests/GameSystems/GameSystemTests.cs`

### T2: Fix `Camera/` namespace pluralization (1 pt)
- **Rename namespace** from `CoreEssentials.Cameras` → `CoreEssentials.Camera` in `Camera.cs`
- Update `using CoreEssentials.Cameras;` → `using CoreEssentials.Camera;` in affected CoreEssentials files:
  - `src/GUI/Canvas.cs` (note: GUI folder renamed in Sprint 0)

### T2b: Update Playground & Tests After Camera Rename (1 pt) 🔁
- Update `using CoreEssentials.Cameras;` → `using CoreEssentials.Camera;` in:
  - **Playground:** `CameraEntity.cs`, `CameraScene.cs`
  - **Tests:** `CoreEssentials.Tests/Camera/CameraTests.cs`, `CoreEssentials.Tests/GUI/CanvasWorldSpaceTests.cs`

### T3: Final Build & Test Validation (1 pt) ⭐ Critical
- Run `dotnet build core-essentials-monogame.sln -c Release` — must compile with zero errors
- Run `dotnet test CoreEssentials.Tests/ --no-build` — **all 364 tests** must pass
- Verify Playground project still compiles

---

## Acceptance Criteria

- [ ] Folder renamed: `Scene/` (PascalCase)
- [ ] Namespace `CoreEssentials.Camera` (singular) replaces `CoreEssentials.Cameras`
- [ ] Namespace `CoreEssentials.Scene` replaces `CoreEssentials.SceneManagement`
- [ ] All test files updated with new namespace imports (`Scene`, `Camera`)
- [ ] Solution builds cleanly with zero errors
- [ ] All 364 tests pass
- [ ] No broken references in Playground or Tests projects

---

## Deliverables

- Namespace fixes for Scene and Camera subsystems
- Updated `using` statements across all three projects (CoreEssentials, Playground, Tests)
- Clean build and passing full test suite

## Affected File Summary

| Change | Source Files | Playground Files | Test Files |
|--------|-------------|------------------|------------|
| `scene/` → `Scene/` + namespace | 3 files (2 usings inside CoreEssentials) | 5 files | 4 files |
| `Cameras` → `Camera` namespace | 1 file (1 using inside CoreEssentials) | 2 files | 2 files |
