# Sprint 9 — Delayed Lifecycle ⏱️

**Points:** 3  
**Status:** ✅ Complete  
**Sprint Goal:** Built-in spawn/destroy/respawn scheduling on coroutines.

---

## Tasks

- [x] **T1: Add `DestroyAfter()` to `Entity` (1 pt)** ⭐ User-facing
  - `DestroyAfter(TimeSpan delay)` method
  - Uses existing coroutine system for timing
  - Cancelable before delay expires

- [x] **T2: Add `SpawnAfter()` to `EntitySystem` (1 pt)** ⭐ User-facing
  - `SpawnAfter<T>(position, TimeSpan delay)` method
  - Uses coroutine system for timing
  - Cancelable before delay expires

- [x] **T3: Add `RespawnAt()` to `Entity` (0.5 pt)** ⭐ User-facing
  - `RespawnAt(Vector2 position, TimeSpan delay)` method
  - Stores original position for respawn
  - Auto-respawn after destroy and delay

- [x] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test destroy after delay
  - Test spawn after delay
  - Test respawn at position
  - Test cancellation of delayed operations

---

## Acceptance Criteria

- [x] `DestroyAfter()` schedules entity destruction
- [x] `SpawnAfter()` schedules entity creation
- [x] `RespawnAt()` schedules entity respawn
- [x] Delayed operations can be cancelled
- [x] Project builds cleanly — **0 errors, 0 warnings**
- [x] All existing tests pass + new lifecycle tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Entity.cs` | Modified | ⭐ PUBLIC | Add `DestroyAfter`, `RespawnAt` |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add `SpawnAfter` |
| `DelayedLifecycleTests.cs` | New | 🔒 Internal | Unit tests for delayed lifecycle |

---

## Notes & Risks

- **Low risk** — simple wrapper around coroutine system
- Need to handle entity pooling with delayed operations
- Memory leak risk if delayed operations aren't cleaned up

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
