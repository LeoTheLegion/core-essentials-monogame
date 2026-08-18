# Sprint 2 — GameSystems Inner Folder Renaming 📋

**Points:** 5  
**Status:** ✅ Complete  
**Prerequisite:** Sprints 0 & 1 must be completed first  
**Sprint Goal:** Rename inner folders under `GameSystems/` to PascalCase. Namespaces already match — only folder casing needs fixing.

---

## Results Summary

| Task | Folder | Build | Tests | Status |
|------|--------|-------|-------|--------|
| T2+T3 | `entitySystems/entityOOPsystem` → `EntitySystems/EntityOOPSystem` | ✅ | 364 passed | ✅ |
| T4+T5 | `physics/types`, `physics/engines` → `Physics/Types`, `Physics/Engines` | ✅ | 364 passed | ✅ |

**Final Validation:** Full test suite — 366 total, 364 succeeded, 2 skipped (matches baseline)

---

## Tasks

### T1: Rename `entitySystems/` → `EntitySystems/` (1 pt) ✅
- Move folder from `CoreEssentials/src/GameSystems/entitySystems/` to `CoreEssentials/src/GameSystems/EntitySystems/`
- Namespace `CoreEssentials.GameSystems.EntitySystems.*` already matches ✅

### T2: Rename `entityOOPsystem/` → `EntityOOPSystem/` (1 pt) ✅
- Move folder from `CoreEssentials/src/GameSystems/entitySystems/entityOOPsystem/` to `CoreEssentials/src/GameSystems/EntitySystems/EntityOOPSystem/`
- Namespace `CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem` already matches ✅

### T3: Rename `physics/` → `Physics/` (1 pt) ✅
- Move folder from `CoreEssentials/src/GameSystems/physics/` to `CoreEssentials/src/GameSystems/Physics/`
- Namespace `CoreEssentials.GameSystems.Physics.*` already matches ✅

### T4: Rename `types/` & `engines/` under Physics (1 pt) ✅
- Move `CoreEssentials/src/GameSystems/physics/types/` → `CoreEssentials/src/GameSystems/Physics/Types/`
- Move `CoreEssentials/src/GameSystems/physics/engines/` → `CoreEssentials/src/GameSystems/Physics/Engines/`

### T5: Final Build & Test Validation (1 pt) ✅ ⭐ Critical
- Run `dotnet build core-essentials-monogame.sln -c Release` — **0 errors** ✅
- Run `dotnet test CoreEssentials.Tests/ --no-build` — **364 passed, 2 skipped, 0 failed** ✅

---

## Affected File Summary

| Change | Source Files | Playground Files | Test Files |
|--------|-------------|------------------|------------|
| `entitySystems/` → `EntitySystems/` | entityOOPsystem/* (2 files) | — | 1 folder rename in Tests |
| `physics/` → `Physics/` + inner | physics/types/*, physics/engines/* | Ball.cs, PhysicsEntityScene.cs, WorldBorder.cs | 7 test files under Physics/ |

---

## Acceptance Criteria

- [x] All inner folders under `GameSystems/` use PascalCase
- [x] No namespace changes required (namespaces already match)
- [x] Solution builds cleanly with zero errors
- [x] All 364 tests pass
- [x] No broken references in Playground or Tests projects
