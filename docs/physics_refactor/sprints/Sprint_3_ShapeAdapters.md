# Sprint 3: Spatial Shape Adapters

**Points:** 1  
**Status:** Not Started  
**Description:** Implement unified shape adapters that provide consistent API across all shape types.

---

## Tasks

- [ ] **Create CircleShapeAdapter.cs** - Wraps Aether.CircleShape, implements ISpatialShape
  ```csharp
  // Properties: Center (Aether's localCenter), Radius
  // Methods: Translate, Rotate, PointContains
  // Returns bounding radius for unified API
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - ShapeAdapters

- [ ] **Create RectangleShapeAdapter.cs** - Wraps Aether.PolygonShape (4 vertices)
  ```csharp
  // Properties: Center, Size or Vertices
  // Methods: Translate, Rotate, PointContains
  // GetVertices returns array of 4 Vector2 points
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - ShapeAdapters

- [ ] **Create PolygonShapeAdapter.cs** - Wraps Aether.PolygonShape (variable vertices)
  ```csharp
  // Properties: Center, Vertices count
  // Methods: Translate, Rotate, PointContains
  // GetVertices returns dynamic array of Vector2 points
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - ShapeAdapters

- [ ] **Create ConvexHullShapeAdapter.cs** (bonus) - Wraps Aether.ConvexHullPolygonShape
  ```csharp
  // Builds convex hull from input points
  // Exposes same ISpatialShape interface as other shapes
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

- [ ] **Create ConvexHullShapeAdapter.cs** (bonus) - Wraps Aether.ConvexHullPolygonShape
  ```csharp
  // Builds convex hull from input points
  // Exposes same ISpatialShape interface as other shapes
  ```
  Reference: `docs/PhysicsSystemRefactor.md` - Phase 2

---

## Acceptance Criteria

- All shape adapters implement ISpatialShape interface consistently in `CoreEssentials.Physics/adapters/implementations/ShapeAdapters/`
- Translate/Rotate work identically across all shape types
- PointContains method available on all shapes
- GetVertices returns appropriate vertex array for each type
- XML documentation with examples for each adapter class

---

*Target Completion: Week of August 3, 2026*
