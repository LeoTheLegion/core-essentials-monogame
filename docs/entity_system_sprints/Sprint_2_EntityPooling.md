# Sprint 2 — Entity Pooling ♻️

**Points:** 5  
**Status:** Not Started  
**Sprint Goal:** Recycle destroyed entities instead of letting GC collect them for high-spawn-rate scenarios.

---

## Tasks

- [ ] **T1: Create `IPooledEntity` interface (1 pt)** ⭐ User-facing
  - Define `Reset()` method for entity state reset
  - Define `IsActive` property for pool state tracking
  - Add documentation comments for pool lifecycle

- [ ] **T2: Create `EntityPool<T>` class (2 pts)** 🔒 Internal
  - Generic pool with `Stack<T>` for recycled instances
  - `Acquire()` returns pooled instance or creates new one
  - `Release(instance)` returns entity to pool
  - Configurable pool size (initial capacity, max size)
  - Thread-safe considerations for single-threaded game loop

- [ ] **T3: Add pool-aware methods to `EntitySystem` (1 pt)** ⭐ User-facing
  - `CreatePooled<T>()` returns pooled entity instance
  - `ReleasePooled<T>()` returns entity to pool instead of destroying
  - Auto-register entity types for pooling

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test pool acquire/release cycle
  - Test pool capacity limits
  - Test entity state reset on release
  - Test performance comparison (pooled vs non-pooled)

---

## Acceptance Criteria

- [ ] `IPooledEntity` interface defines pool lifecycle
- [ ] `EntityPool<T>` manages entity recycling
- [ ] `CreatePooled<T>()` and `ReleasePooled<T>()` work correctly
- [ ] Pool doesn't grow beyond configured max size
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new pooling tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Pooling/IPooledEntity.cs` | New | ⭐ PUBLIC | Pool lifecycle interface |
| `Pooling/EntityPool.cs` | New | 🔒 Internal | Generic entity pool |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add pool-aware methods |
| `EntityPoolTests.cs` | New | 🔒 Internal | Unit tests for pooling |

---

## Notes & Risks

- **Medium risk** — need to ensure `Reset()` properly clears all entity state
- Consider which entity types benefit most from pooling (projectiles, particles, effects)
- Pool size should be configurable per entity type

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
