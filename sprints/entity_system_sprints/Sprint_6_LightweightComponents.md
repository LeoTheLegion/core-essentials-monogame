# Sprint 6 — Lightweight Components 🧩

**Points:** 10  
**Status:** In Progress  
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

- [x] **T3: Create `RigidbodyComponent` (1 pt)** ⭐ User-facing
  - Wraps `IPhysicsBody` with sync to Entity Position/Rotation
  - Body type (Static, Kinematic, Dynamic)
  - **Lazy body creation** — body created on first `Body` access or `Update()`, not in constructor
  - **Mass & FixedRotation properties** with auto-sync to underlying body
  - **Dynamic defaults to `SyncFromPhysics = true`** in constructor (physics drives entity)
  - Helper methods: `ApplyImpulse()`, `SetLinearVelocity()`, `AngularVelocity` getter/setter, `ApplyAngularImpulse()`
  - No manual `.Body` access needed — component is self-contained

- [x] **T4: Create `SpriteComponent` (1 pt)** ⭐ User-facing
  - Hybrid rendering component (additional draw path alongside Entity.Render)
  - Sprite, scale, origin, color, effect (flip), layer depth
  - Sort order override
  - Animation frame support

- [x] **T5: Write unit tests (1 pt)** 🔁 Validation
  - Test add/get/remove components
  - Test component lifecycle hooks
  - Test component cleanup on entity destroy
  - Test duplicate component handling

- [x] **T7: Create `ColliderComponent` (2 pts)** ⭐ User-facing
  - Wraps `ICollider` with shape creation helpers
  - Requires `RigidbodyComponent` on the same entity
  - Circle, Rectangle, Polygon shape factories
  - Friction, Restitution, Offset properties
  - Collision event forwarding (OnCollision, OnSeparation)
  - Auto-creates collider on RigidbodyComponent's body in `OnAttach()`

- [x] **T8: Refactor `Ball.cs` to use components (1 pt)** 🔧 Playground demo
  - All component setup moved to `OnStart()` (constructor only sets Position, sort, scale)
  - No manual `Update()` override — `RigidbodyComponent.Update()` handles physics→entity sync
  - No `.Body` access — uses `ApplyImpulse()`, `Mass`, etc. through component
  - Angular impulse applied for visible rotation on soccer ball sprite
  - Scene accesses component via standard `GetComponent<RigidbodyComponent>()` API (no Ball-specific property needed)

- [x] **T9: Write unit tests for ColliderComponent (1 pt)** 🔁 Validation
  - Test collider creation (circle, rectangle, polygon)
  - Test dependency on RigidbodyComponent
  - Test friction/restitution properties
  - Test collider cleanup on detach
  - **25 tests** covering construction, shape types, property defaults, CreateCollider/DestroyCollider lifecycle, and scene integration

---

## Acceptance Criteria

- [x] `EntityComponent` base class with lifecycle hooks
- [x] Entities can add, get, and remove components
- [x] Built-in components (Rigidbody, Sprite, Collider) are available
- [x] Components are cleaned up when entity is destroyed
- [x] `Entity.Update()` automatically calls `component.Update()` for all attached components
- [x] Components are self-contained — no manual transform sync, no `.Body` access, no `CreateBody()` calls
- [x] Project builds cleanly — **0 errors** (5 nullable warnings in RigidbodyComponent from `_body` after `EnsureBody()`, false positives)
- [x] All 529 tests pass (+ new component tests: 18 EntityComponent + 25 ColliderComponent = 43 new tests, up from 504 baseline)

---

## Key Decisions

1. **Lazy body creation** — `OnAttach()` fires during `AddComponent()` before entity is in EntitySystem, so body creation moved to lazy initialization via `Body` getter and `Update()`.
2. **`Entity.Update()` iterates components** — Instead of EntitySystem calling component updates separately, `Entity.Update()` loops `_components.Values` and calls each component's `Update(gameTime)`. Keeps it per-entity and avoids duplication.
3. **Dynamic → SyncFromPhysics by default** — Constructor sets `SyncFromPhysics = (type == RigidbodyType.Dynamic)` so users don't need to set it explicitly for the most common case.
4. **No wrapper properties on Ball** — Scenes use standard `GetComponent<RigidbodyComponent>()` instead of `ball.RigidbodyComponent` or `ball.Body`. Component is the API surface.

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Components/EntityComponent.cs` | New | ⭐ PUBLIC | Base component class |
| `Components/BuiltIn/RigidbodyComponent.cs` | New | ⭐ PUBLIC | Wraps `IPhysicsBody`, syncs transform ↔ entity, helper methods for impulse/velocity |
| `Components/BuiltIn/SpriteComponent.cs` | New | ⭐ PUBLIC | Decouples rendering (texture, scale, origin, color, effect, sort) |
| `Components/BuiltIn/ColliderComponent.cs` | New | ⭐ PUBLIC | Wraps `ICollider`, shape factories, event forwarding |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add component management + auto-iterate components in `Update()` |
| `Ball.cs` | Modified | 🔒 Playground | Demo of fully component-based entity — no manual sync or `.Body` access |
| `PhysicsEntityScene.cs` | Modified | 🔒 Playground | Uses `GetComponent<RigidbodyComponent>()` to apply impulse |
| `EntityComponentTests.cs` | New | 🔒 Internal | Unit tests for components (18 tests) |

---

## Notes & Risks

- **Medium risk** — component order matters for some use cases (e.g., ColliderComponent needs RigidbodyComponent first)
- Consider component priority for update ordering
- Memory overhead for component dictionaries
- **Hybrid rendering approach** — `Entity.Draw()` remains the default render path for backward compatibility. Components like `SpriteComponent` provide *additional* rendering options. Future sprint will enforce full component-based rendering.
- The 5 nullable warnings in `RigidbodyComponent` are false positives — `EnsureBody()` guarantees `_body` is non-null before access. Could suppress with `#pragma warning disable CS8602` or add null-forgiving operator.

---

*Created: 2026-08-07 | Updated: 2026-08-08 | Part of Entity System Enhancements Project*
