# Sprint 3 — GUI Inner Folder Renaming 📋

**Points:** 3  
**Status:** ✅ Complete  
**Prerequisite:** Sprints 0, 1 & 2 must be completed first  
**Sprint Goal:** Rename inner folders under `GUI/` to PascalCase. Namespaces already match — only folder casing needs fixing.

---

## Results Summary

| Task | Folder | Build | Tests | Status |
|------|--------|-------|-------|--------|
| T1 | `types/` → `Types/` | ✅ | 364 passed | ✅ |
| T2 | `factory/` → `Factory/` | ✅ | 364 passed | ✅ |

**Final Validation:** Full test suite — 366 total, 364 succeeded, 2 skipped (matches baseline)

---

## Tasks

### T1: Rename `types/` → `Types/` (1 pt)
- Move folder from `CoreEssentials/src/GUI/types/` to `CoreEssentials/src/GUI/Types/`
- Namespace `CoreEssentials.GUI.Types` already matches ✅

### T2: Rename `factory/` → `Factory/` (1 pt)
- Move folder from `CoreEssentials/src/GUI/factory/` to `CoreEssentials/src/GUI/Factory/`
- Namespace `CoreEssentials.GUI.Factory` already matches ✅

### T3: Final Build & Test Validation (1 pt) ⭐ Critical
- Run `dotnet build core-essentials-monogame.sln -c Release` — must compile with zero errors
- Run `dotnet test CoreEssentials.Tests/ --no-build` — **all 364 tests** must pass

---

## Note: Already PascalCase (no changes needed)

| Folder | Status |
|--------|--------|
| `engines/myra/Brushes/` | ✅ already PascalCase |
| `engines/myra/Widgets/` | ✅ already PascalCase |
| `Internal/` | ✅ already PascalCase |

---

## Acceptance Criteria

- [ ] All inner folders under `GUI/` use PascalCase
- [ ] No namespace changes required (namespaces already match)
- [ ] Solution builds cleanly with zero errors
- [ ] All 364 tests pass
