# Sprint 1 — Entity Query API 🔍

**Points:** 4  
**Status:** ✅ Completed  
**Completed:** 2026-08-07  
**Sprint Goal:** Add convenient lookup methods on `EntitySystem` for finding entities by type, position, and tag.

**Dependencies:** Sprint 0 (Entity Tags) — `FindByTag` needs the tagging system.

---

## Tasks

- [x] **T1: Implement `FindByType<T>()` (1 pt)** ⭐ User-facing
  - Add `List<T> FindByType<T>() where T : Entity` method to `EntitySystem`
  - Return all active entities of the specified type

- [x] **T2: Implement `FindNearby()` (2 pts)** ⭐ User-facing
  - Add `List<Entity> FindNearby(Vector2 position, float radius)` method
  - Use distance-based filtering (optimized with squared distance comparison)
  - Add overload `FindNearby<T>()` for type-filtered nearby queries

- [x] **T3: Implement `FindByTag()` (0.5 pt)** ⭐ User-facing
  - ~~Add `List<Entity> FindByTag(string tag)` convenience method~~ — **Already implemented in Sprint 0**

- [x] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test `FindByType` returns correct entities, filters inactive
  - Test `FindNearby` with various positions and radii
  - Test `FindNearby<T>` combines type and spatial filtering
  - Test edge cases (empty system, single entity, exact boundary)

---

## Acceptance Criteria

- [x] `FindByType<T>()` returns all active entities of type T
- [x] `FindNearby()` returns entities within the specified radius
- [x] `FindNearby<T>()` combines type and spatial filtering
- [x] `FindByTag()` returns entities with the specified tag
- [x] Project builds cleanly — **0 errors, 0 warnings**
- [x] All existing tests pass + new query tests added (**16 query tests passed**)

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Added `FindByType<T>`, `FindNearby()`, `FindNearby<T>()` methods |
| `EntityQueryTests.cs` | New | 🔒 Internal | 16 unit tests for query functionality |

---

## Notes & Risks

- **Low risk** — additive feature building on Sprint 0
- Performance consideration: `FindNearby` is O(n) without spatial partitioning (Sprint 7). This is acceptable for now, but spatial partitioning will optimize it later
- `FindByTag()` was already implemented during Sprint 0, so T3 required no new code

---

*Created: 2026-08-07 | Completed: 2026-08-07 | Part of Entity System Enhancements Project*
