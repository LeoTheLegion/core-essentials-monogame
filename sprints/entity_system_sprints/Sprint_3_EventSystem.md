# Sprint 3 — Event System 📡

**Points:** 5  
**Status:** ✅ Completed  
**Sprint Goal:** Add decoupled publish/subscribe so entities don't need direct references.

---

## Tasks

- [x] **T1: Create `EntityEventArgs<T>` class (1 pt)** 🔒 Internal
  - Generic event data container with `Source` (sender entity) and `Data` (payload)
  - Non-generic `EntityEventArgs` for simple events
  - Thread-safe event data encapsulation

- [x] **T2: Create `EntityEventSystem` class (2 pts)** ⭐ User-facing
  - Global event registry with string-based event names
  - `Subscribe(string eventName, Action<EntityEventArgs> handler)`
  - `Publish(string eventName, EntityEventArgs data)`
  - Entity-scoped subscriptions (auto-unsubscribe on entity destroy)

- [x] **T3: Add event convenience methods to `Entity` (1 pt)** ⭐ User-facing
  - `Subscribe(eventName, handler)` — subscribe with auto-cleanup
  - `Publish(eventName, data)` — publish from entity
  - `Unsubscribe(eventName, handler)` — manual unsubscribe
  - Auto-unsubscribe on `OnDestroy()`

- [x] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test subscribe/publish cycle
  - Test entity-scoped subscriptions
  - Test auto-unsubscribe on entity destroy
  - Test multiple handlers for same event

---

## Acceptance Criteria

- [ ] `EntityEventSystem` manages global event registry
- [ ] Entities can subscribe and publish events
- [ ] Subscriptions auto-clean when entity is destroyed
- [ ] Event data can be generic or non-generic
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new event tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Events/EntityEventArgs.cs` | New | 🔒 Internal | Event data container |
| `Events/EntityEventSystem.cs` | New | ⭐ PUBLIC | Global event registry |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add subscribe/publish methods |
| `EntityEventTests.cs` | New | 🔒 Internal | Unit tests for event system |

---

## Notes & Risks

- **Medium risk** — event naming collisions should be documented
- Consider event priorities for ordering
- Memory leak risk if subscriptions aren't properly cleaned up

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
