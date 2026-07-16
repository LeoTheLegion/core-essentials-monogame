# Sprint 2 — Engine Implementations: Body & Fixture ⚙️

**Points:** 5  
**Status:** Not Started (depends on Sprint 1)  
**Sprint Goal:** Implement `PhysicsBody`, `Fixture`, and `PhysicsEngine` GameSystem — the core of the physics system. These wrap Aether types but expose only our clean interfaces.

---

## Tasks

- [ ] **T1: Implement `PhysicsBody.cs` (2 pts)** ⭐
  - Wraps a single `Aether.Physics2D.Dynamics.Body` instance
  - Implements all properties/methods from `IPhysicsBody`:
    - Position/rotation: delegate to Aether Body's `Position`, `Angle`
    - Type management: wrap `BodyType` enum (Static/Dynamic/Kinematic)
    - Shape creation: `CreateCircle()` → creates CircleShape, calls internal fixture creation; same for Rectangle/Polygon/ConvexHull
    - Material properties: map to Aether Body's `Mass`, `Friction`, `Restitution`, etc.
    - Forces/impulses: delegate to `Body.ApplyForce()`, `Body.ApplyLinearImpulse()`
    - Velocity control: wrap `Body.LinearVelocity`, `Body.AngularVelocity`
  - Implement `Dispose()` — signal world to remove body

- [ ] **T2: Implement `Fixture.cs` (1 pt)** 🔒
  - Wraps `Aether.Physics2D.Dynamics.Fixture`
  - Implements `IFixture`: expose `Shape`, `IsActive`, `OwnerBody`
  - `Activate()`/`Deactivate()` → delegate to Aether Fixture

- [ ] **T3: Implement `PhysicsEngine.cs` (2 pts)** ⭐ GameSystem
  - Inherit from `CoreEssentials.GameSystems.GameSystem` + implement `IFixedUpdateGameSystem`
  - Internally manages an `Aether.World` instance (users never see it)
  - Public methods:
    - `CreateDynamic(Vector2 position)` → creates PhysicsBody(Dynamic), adds to world, returns IPhysicsBody ⭐
    - `CreateStatic(Vector2 position)` → same for Static bodies
    - `CreateKinematic(Vector2 position)` → same for Kinematic bodies
    - `Destroy(IPhysicsBody body)` → removes from world, disposes
    - `CreateRevoluteJoint(bodyA, bodyB, anchor)` → creates IRevoluteJoint
    - `Gravity { get; set; }` → proxy to internal world's gravity
  - `IFixedUpdateGameSystem.Update(fixedDt)`: calls `_world.Step(fixedDt, ...)` with solver config

- [ ] **T4: Verify build + basic sanity (0.5 pt)**
  - Project builds cleanly
  - Manual test in Playground: create a dynamic body via new API, verify it appears in world

---

## Acceptance Criteria

- [ ] `PhysicsBody` implements all `IPhysicsBody` members correctly
- [ ] `Fixture` wraps Aether.Fixture and implements `IFixture`
- [ ] `PhysicsEngine` is a proper GameSystem with `CreateDynamic/CreateStatic/CreateKinematic/Destroy` methods
- [ ] `IFixedUpdateGameSystem.Update()` steps the internal world each fixed tick
- [ ] Users can call `GetGameSystem<PhysicsEngine>()` → `.CreateDynamic(pos)` and get back an `IPhysicsBody`
- [ ] Project builds cleanly

---

## Deliverables

| File | Implements | Notes |
|------|-----------|-------|
| `engines/aether/PhysicsEngine.cs` | `GameSystem`, `IFixedUpdateGameSystem` | ⭐ Main entry point for users |
| `engines/aether/PhysicsBody.cs` | `IPhysicsBody` | ⭐ Wraps Aether.Body |
| `engines/aether/Fixture.cs` | `IFixture` | 🔒 Internal only |

---

## Notes & Risks

- **Biggest risk:** Shape creation methods on PhysicsBody (`CreateCircle`, etc.) need the shape implementations from Sprint 3. Stub them out with `throw new NotImplementedException()` for now, or implement a basic CircleShape in this sprint if time allows.
- The existing `CoreEssentials/src/gameSystems/physics/PhysicsEngine.cs` is a DIFFERENT file — don't overwrite it yet (that's Sprint 5). This new one lives in `CoreEssentials.Physics/engines/aether/`.

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
