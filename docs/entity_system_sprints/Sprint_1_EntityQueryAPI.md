# Sprint 1 — Entity Query API 🔍

**Points:** 4  
**Status:** Not Started  
**Sprint Goal:** Add convenient lookup methods on `EntitySystem` for finding entities by type, position, and tag.

**Dependencies:** Sprint 0 (Entity Tags) — `FindByTag` needs the tagging system.

---

## Tasks

- [ ] **T1: Implement `FindByType<T>()` (1 pt)** ⭐ User-facing
  - Add `List<T> FindByType<T>() where T : Entity` method to `EntitySystem`
  - Return all active entities of the specified type
  - Consider caching type index for performance

- [ ] **T2: Implement `FindNearby()` (2 pts)** ⭐ User-facing
  - Add `List<Entity> FindNearby(Vector2 position, float radius)` method
  - Use distance-based filtering (optimized with squared distance comparison)
  - Add overload `FindNearby<T>()` for type-filtered nearby queries
  - Consider bounding box pre-filter for early rejection

- [ ] **T3: Implement `FindByTag()` (0.5 pt)** ⭐ User-facing
  - Add `List<Entity> FindByTag(string tag)` convenience method (reuses Sprint 0 tag index)

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test `FindByType` returns correct entities, filters inactive
  - Test `FindNearby` with various positions and radii
  - Test `FindNearby<T>` combines type and spatial filtering
  - Test edge cases (empty system, single entity, exact boundary)

---

## Acceptance Criteria

- [ ] `FindByType<T>()` returns all active entities of type T
- [ ] `FindNearby()` returns entities within the specified radius
- [ ] `FindNearby<T>()` combines type and spatial filtering
- [ ] `FindByTag()` returns entities with the specified tag
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new query tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add `FindByType`, `FindNearby`, `FindByTag` methods |
| `EntityQueryTests.cs` | New | 🔒 Internal | Unit tests for query functionality |

---

## Notes & Risks

- **Low risk** — additive feature building on Sprint 0
- Performance consideration: `FindNearby` is O(n) without spatial partitioning (Sprint 7). This is acceptable for now, but spatial partitioning will optimize it later
- Consider whether queries should return `IEnumerable<T>` instead of `List<T>` for lazy evaluation

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
