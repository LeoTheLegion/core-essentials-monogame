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
| 📋 [Sprint 0](Sprint_0_Folder_Renaming.md) | Folder Renaming (Simple) | 7 | Not Started | Rename 5 folders to PascalCase where namespace already matches — no code changes needed |
| 🔧 [Sprint 1](Sprint_1_Namespace_Fixes.md) | Namespace Fixes & Test Updates | 7 | Not Started | Fix `Cameras`→`Camera`, `SceneManager`→`Scene` namespaces, update Playground + Tests |

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
│   ├── entitySystems/
│   │   └── EntityOOPSystem/          ← CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem
│   └── Physics/                      ← CoreEssentials.GameSystems.Physics.*
│       ├── types/
│       └── engines/aether/
├── GUI/                               ← CoreEssentials.GUI.* ✅
│   ├── types/
│   ├── factory/
│   ├── Internal/
│   └── engines/myra/
├── Inputs/                            ← CoreEssentials.Inputs ✅
├── Scene/                             ← CoreEssentials.Scene ✅ (fixed from SceneManager)
└── Timing/                            ← CoreEssentials.Timing ✅
```

---

## Acceptance Criteria (Overall)

- [ ] All source folders use PascalCase matching their namespace prefix
- [ ] `CoreEssentials.Cameras` renamed to `CoreEssentials.Camera` (singular, consistent with all other subsystems)
- [ ] `CoreEssentials.SceneManagement` renamed to `CoreEssentials.Scene` (matches folder name)
- [ ] Solution builds cleanly with zero errors
- [ ] All 364 tests pass
- [ ] No usings or references are broken across CoreEssentials, Playground, or Tests
