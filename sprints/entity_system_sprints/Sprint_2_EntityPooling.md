# Sprint 2 — Entity Pooling ♻️

**Points:** 5  
**Status:** ✅ Completed  
**Sprint Goal:** Recycle destroyed entities instead of letting GC collect them for high-spawn-rate scenarios.

---

## Tasks

- [x] **T1: Create `IPooledEntity` interface (1 pt)** ⭐ User-facing
  - Define `Reset()` method for entity state reset
  - Define `Activate(Vector2)` method for activation at position
  - Opt-in design — entities implement interface to be poolable
  - Add documentation comments for pool lifecycle

- [x] **T2: Create `EntityPool<T>` class (2 pts)** 🔒 Internal
  - Generic pool with `Stack<T>` for recycled instances
  - `Acquire(Vector2)` returns pooled instance or creates new one
  - `Release(T)` returns entity to pool
  - Configurable initial capacity
  - Properties: TotalCount, AvailableCount, ActiveCount

- [x] **T3: Add pool-aware methods to `EntitySystem` (1 pt)** ⭐ User-facing
  - `CreatePooled<T>()` creates pooled entity and adds to system
  - `ReleasePooled<T>()` removes from system and returns to pool
  - Lazy pool initialization via `GetOrCreatePool<T>()`

- [x] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test pool acquire/release cycle
  - Test entity state reset on release
  - Test EntitySystem pooled entity creation and release
  - Test pool statistics

---

## Acceptance Criteria

- [x] `IPooledEntity` interface defines pool lifecycle
- [x] `EntityPool<T>` manages entity recycling
- [x] `CreatePooled<T>()` and `ReleasePooled<T>()` work correctly
- [x] Pool doesn't grow beyond configured max size
- [x] Project builds cleanly — **0 errors, 0 warnings**
- [x] All existing tests pass + new pooling tests added (19 new tests, 432 total passing)

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

### Design Decisions

1. **Opt-in via `IPooledEntity`** — Not all entities are pooled. Entities must explicitly implement `IPooledEntity` to opt into pooling. This avoids unexpected behavior for entities that don't need recycling.
2. **No `IsActive` property** — The interface doesn't define `IsActive` since the base `Entity` class already has `SetActive()`/`GetActive()` methods. Entities use `SetActive(false)` when released to the pool.
3. **`new()` constraint** — All generic constraints include `new()` so the pool can pre-create instances at initialization.
4. **Lazy pool initialization** — `EntitySystem.GetOrCreatePool<T>()` creates pools on first call to `CreatePooled<T>()`, avoiding unnecessary allocations for unused entity types.

### Implementation Summary

| Component | File | Key Methods |
|-----------|------|-------------|
| `IPooledEntity` | `Pooling/IPooledEntity.cs` | `Reset()`, `Activate(Vector2)` |
| `EntityPool<T>` | `Pooling/EntityPool.cs` | `Acquire(Vector2)`, `Release(T)`, TotalCount, AvailableCount, ActiveCount |
| `EntitySystem` | `EntitySystem.cs` | `CreatePooled<T>()`, `ReleasePooled<T>()`, `GetOrCreatePool<T>()` |

### Testing

- **19 new tests** in `EntityPoolTests.cs` covering:
  - `TestPooledEntity` helper class implementing `IPooledEntity`
  - Pool acquire/release cycles
  - Entity state reset verification
  - EntitySystem pooled entity lifecycle
  - Pool statistics accuracy

*Created: 2026-08-07 | Completed: 2026-08-07 | Part of Entity System Enhancements Project*
