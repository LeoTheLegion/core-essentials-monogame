# Sprint 0 — Folder Renaming (Simple Renames) 📋

**Points:** 7  
**Status:** ✅ Complete  
**Sprint Goal:** Rename folders with casing mismatches that have **no namespace changes required**. These are safe renames where the namespace already matches the target folder name.

---

## Results Summary

| Task | Folder | Build | Tests | Status |
|------|--------|-------|-------|--------|
| T1 + T1b | `assets/` → `Assets/` | ✅ | 37 passed | ✅ |
| T2 + T2b | `debugging/` → `Debugging/` | ✅ | 13 passed | ✅ |
| T3 + T3b | `gameSystems/` → `GameSystems/` | ✅ | 196 passed | ✅ |
| T4 + T4b | `gui/` → `GUI/` | ✅ | 48 passed | ✅ |
| T5 + T5b | `inputs/` → `Inputs/` | ✅ | 5 passed, 2 skipped | ✅ |

**Final Validation:** Full test suite — 366 total, 364 succeeded, 2 skipped (matches baseline)

---

## Tasks

### T1: Rename `assets/` → `Assets/` (1 pt)
- Move folder from `CoreEssentials/src/assets/` to `CoreEssentials/src/Assets/`
- Update `.csproj` any embedded resource paths referencing the old folder name
- Verify namespace `CoreEssentials.Assets` already matches ✅ (no code changes needed inside files)

### T1b: Validate After Assets Rename (0.5 pt) 🔁
- Run `dotnet build core-essentials-monogame.sln -c Release` — must compile with zero errors
- Run `dotnet test CoreEssentials.Tests/Asset/ --no-build` — asset tests must pass

### T2: Rename `debugging/` → `Debugging/` (1 pt)
- Move folder from `CoreEssentials/src/debugging/` to `CoreEssentials/src/Debugging/`
- Verify namespace `CoreEssentials.Debugging` already matches ✅

### T2b: Validate After Debugging Rename (0.5 pt) 🔁
- Run `dotnet build core-essentials-monogame.sln -c Release` — must compile with zero errors
- Run `dotnet test CoreEssentials.Tests/Debugging/ --no-build` — debugging tests must pass

### T3: Rename `gameSystems/` → `GameSystems/` (1 pt)
- Move folder from `CoreEssentials/src/gameSystems/` to `CoreEssentials/src/GameSystems/`
- This includes subfolders: `entitySystems/entityOOPsystem/` and `physics/` with all nested content
- Verify namespaces already match ✅

### T3b: Validate After GameSystems Rename (1 pt) 🔁
- Check affected tests: `GameSystemTests.cs`, `GameSystemOnStartTests.cs`, all physics tests under `GameSystems/Physics/`
- Run `dotnet build core-essentials-monogame.sln -c Release` — must compile with zero errors
- Run `dotnet test CoreEssentials.Tests/GameSystems/ --no-build` — game systems tests must pass

### T4: Rename `gui/` → `GUI/` (1 pt)
- Move folder from `CoreEssentials/src/gui/` to `CoreEssentials/src/GUI/`
- Includes subfolders: `types/`, `factory/`, `Internal/`, `engines/myra/Widgets/`, `engines/myra/Brushes/`
- Verify namespaces already match ✅

### T4b: Validate After GUI Rename (1 pt) 🔁
- Check affected tests: all tests under `CoreEssentials.Tests/GUI/` (`CanvasTests.cs`, `GuiSerializerTests.cs`, etc.)
- Run `dotnet build core-essentials-monogame.sln -c Release` — must compile with zero errors
- Run `dotnet test CoreEssentials.Tests/GUI/ --no-build` — GUI tests must pass

### T5: Rename `inputs/` → `Inputs/` (0.5 pt)
- Move folder from `CoreEssentials/src/inputs/` to `CoreEssentials/src/Inputs/`
- Verify namespace `CoreEssentials.Inputs` already matches ✅

### T5b: Validate After Inputs Rename (0.5 pt) 🔁
- Run `dotnet build core-essentials-monogame.sln -c Release` — must compile with zero errors
- Run `dotnet test CoreEssentials.Tests/Inputs/ --no-build` — input tests must pass

---

## Acceptance Criteria

- [x] Folders renamed: `Assets/`, `Debugging/`, `GameSystems/`, `GUI/`, `Inputs/` (all PascalCase)
- [x] No namespace changes required for these folders
- [x] Solution builds cleanly with zero errors after each rename
- [x] All affected test suites pass after each rename

---

## Deliverables

- 5 folders renamed to PascalCase under `CoreEssentials/src/`
- Clean build and passing tests after every change
