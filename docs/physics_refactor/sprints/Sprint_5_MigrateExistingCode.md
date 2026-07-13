# Sprint 5: Migrate Existing Physics Code to Adapters

**Points:** 1  
**Status:** Not Started  
**Description:** Update existing physics classes (PhysicsEngine, WorldPool, DebugRenderer) to use new adapter pattern internally.

---

## Tasks

- [ ] **Update CoreEssentials.Physics/PhysicsEngine.cs** - Migrate from direct Aether usage to adapters
  ```csharp
  // Remove: Direct exposure of _world as Aether.World
  // Add: Internal IPhysicsWorldAdapter implementation
  // Update CreateCircle() to return IPhysicsBody instead of Body
  // All methods now work through adapter interfaces internally
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 4

- [ ] **Update CoreEssentials.Physics/WorldPool.cs** or use BodyPoolAdapter from new project
  ```csharp
  // Option A: Wrap existing code with adapters (backward compatible)
  // Option B: Replace entirely with new BodyPoolAdapter implementation
  // Update to return IPhysicsBody instead of Aether.Body
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 4

- [ ] **Update CoreEssentials.Physics/PhysicsDebugRenderer.cs** - Accept shape interfaces instead of Aether types
  ```csharp
  // Change: DrawShape(ISpatialShape) instead of DrawShape(Aether.Shape)
  // Update fixture rendering to use adapter's Shape property
  // Maintain debug drawing capabilities with new interface
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 4

- [ ] **Verify No Aether Leaks** - Check all modified files for exposed Aether types
  ```csharp
  // Search for any remaining direct references to Aether classes
  // Ensure public API only exposes interfaces
  // Update XML documentation with migration notes if needed
  ```

---

## Acceptance Criteria

- PhysicsEngine in CoreEssentials.Physics returns adapter interfaces in all public methods
- WorldPool updated or replaced with adapter-based implementation
- PhysicsDebugRenderer accepts ISpatialShape interface
- Zero Aether types exposed through public API
- All existing unit tests still pass after migration
- Migration notes added to XML documentation where API changed

---

*Target Completion: Week of August 17, 2026*
