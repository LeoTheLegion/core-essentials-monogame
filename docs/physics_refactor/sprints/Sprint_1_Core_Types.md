# Sprint 1 — Core Type Definitions 🔧

**Points:** 3  
**Status:** Not Started (depends on Sprint 0)  
**Sprint Goal:** Define all pure interface types in `types/` folder with zero references to Aether or any external physics engine.

---

## Tasks

- [ ] **T1: Create `IPhysicsBody.cs` (2 pts)** ⭐ User-facing
  - Position and rotation: `WorldPosition { get; }`, `Rotation { get; set; }`
  - Type management: `Type { get; set; }`, `IsStatic`, `IsDynamic`, `IsKinematic`
  - Shape creation: `CreateCircle()`, `CreateRectangle()`, `CreatePolygon()`, `CreateConvexHull()`
  - Fixture management: `AddFixture()`, `RemoveFixture()` — returns/accepts `IFixture`/`IShape`
  - Material properties: `Mass`, `Inertia`, `Friction`, `Restitution`, `FixedRotation`
  - Movement and forces: `ApplyForce()`, `ApplyTorque()`, `ApplyImpulse()`
  - Velocity control: `LinearVelocity { get; }`, `AngularVelocity { get; }`, setters, `StopAll()`
  - Body state: `IsAwake { get; }`, `IsActive { get; }`

- [ ] **T2: Create `IFixture.cs` (0.5 pt)** 🔒 Internal only
  - Properties: `Shape { get; }`, `IsActive { get; }`, `OwnerBody { get; }`
  - Lifecycle: `Activate()`, `Deactivate()`
  - Mark interface with `[Obsolete("Internal use only")]`

- [ ] **T3: Create `IShape.cs` (0.5 pt)** 🔒 Internal only
  - Properties: `Center { get; }`, `Radius { get; }`, `Vertices { get; }`
  - Transform operations: `Translate()`, `Rotate()`
  - Query methods: `PointContains()`
  - Type identification: `GetType() → ShapeType enum`
  - Define `ShapeType` enum (Circle, Rectangle, Polygon, ConvexHull, LineSegment, Unknown)

- [ ] **T4: Create `IPhysicsWorld.cs` + `SolverConfig.cs` (0.5 pt)** 🔒 Internal only
  - Properties: `Gravity { get; set; }`
  - Body management: `AddBody()`, `RemoveBody()`, `ClearAllBodies()`
  - Simulation: `Step(deltaTime, solverOptions)`
  - `SolverConfig`: `VelocityIterations`, `PositionIterations`, `ContinuousCollisionDetection`

- [ ] **T5: Create `IConstraint.cs` + Joint interfaces (0.5 pt)** 🔒 Internal only
  - Base `IConstraint`: `BodyA { get; }`, `BodyB { get; }`, `IsActive { get; }`, `Apply()`, `Remove()`
  - `IRevoluteJoint : IConstraint`: `LocalAnchorA/B`, `LimitAngle` (hinge joint)
  - `IWeldJoint : IConstraint`: `CollideConnected` (fixed/weld joint)
  - `IDistanceJoint : IConstraint`: `Length`, `MaxForce`

- [ ] **T6: Create `IPhysicsFactory.cs` + `PhysicsConfig.cs` (0.5 pt)** 🔒 Internal only
  - Factory methods: `CreateDefault()`, `CreateWithGravity()`, `CreateWithConfig()`
  - Body creation: `CreateStatic()`, `CreateDynamic()`, `CreateKinematic()`
  - Shape factory accessor: `Shapes { get; }`
  - `PhysicsConfig`: Solver iterations, CCD flag, sub-stepping factor

---

## Acceptance Criteria

- [x] All interface files exist in `types/` folder
- [x] **ZERO references to Aether types** — no `using nkast.Aether.*`, no Aether type names anywhere in `types/`
- [x] Project builds cleanly (`dotnet build CoreEssentials.Physics`) — 0 errors, only NuGet warnings
- [x] Internal-only interfaces marked as `public` (see Notes & Risks)

---

## Deliverables

| File | Interface | Visibility |
|------|-----------|------------|
| `types/IPhysicsBody.cs` | `IPhysicsBody : IDisposable` | ⭐ PUBLIC — users interact directly |
| `types/IFixture.cs` | `IFixture : IDisposable` | public (documented internal use) |
| `types/IShape.cs` | `IShape : IDisposable`, `ShapeType enum` | public (documented internal use) |
| `types/IPhysicsWorld.cs` | `IPhysicsWorld : IDisposable`, `SolverConfig class` | public (documented internal use) |
| `types/IConstraint.cs` | `IConstraint`, `IRevoluteJoint`, `IWeldJoint`, `IDistanceJoint` | public (documented internal use) |
| `types/IPhysicsFactory.cs` | `IPhysicsFactory : IDisposable`, `ISpatialShapeFactory : IDisposable` | public (documented internal use) |
| `types/PhysicsConfig.cs` | `PhysicsConfig class` | public (documented internal use) |

---

## Notes & Risks

- **Critical:** These interfaces MUST NOT reference Aether. They are the *contract* that allows engine swapping later.
- `IPhysicsBody` is the ONLY interface exposed to users — everything else should be marked `[Obsolete("Internal use only")]` or declared as `internal`.
- Verify all types use `Microsoft.Xna.Framework.Vector2`, NOT Aether's Vector2.

## Lessons Learned (Post-Sprint)

- **Visibility decision:** We initially tried making internal-only interfaces `internal`, but that broke the public API contract of `IPhysicsBody` which exposes them as return/parameter types. In C#, you cannot expose an `internal` type in a `public` interface signature. So all types were reverted to `public` with XML docs indicating they're for internal use only.
- **Stub implementation visibility:** Interface implementation members must be explicitly marked `public` in stub classes, otherwise the compiler reports "'X' does not implement interface member 'Y' because it is not public".

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
