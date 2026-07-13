# Sprint 2: Adapter Implementations - Core Classes

**Points:** 1  
**Status:** Not Started  
**Description:** Implement the core adapter classes that wrap Aether internally while exposing clean interfaces.

---

## Tasks

- [ ] **Implement PhysicsBodyAdapter.cs** - Wraps Aether.Body, implements IPhysicsBody (user-facing)
  ```csharp
  // Internally manages: _body (Aether.Body), _fixtures list
  // Exposes: CreateCircle, AddFixture, movement methods via interface
  // Handles fixture lifecycle internally (no manual Add/Remove needed)
  // This is the ONLY adapter users will ever see interact with directly
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Implement FixtureAdapter.cs** - Wraps Aether.Fixture, implements IFixture (internal only)
  ```csharp
  // Internally manages: _fixture (Aether.Fixture), _shape reference
  // Exposes: Activate, Deactivate via interface methods
  // Disposal properly cleans up Aether resources
  // Used ONLY by PhysicsBodyAdapter internally
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Implement ISpatialShapeAdapters** - Circle, Rectangle, Polygon (internal only)
  ```csharp
  // These wrap Aether shapes and are used internally by BodyAdapter
  // Users never see or interact with these directly
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Implement PhysicsWorldAdapter.cs** - Wraps Aether.World (internal only)
  ```csharp
  // Internally manages: _world (Aether.World), bodies collection
  // Implements IFixedUpdateGameSystem for automatic updates
  // Exposes Step(), AddBody() via interface methods
  // Used ONLY by PhysicsEngine internally - users NEVER access this directly
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Create Constraint Adapter Implementations** (internal only)
  - IPivotJointAdapter.cs (wraps Aether.RevoluteJoint)
  - IFixedJointAdapter.cs (wraps Aether.WeldJoint)  
  - IDistanceJointAdapter.cs (wraps Aether.DistanceJoint)
  ```csharp
  // Used ONLY by PhysicsFactory internally when creating joints
  // Users work with constraints through Body methods, not directly
  ```

---

## Acceptance Criteria

- All adapter classes implement their respective interfaces in `CoreEssentials.Physics/adapters/implementations/`
- WorldAdapter and constraint adapters are marked as [Internal] or [Obsolete] for external use
- PhysicsBodyAdapter is the ONLY public-facing body interface users interact with
- No Aether types in public method signatures or properties
- Proper disposal pattern implemented for all adapters
- IFixedUpdateGameSystem inherited by PhysicsWorldAdapter
- Unit tests added for each adapter (covered in next sprint)

---

*Target Completion: Week of July 27, 2026*
