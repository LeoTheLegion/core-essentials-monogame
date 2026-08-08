# Sprint 16 — Entity Lifecycle Hooks 🔄

**Points:** 3  
**Status:** Not Started  
**Sprint Goal:** Additional lifecycle events for fine-grained control (OnEnable, OnDisable, OnPause, OnResume, OnAwake).

---

## Tasks

- [ ] **T1: Add lifecycle hooks to `Entity` (1.5 pts)** ⭐ User-facing
  - `OnAwake()` — called after entity is added to system (before OnStart)
  - `OnEnable()` — called when SetActive(true) is called
  - `OnDisable()` — called when SetActive(false) is called
  - `OnPause()` — called when entity is paused
  - `OnResume()` — called when entity is unpaused

- [ ] **T2: Add lifecycle management to `EntitySystem` (1 pt)** ⭐ User-facing
  - Call `OnAwake()` when entity is added
  - Track enabled/disabled state
  - Track paused/resumed state
  - Respect lifecycle in update loop

- [ ] **T3: Write unit tests (1 pt)** 🔁 Validation
  - Test OnAwake is called on entity creation
  - Test OnEnable/OnDisable are called correctly
  - Test OnPause/OnResume are called correctly
  - Test lifecycle order

---

## Acceptance Criteria

- [ ] `OnAwake()` is called after entity is added to system
- [ ] `OnEnable()` and `OnDisable()` are called on active state change
- [ ] `OnPause()` and `OnResume()` are called on pause state change
- [ ] Lifecycle hooks respect entity state
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new lifecycle tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Entity.cs` | Modified | ⭐ PUBLIC | Add lifecycle hooks |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add lifecycle management |
| `EntityLifecycleTests.cs` | New | 🔒 Internal | Unit tests for lifecycle hooks |

---

## Notes & Risks

- **Low risk** — additive feature with no breaking changes
- Need to document lifecycle order clearly
- Consider virtual vs. interface for lifecycle hooks

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
