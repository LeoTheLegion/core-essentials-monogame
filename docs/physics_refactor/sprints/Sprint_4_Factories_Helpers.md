# Sprint 4: Factory Classes & Helper Utilities

**Points:** 1  
**Status:** Not Started  
**Description:** Create factory classes that provide convenient creation methods while maintaining adapter pattern.

---

## Tasks

- [ ] **Create PhysicsFactory.cs** - Main factory for creating worlds and bodies
  ```csharp
  // Methods: CreateDefault(), CreateWithGravity()
  // Body factories: CreateStatic, CreateDynamic, CreateKinematic
  // Returns IPhysicsWorld and IPhysicsBody interfaces (never Aether types)
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 3

- [ ] **Create SpatialShapeFactory.cs** - Factory for creating shape instances
  ```csharp
  // Property: Shapes { get; } returns ISpatialShapeFactory interface
  // Methods: CreateCircle, CreateRectangle, CreatePolygon, CreateConvexHull
  // Each method returns ISpatialShape (never Aether types)
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 3

- [ ] **Create BodyPoolAdapter.cs** - Enhanced wrapper around existing WorldPool
  ```csharp
  // Wraps CoreEssentials/src/gameSystems/physics/WorldPool.cs
  // Adds adapter pattern support for body pooling
  // Maintains backward compatibility with existing code
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 3

- [ ] **Create PhysicsConfig.cs** (if not in IPhysicsWorldAdapter) - Configuration class
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
