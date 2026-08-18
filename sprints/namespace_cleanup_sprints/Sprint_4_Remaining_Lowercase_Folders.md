# Sprint 4: Remaining Lowercase Inner Folders

## Overview 📋

| Item | Detail |
|------|--------|
| **Sprint Number** | 4 |
| **Goal** | Rename remaining lowercase inner folders (`aether/`, `engines/`, `myra/`) to PascalCase |
| **Points** | 3 |
| **Status** | ✅ Complete |

## Background

After completing Sprints 0-3, a final scan revealed 3 nested folders still using lowercase names. These are framework/engine-specific folder names (Aether physics engine, Myra GUI engine) that should follow PascalCase for consistency with the rest of the codebase.

| Folder Path | Issue |
|-------------|-------|
| `GameSystems/Physics/Engines/aether/` | ❌ lowercase `aether` → should be `Aether` |
| `GUI/engines/` | ❌ lowercase `engines` → should be `Engines` |
| `GUI/engines/myra/` | ❌ lowercase `myra` → should be `Myra` |

## Task Breakdown

### T1: Rename `aether/` → `Aether/` (1 pt) ✅
- Move folder from `CoreEssentials/src/GameSystems/Physics/Engines/aether/` to `CoreEssentials/src/GameSystems/Physics/Engines/Aether/`
- Namespace `CoreEssentials.GameSystems.Physics.Engines.Aether.*` already matches ✅

### T2: Rename `engines/` → `Engines/` under GUI (1 pt) ✅
- Move folder from `CoreEssentials/src/GUI/engines/` to `CoreEssentials/src/GUI/Engines/`
- Namespace `CoreEssentials.GUI.Engines.*` already matches ✅

### T3: Rename `myra/` → `Myra/` under GUI/Engines (1 pt) ✅
- Move folder from `CoreEssentials/src/GUI/engines/myra/` to `CoreEssentials/src/GUI/Engines/Myra/`
- Namespace `CoreEssentials.GUI.Engines.Myra.*` already matches ✅

## Execution Notes

Windows requires a temporary intermediate path for casing-only renames since the filesystem is case-insensitive:

```powershell
# Rename myra → Myra (before renaming its parent)
Move-Item -Path "GUI/engines/myra" -Destination "GUI/engines/_tmp_myra"
Move-Item -Path "GUI/engines/_tmp_myra" -Destination "GUI/engines/Myra"

# Rename engines → Engines
Move-Item -Path "GUI/engines" -Destination "GUI/_tmp_engines"
Move-Item -Path "GUI/_tmp_engines" -Destination "GUI/Engines"

# Rename aether → Aether
Move-Item -Path "Physics/Engines/aether" -Destination "Physics/Engines/_tmp_aether"
Move-Item -Path "Physics/Engines/_tmp_aether" -Destination "Physics/Engines/Aether"
```

## Code Changes Required

None — all namespaces already match the target PascalCase folder names, and `.csproj` files use glob-based auto-inclusion rather than hardcoded paths.

## Results Summary

| Metric | Result |
|--------|--------|
| Build (Release) | ✅ 0 errors, 0 warnings |
| Tests | ✅ 364 passed, 2 skipped, 0 failed |
| Lowercase folders remaining | ✅ 0 |
| Points earned | 3/3 |

## Acceptance Criteria

- [x] All three lowercase inner folders renamed to PascalCase
- [x] No code changes required (namespaces already match)
- [x] Solution builds cleanly with zero errors
- [x] All 364 tests pass (2 skipped — same as baseline)
- [x] Zero lowercase folder names remain anywhere under `CoreEssentials/src/`
