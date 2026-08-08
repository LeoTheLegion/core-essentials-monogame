# Sprint 9 — Delayed Lifecycle ⏱️

**Points:** 3  
**Status:** Not Started  
**Sprint Goal:** Built-in spawn/destroy/respawn scheduling on coroutines.

---

## Tasks

- [ ] **T1: Add `DestroyAfter()` to `Entity` (1 pt)** ⭐ User-facing
  - `DestroyAfter(TimeSpan delay)` method
  - Uses existing coroutine system for timing
  - Cancelable before delay expires

- [ ] **T2: Add `SpawnAfter()` to `EntitySystem` (1 pt)** ⭐ User-facing
  - `SpawnAfter<T>(position, TimeSpan delay)` method
  - Uses coroutine system for timing
  - Cancelable before delay expires

- [ ] **T3: Add `RespawnAt()` to `Entity` (0.5 pt)** ⭐ User-facing
  - `RespawnAt(Vector2 position, TimeSpan delay)` method
  - Stores original position for respawn
  - Auto-respawn after destroy and delay

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test destroy after delay
  - Test spawn after delay
  - Test respawn at position
  - Test cancellation of delayed operations

---

## Acceptance Criteria

- [ ] `DestroyAfter()` schedules entity destruction
- [ ] `SpawnAfter()` schedules entity creation
- [ ] `RespawnAt()` schedules entity respawn
- [ ] Delayed operations can be cancelled
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new lifecycle tests added

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
