# Sprint 6 — Lightweight Components 🧩

**Points:** 6  
**Status:** Not Started  
**Sprint Goal:** Add mixin-style component system for composable entity behavior.

---

## Tasks

- [x] **T1: Create `EntityComponent` base class (1 pt)** ⭐ User-facing
  - Base class with `Entity Owner` reference
  - `OnAttach()` and `OnDetach()` lifecycle hooks
  - `Update(GameTime)` optional override

- [x] **T2: Add component management to `Entity` (2 pts)** ⭐ User-facing
  - `Dictionary<Type, EntityComponent>` for component storage
  - `AddComponent<T>(T component)` method
  - `GetComponent<T>()` method
  - `HasComponent<T>()` method
  - Auto-call lifecycle hooks on attach/detach

- [ ] **T3: Create `RigidbodyComponent` (1 pt)** ⭐ User-facing
  - Wraps `IPhysicsBody` with sync to Entity Position/Rotation
  - Body type (Static, Kinematic, Dynamic)
  - Auto-sync physics body transform ↔ entity transform on attach/update
  - Lazy body creation via `PhysicsEngine` GameSystem

- [ ] **T4: Create `SpriteComponent` (1 pt)** ⭐ User-facing
  - Decouples rendering from Entity
  - Texture, scale, origin, color, effect (flip)
  - Sort order override
  - Optional: animation frame support

- [x] **T5: Write unit tests (1 pt)** 🔁 Validation
  - Test add/get/remove components
  - Test component lifecycle hooks
  - Test component cleanup on entity destroy
  - Test duplicate component handling

---

## Acceptance Criteria

- [ ] `EntityComponent` base class with lifecycle hooks
- [ ] Entities can add, get, and remove components
- [ ] Built-in components (Health, Velocity, Damage) are available
- [ ] Components are cleaned up when entity is destroyed
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new component tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Components/EntityComponent.cs` | New | ⭐ PUBLIC | Base component class |
| `Components/ComponentSystem.cs` | New | 🔒 Internal | Component management |
| `Components/BuiltIn/RigidbodyComponent.cs` | New | ⭐ PUBLIC | Wraps `IPhysicsBody`, syncs transform ↔ entity |
| `Components/BuiltIn/SpriteComponent.cs` | New | ⭐ PUBLIC | Decouples rendering (texture, scale, origin, color, effect, sort) |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add component management |
| `EntityComponentTests.cs` | New | 🔒 Internal | Unit tests for components |

---

## Notes & Risks

- **Medium risk** — component order matters for some use cases
- Consider component priority for update ordering
- Memory overhead for component dictionaries
- **Hybrid rendering approach** — `Entity.Draw()` remains the default render path for backward compatibility. Components like `SpriteComponent` provide *additional* rendering options. Future sprint will enforce full component-based rendering.

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
