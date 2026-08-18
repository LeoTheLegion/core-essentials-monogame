# Namespace & Folder Structure Cleanup — Scrum Sprints 🚀

This folder contains sprint plans for cleaning up the namespace and folder structure inconsistencies across the CoreEssentials source code.

## Why This Cleanup? ⚠️

The `CoreEssentials/src/` directory has **inconsistent folder casing** and a few **namespace-to-folder mismatches**:

| Folder | Namespace | Issue |
|--------|-----------|-------|
| `assets/` | `CoreEssentials.Assets` | ❌ lowercase vs PascalCase |
| `debugging/` | `CoreEssentials.Debugging` | ❌ lowercase vs PascalCase |
| `gameSystems/` | `CoreEssentials.GameSystems.*` | ❌ camelCase vs PascalCase |
| `gui/` | `CoreEssentials.GUI.*` | ❌ lowercase vs PascalCase |
| `inputs/` | `CoreEssentials.Inputs` | ❌ lowercase vs PascalCase |
| `scene/` | `CoreEssentials.SceneManagement` | ❌ lowercase + name mismatch |
| `Camera/` | `CoreEssentials.Cameras` | ⚠️ plural mismatch (singular folder → plural namespace) |

**6 out of 10 folders don't match their namespace casing.** This makes the codebase harder to navigate and inconsistent with C# conventions.

---

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 📋 [Sprint 0](Sprint_0_Folder_Renaming.md) | Folder Renaming (Simple) | 7 | ✅ Complete | Renamed 5 folders to PascalCase where namespace already matches — no code changes needed |
| 🔧 [Sprint 1](Sprint_1_Namespace_Fixes.md) | Namespace Fixes & Test Updates | 7 | ✅ Complete | Fixed `Cameras`→`Camera`, `SceneManager`→`Scenes` namespaces, updated Playground + Tests |
| 📁 [Sprint 2](Sprint_2_GameSystems_Inner_Folders.md) | GameSystems Inner Folders | 5 | ✅ Complete | Renamed inner folders under `GameSystems/` to PascalCase (EntitySystems/, Physics/, Types/, Engines/) — no code changes needed |
| 🎨 [Sprint 3](Sprint_3_GUI_Inner_Folders.md) | GUI Inner Folders | 3 | ✅ Complete | Renamed inner folders under `GUI/` to PascalCase (Factory/, Types/) — no code changes needed |
| 🔍 [Sprint 4](Sprint_4_Remaining_Lowercase_Folders.md) | Remaining Lowercase Folders | 3 | ✅ Complete | Renamed `aether/`, `engines/`, `myra/` to PascalCase — zero lowercase folders remain |

---

## Target Structure ✅

```
CoreEssentials/src/
├── MainGame.cs                        ← CoreEssentials
├── AssemblyInfo.cs                    ← (assembly only)
├── Assets/                            ← CoreEssentials.Assets ✅
├── Audio/                             ← CoreEssentials.Audio ✅
├── Camera/                            ← CoreEssentials.Camera ✅ (fixed pluralization)
├── Coroutines/                        ← CoreEssentials.Coroutines ✅
├── Debugging/                         ← CoreEssentials.Debugging ✅
├── GameSystems/                       ← CoreEssentials.GameSystems.* ✅
│   ├── EntitySystems/
│   │   └── EntityOOPSystem/          ← CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem
│   └── Physics/                      ← CoreEssentials.GameSystems.Physics.*
│       ├── Types/
│       └── Engines/Aether/
├── GUI/                               ← CoreEssentials.GUI.* ✅
│   ├── Types/
│   ├── Factory/
│   ├── Internal/
│   └── Engines/Myra/
├── Inputs/                            ← CoreEssentials.Inputs ✅
├── Scene/                             ← CoreEssentials.Scenes ✅ (fixed from SceneManager; plural to avoid type conflict)
└── Timing/                            ← CoreEssentials.Timing ✅
```

---

## Acceptance Criteria (Overall)

- [x] All source folders use PascalCase matching their namespace prefix
- [x] `CoreEssentials.Cameras` renamed to `CoreEssentials.Camera` (singular, consistent with all other subsystems)
- [x] `CoreEssentials.SceneManagement` renamed to `CoreEssentials.Scenes` (plural to avoid type/namespace conflict)
- [x] Solution builds cleanly with zero errors
- [x] All 364 tests pass (2 skipped — same as baseline)
- [x] No usings or references are broken across CoreEssentials, Playground, or Tests

## Results Summary

| Sprint | Points | Status | Build | Tests |
|--------|--------|--------|-------|-------|
| Sprint 0 | 7 | ✅ Complete | Clean | 364 passed, 2 skipped |
| Sprint 1 | 7 | ✅ Complete | Clean (0 errors) | 364 passed, 2 skipped |
| Sprint 2 | 5 | ✅ Complete | Clean (0 errors) | 364 passed, 2 skipped |
| Sprint 3 | 3 | ✅ Complete | Clean (0 errors) | 364 passed, 2 skipped |
| Sprint 4 | 3 | ✅ Complete | Clean (0 errors) | 364 passed, 2 skipped |

**Total: 25 points earned — all namespace & folder cleanup complete!** 🎉
