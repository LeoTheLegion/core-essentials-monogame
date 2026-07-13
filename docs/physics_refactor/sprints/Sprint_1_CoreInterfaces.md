# Sprint 1: Core Interface Definitions

**Points:** 1  
**Status:** Not Started  
**Description:** Create all adapter interface definitions with proper XML documentation. No Aether types exposed.

---

## Tasks

- [ ] **Create IPhysicsBodyAdapter.cs** - Body interface with shape creation methods (user-facing)
  ```csharp
  // Methods: CreateCircle, CreateRectangle, CreatePolygon, AddFixture, RemoveFixture
  // Properties: Position, Rotation, Type, Mass, Friction, Restitution, FixedRotation
  // This is the ONLY physics object interface users interact with directly
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 1

- [ ] **Create IFixtureAdapter.cs** - Fixture lifecycle management interface (internal use)
  ```csharp
  // Methods: Activate, Deactivate, DisableSleep, EnableSleep
  // Properties: Shape, IsActive, OwnerBody
  // Used internally by PhysicsBodyAdapter, NOT exposed to users
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 1

- [ ] **Create ISpatialShapeAdapter.cs** - Unified shape interface with ShapeType enum (internal use)
  ```csharp
  // Methods: Translate, Rotate, PointContains, GetVertices
  // Properties: Center, Radius
  // Enum: Circle, Rectangle, Polygon, ConvexHull, LineSegment, Unknown
  // Used internally by BodyAdapter and Factory, NOT exposed to users
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 1

- [ ] **Create IConstraintAdapter.cs** - Joint/constraint interfaces (internal use)
  ```csharp
  // Interfaces: IConstraint, IPivotJoint, IFixedJoint, IDistanceJoint
  // Properties: BodyA, BodyB, IsActive, LocalAnchorA/B for joints
  // Used internally by PhysicsFactory, NOT exposed to users
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 2

**Note:** World simulation (IPhysicsWorldAdapter) is intentionally **NOT user-facing**. Users access physics through:
1. `GetGameSystem<PhysicsEngine>()` - PhysicsEngine wraps the world internally
2. Factory methods on PhysicsEngine to create bodies directly
3. All world operations hidden behind clean body creation APIs

---

## Acceptance Criteria

- 4 interface files created in `CoreEssentials.Physics/adapters/interfaces/` folder (excluding IPhysicsWorldAdapter)
- Zero Aether type references in any file
- XML documentation on all public methods and properties
- ShapeType enum defined with all required values
- SolverConfig class properly structured as nested or separate class

---

*Target Completion: Week of July 20, 2026*
