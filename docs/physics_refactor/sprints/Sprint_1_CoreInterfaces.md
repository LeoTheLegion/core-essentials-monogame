# Sprint 1: Core Interface Definitions

**Points:** 1  
**Status:** Not Started  
**Description:** Create all adapter interface definitions with proper XML documentation. No Aether types exposed.

---

## Tasks

- [ ] **Create IPhysicsBodyAdapter.cs** - Body interface with shape creation methods
  ```csharp
  // Methods: CreateCircle, CreateRectangle, CreatePolygon, AddFixture, RemoveFixture
  // Properties: Position, Rotation, Type, Mass, Friction, Restitution, FixedRotation
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 1

- [ ] **Create IFixtureAdapter.cs** - Fixture lifecycle management interface
  ```csharp
  // Methods: Activate, Deactivate, DisableSleep, EnableSleep
  // Properties: Shape, IsActive, OwnerBody
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 1

- [ ] **Create ISpatialShapeAdapter.cs** - Unified shape interface with ShapeType enum
  ```csharp
  // Methods: Translate, Rotate, PointContains, GetVertices
  // Properties: Center, Radius
  // Enum: Circle, Rectangle, Polygon, ConvexHull, LineSegment, Unknown
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 1

- [ ] **Create IPhysicsWorldAdapter.cs** - World simulation management interface
  ```csharp
  // Methods: AddBody, RemoveBody, Step, ClearAllBodies
  // Properties: Gravity
  // Class: SolverConfig (VelocityIterations, PositionIterations, CCD)
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 2

- [ ] **Create IConstraintAdapter.cs** - Joint/constraint interfaces
  ```csharp
  // Interfaces: IConstraint, IPivotJoint, IFixedJoint, IDistanceJoint
  // Properties: BodyA, BodyB, IsActive, LocalAnchorA/B for joints
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Priority 2

---

## Acceptance Criteria

- All 5 interface files created in `CoreEssentials/src/gameSystems/physics/adapters/` folder
- Zero Aether type references in any file
- XML documentation on all public methods and properties
- ShapeType enum defined with all required values
- SolverConfig class properly structured as nested or separate class

---

*Target Completion: Week of July 20, 2026*
