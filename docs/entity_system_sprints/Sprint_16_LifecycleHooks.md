# Sprint 16 — Entity Lifecycle Hooks 🔄

**Points:** 5  
**Status:** In Progress  
**Sprint Goal:** Full Unity-style lifecycle alignment — add OnAwake, OnEnable, OnDisable, OnLateUpdate, OnFixedUpdate, and app-wide OnApplicationPause(bool) to the OOP Entity system.

**Naming decision:** Keep the existing `On`-prefixed convention (`OnStart`, `OnDestroy`) and extend it with `OnAwake`, `OnEnable`, `OnDisable`, `OnLateUpdate`, `OnFixedUpdate`, `OnApplicationPause`. No existing hooks are renamed — **no breaking changes**.

**Dependencies:** Sprint 13 (GameStateSerialization)

**Existing Lifecycle (from Sprint 13):**
```
Entity: OnStart() → Update() → OnDestroy()
Component: OnAttach() → Update() → OnDetach()
OnDestroy() calls OnDetach() on all components, nulls Owner, then auto-unsubscribes all event subscriptions.
Physics bodies are cleaned up via RigidbodyComponent.OnDetach() → DestroyBody().
```

**Target Lifecycle (after this sprint):**
```
OnAwake() → OnEnable() → OnStart()
   → [ OnFixedUpdate() (fixed timestep) → Update() → OnLateUpdate() ]*
   → OnApplicationPause(bool) (app-wide, any time)
   → OnDisable() → OnDestroy()
```

---

## Tasks

- [ ] **T1: Add lifecycle hooks to `Entity` (1.5 pts)** ⭐ User-facing
  - `OnAwake()` — called once when the entity is added to the system (before OnStart)
  - `OnEnable()` — called when `SetActive(true)` transitions the entity to active
  - `OnDisable()` — called when `SetActive(false)` transitions the entity to inactive
  - `OnLateUpdate(GameTime)` — called after `Update()` each frame, for active entities
  - `OnFixedUpdate(GameTime)` — called on the fixed timestep, for active entities
  - `OnApplicationPause(bool)` — called app-wide when the game window loses/gains focus
  - `SetActive(bool)` fires OnEnable/OnDisable only on real state transitions (no redundant calls)

- [ ] **T2: Add lifecycle management to `EntitySystem` (1.5 pts)** ⭐ User-facing
  - Call `OnAwake()` when an entity is added (all create paths: CreateEntity, CreateEntityUnstarted, CreatePooled, serializer/template instantiation)
  - Call `OnLateUpdate()` in the update loop after `Update()`
  - Implement `IFixedUpdateGameSystem` so `OnFixedUpdate()` runs on the fixed timestep
  - Implement `IPausableGameSystem` so `OnApplicationPause(bool)` reaches every entity

- [ ] **T3: Write unit tests (1 pt)** 🔁 Validation
  - Test OnAwake is called on entity creation
  - Test OnEnable/OnDisable fire on real SetActive transitions only (no redundant calls)
  - Test OnLateUpdate runs after Update each frame
  - Test OnFixedUpdate runs on the fixed timestep
  - Test OnApplicationPause(bool) reaches all active entities
  - Test full lifecycle order (Awake → Enable → Start → … → Disable → Destroy)

- [ ] **T4: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/EntityLifecycleHooks.md` user guide
  - Document lifecycle hook order
  - Document when each hook is called
  - Provide examples for each hook

---

## Acceptance Criteria

- [ ] `OnAwake()` is called after entity is added to system (all create paths)
- [ ] `OnEnable()` and `OnDisable()` fire on active state change (transitions only)
- [ ] `OnLateUpdate()` runs after `Update()` each frame for active entities
- [ ] `OnFixedUpdate()` runs on the fixed timestep for active entities
- [ ] `OnApplicationPause(bool)` is fired app-wide on window focus change
- [ ] Lifecycle hooks respect entity state (inactive entities skip per-frame hooks)
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new lifecycle tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Entity.cs` | Modified | ⭐ PUBLIC | Add lifecycle hooks |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add lifecycle management (IFixedUpdateGameSystem, IPausableGameSystem) |
| `GameSystem.cs` | Modified | ⭐ PUBLIC | Add `IPausableGameSystem` interface |
| `Scene.cs` | Modified | ⭐ PUBLIC | Route app-wide pause to pausable systems |
| `SceneManager.cs` | Modified | ⭐ PUBLIC | Route app-wide pause to current scene |
| `MainGame.cs` | Modified | ⭐ PUBLIC | Fire pause on window focus change |
| `EntityLifecycleTests.cs` | New | 🔒 Internal | Unit tests for lifecycle hooks |
| `docs/EntityLifecycleHooks.md` | New | ⭐ PUBLIC | User guide for lifecycle hooks |

---

## Comparison with Unity

| Unity hook | Our hook | Notes |
|-----------|----------|-------|
| `Awake()` | `OnAwake()` | Unity: on instantiation. Ours: on add-to-system |
| `OnEnable()` | `OnEnable()` | Ours fires only on real transitions (Unity fires every toggle) |
| `Start()` | `OnStart()` | Unchanged |
| `Update()` | `Update()` | Unchanged |
| `LateUpdate()` | `OnLateUpdate()` | New |
| `FixedUpdate()` | `OnFixedUpdate()` | New; driven by `IFixedUpdateGameSystem` |
| `OnDisable()` | `OnDisable()` | Ours fires only on real transitions |
| `OnDestroy()` | `OnDestroy()` | Unchanged |
| `OnApplicationPause(bool)` | `OnApplicationPause(bool)` | App-wide (window focus), matching Unity semantics |

Deliberate divergences:
- **Naming:** we keep the `On` prefix for consistency with existing `OnStart`/`OnDestroy`.
- **Enable/disable:** we guard against redundant hook calls on no-op `SetActive`; Unity does not.
- **Pause:** app-wide via `OnApplicationPause(bool)`, matching Unity (not per-entity).

## Notes & Risks

- **Low risk** — additive feature with no breaking changes (no existing hooks renamed)
- Need to document lifecycle order clearly
- All hooks are `virtual` (consistent with existing `OnStart`/`OnDestroy`)
- App-wide pause requires threading a new `IPausableGameSystem` through Scene → SceneManager → MainGame

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
