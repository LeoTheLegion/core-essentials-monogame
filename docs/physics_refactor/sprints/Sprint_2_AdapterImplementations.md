# Sprint 2: Adapter Implementations - Core Classes

**Points:** 1  
**Status:** Not Started  
**Description:** Implement the core adapter classes that wrap Aether internally while exposing clean interfaces.

---

## Tasks

- [ ] **Implement PhysicsBodyAdapter.cs** - Wraps Aether.Body, implements IPhysicsBody
  ```csharp
  // Internally manages: _body (Aether.Body), _fixtures list
  // Exposes: CreateCircle, AddFixture, movement methods via interface
  // Handles fixture lifecycle internally (no manual Add/Remove needed)
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Implement FixtureAdapter.cs** - Wraps Aether.Fixture, implements IFixture
  ```csharp
  // Internally manages: _fixture (Aether.Fixture), _shape reference
  // Exposes: Activate, Deactivate via interface methods
  // Disposal properly cleans up Aether resources
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Implement PhysicsWorldAdapter.cs** - Wraps Aether.World, implements IPhysicsWorld
  ```csharp
  // Internally manages: _world (Aether.World), bodies collection
  // Implements IFixedUpdateGameSystem for automatic updates
  // Exposes Step(), AddBody() via interface methods
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Create Constraint Adapter Implementations**
  - IPivotJointAdapter.cs (wraps Aether.RevoluteJoint)
  - IFixedJointAdapter.cs (wraps Aether.WeldJoint)
  - IDistanceJointAdapter.cs (wraps Aether.DistanceJoint)
  
---

## Acceptance Criteria

- All adapter classes implement their respective interfaces
- No Aether types in public method signatures or properties
- Proper disposal pattern implemented for all adapters
- IFixedUpdateGameSystem inherited by PhysicsWorldAdapter
- Unit tests added for each adapter (covered in next sprint)

---

*Target Completion: Week of July 27, 2026*
