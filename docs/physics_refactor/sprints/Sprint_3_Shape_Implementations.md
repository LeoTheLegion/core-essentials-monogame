# Sprint 3 — Shape Implementations 📐

**Points:** 2  
**Status:** Not Started (depends on Sprint 2)  
**Sprint Goal:** Complete the shape abstractions and joint implementations so that `PhysicsBody.CreateCircle()`, `CreateRectangle()`, etc. work end-to-end.

---

## Tasks

- [x] **T1: Implement `CircleShape.cs` (0.5 pt)** 🔒
  - Wraps `Aether.Collision.Shapes.CircleShape`
  - Implements `IShape`: expose `Center`, `Radius`, `Vertices` (single point for circle)
  - `Translate()`, `Rotate()` → modify underlying Aether shape's offset/transform

- [x] **T2: Implement `RectangleShape.cs` (0.5 pt)** 🔒
  - Wraps `Aether.Collision.Shapes.PolygonShape` (constructed from size)
  - Implements `IShape`: expose computed vertices, bounding radius, center
  - Support vertex offset via constructor

- [x] **T3: Implement `PolygonShape.cs` (0.5 pt)** 🔒
  - Wraps `Aether.Collision.Shapes.PolygonShape` with explicit vertices
  - Implements `IShape`: expose provided vertices array, compute bounding radius and center
  - Support `CreateConvexHull()` — delegate to Aether's convex hull utility

- [x] **T4: Implement Joint interfaces (0.5 pt)** 🔒
  - `RevoluteJoint.cs` → wraps `Aether.Joints.RevoluteJoint`, implements `IRevoluteJoint`
  - `WeldJoint.cs` → wraps `Aether.Joints.WeldJoint`, implements `IWeldJoint`
  - `DistanceJoint.cs` → wraps `Aether.Joints.DistanceJoint`, implements `IDistanceJoint`

---

## Acceptance Criteria

- [x] All three shape classes implement `IShape` correctly
- [x] Creating a body then calling `.CreateCircle(radius)` produces a valid fixture with correct collision geometry
- [x] Joint implementations wrap corresponding Aether joints and expose clean properties
- [x] Project builds cleanly

---

## Deliverables

| File | Implements | Wraps |
|------|-----------|-------|
| `engines/aether/Shapes/CircleShape.cs` | `IShape` | `Aether.CircleShape` |
| `engines/aether/Shapes/RectangleShape.cs` | `IShape` | `Aether.PolygonShape` (from size) |
| `engines/aether/Shapes/PolygonShape.cs` | `IShape` | `Aether.PolygonShape` (vertices) |
| `engines/aether/Joints/RevoluteJoint.cs` | `IRevoluteJoint` | `Aether.RevoluteJoint` |
| `engines/aether/Joints/WeldJoint.cs` | `IWeldJoint` | `Aether.WeldJoint` |
| `engines/aether/Joints/DistanceJoint.cs` | `IDistanceJoint` | `Aether.DistanceJoint` |

---

## Notes & Risks

- Aether's Rectangle doesn't exist as a separate type — it's a helper that creates a PolygonShape. Our `RectangleShape` should abstract this away cleanly.
- Convex hull creation uses `PolygonTools.CreateConvexHull()` from Farseer/Aether — make sure to handle degenerate inputs (collinear points, < 3 unique points).

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
