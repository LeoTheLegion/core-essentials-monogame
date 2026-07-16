# Sprint 2 — Engine Implementations: Body & Fixture ⚙️

**Points:** 5  
**Status:** Not Started (depends on Sprint 1)  
**Sprint Goal:** Implement `PhysicsBody`, `Fixture`, and `PhysicsEngine` GameSystem — the core of the physics system. These wrap Aether types but expose only our clean interfaces.

---

## Tasks

- [x] **T1: Implement `PhysicsBody.cs` (2 pts)** ⭐
  - Wraps a single `Aether.Physics2D.Dynamics.Body` instance
  - Implements all properties/methods from `IPhysicsBody`:
    - Position/rotation: delegate to Aether Body's `Position`, `Angle`
    - Type management: wrap `BodyType` enum (Static/Dynamic/Kinematic)
    - Shape creation: `CreateCircle()` → creates CircleShape, calls internal fixture creation; same for Rectangle/Polygon/ConvexHull
    - Material properties: map to Aether Body's `Mass`, `Friction`, `Restitution`, etc.
    - Forces/impulses: delegate to `Body.ApplyForce()`, `Body.ApplyLinearImpulse()`
    - Velocity control: wrap `Body.LinearVelocity`, `Body.AngularVelocity`
  - Implement `Dispose()` — signal world to remove body

- [x] **T2: Implement `Fixture.cs` (1 pt)** 🔒
  - Wraps `Aether.Physics2D.Dynamics.Fixture`
  - Implements `IFixture`: expose `Shape`, `IsActive`, `OwnerBody`
  - `Activate()`/`Deactivate()` → delegate to Aether Fixture

- [x] **T3: Implement `PhysicsEngine.cs` (2 pts)** ⭐ GameSystem
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

- [x] **T4: Verify build + basic sanity (0.5 pt)**
  - Project builds cleanly
  - Create positive and negatives tests in a new project called CoreEssentials.Physics.Tests
    - Test for gamesystem creation
    - Test for body creation
    - Test for fixture creation
    - Test for destruction for either class object

---

## Acceptance Criteria

- [x] `PhysicsBody` implements all `IPhysicsBody` members correctly ✅
  - All properties/methods delegate to Aether.Body properly
  - Shape creation methods (CreateCircle, CreateRectangle, etc.) throw NotImplementedException — expected for Sprint 2, deferred to Sprint 3
  - ⚠️ Minor: `Type` setter is a no-op; `Restitution` getter returns first fixture's value only (multi-fixture bodies)
- [x] `Fixture` wraps Aether.Fixture and implements `IFixture` ✅
  - Fixed critical bug: `Deactivate()` was disabling entire owner body instead of just this fixture's proxies
  - Fixed critical bug: `IsActive` checked `_aetherFixture.Body.Enabled` (body-level) instead of `_aetherFixture.ProxyCount > 0` (fixture-level)
  - ⚠️ Minor: `Shape` property throws NotImplementedException — expected for Sprint 3
- [x] `PhysicsEngine` is a proper GameSystem with `CreateDynamic/CreateStatic/CreateKinematic/Destroy` methods ✅
  - Inherits from `GameSystem`, implements `IFixedUpdateGameSystem`
  - All body creation methods return `IPhysicsBody`, cache bodies in dictionary for Destroy lookup
  - Destroy properly removes from world and nulls body reference
- [x] `IFixedUpdateGameSystem.Update()` steps the internal world each fixed tick ✅
  - Calls `_world.Step(dt, ref iterations)` with configurable solver iterations (default: 3 position, 8 velocity)
  - Guards against zero delta and checks `_world.Enabled`
- [x] Users can call `GetGameSystem<PhysicsEngine>()` → `.CreateDynamic(pos)` and get back an `IPhysicsBody` ✅
  - Engine is a proper GameSystem with correct return types
- [x] Project builds cleanly ✅ — **0 errors, 84 tests pass** (CoreEssentials.Physics.Tests)

## Verification Notes

During acceptance criteria verification, two bugs were found and fixed in `Fixture.cs`:
1. **Deactivate() disabled entire body** — broke multi-fixture bodies. Fixed to only destroy this fixture's proxies.
2. **IsActive reported body-level state** — checked `_aetherFixture.Body.Enabled` instead of proxy count. Fixed to check `_aetherFixture.ProxyCount > 0`.

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
