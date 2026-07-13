# Sprint 4: Factory Classes & Helper Utilities

**Points:** 1  
**Status:** Not Started  
**Description:** Create factory classes that provide convenient creation methods while maintaining adapter pattern.

---

## Tasks

- [ ] **Create PhysicsEngine (User-Facing)** ⭐
  ```csharp
  // Users get this via GetGameSystem<PhysicsEngine>()
  // Wraps IPhysicsWorldAdapter internally, manages world automatically
  // Provides CreateDynamic(), CreateStatic() methods - returns IPhysicsBody
  // All world operations hidden from users
  ```

- [ ] **Create SpatialShapeFactory.cs** - Factory for creating shape instances (Internal only 🔒)
  ```csharp
  // Internal factory used by BodyAdapter to create shapes
  // Methods: CreateCircle, CreateRectangle, CreatePolygon
  // Returns ISpatialShape (never Aether types)
  ```

- [ ] **Create PhysicsEngine.cs** - Main entry point (User-Facing ⭐)
  ```csharp
  // Gets via GetGameSystem<PhysicsEngine>()
  // Wraps IPhysicsWorldAdapter internally, manages world automatically  
  // Provides CreateDynamic(), CreateStatic() methods - returns IPhysicsBody
  // All world operations hidden from users
  ```

- [ ] **Create SolverConfig.cs** (Internal only 🔒)
  ```csharp
  // Properties: VelocityIterations, PositionIterations, CCD enabled
  // SubSteppingFactor for advanced configuration
  ```
  Reference: `docs/PhysicsSystemRefactor.md`

- [ ] **Create Helper Extensions** (optional but recommended)
  ```csharp
  // Static helper methods for common operations
  // Extension methods on Vector2, Matrix for physics-specific operations
  ```
  
---

## Acceptance Criteria

- All factory classes return interface types only (no Aether leaks) in `CoreEssentials.Physics/` project structure
- SpatialShapeFactory properly delegates to shape adapter implementations
- BodyPoolAdapter maintains backward compatibility while adding new features
- Configuration class allows all necessary tuning options
- XML documentation with usage examples for each factory method

---

*Target Completion: Week of August 10, 2026*
