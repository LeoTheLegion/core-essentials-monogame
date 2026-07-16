# Sprint 4 — Factory Classes & Body Pooling 🔨

**Points:** 3  
**Status:** Not Started (depends on Sprint 3)  
**Sprint Goal:** Create factory classes for object creation and implement body pooling to reduce GC pressure.

---

## Tasks

- [ ] **T1: Implement `PhysicsFactory.cs` (1 pt)** 🔒
  - `CreateDefault()` → creates default world with standard gravity, returns `IPhysicsWorld`
  - `CreateWithGravity(Vector2)` → custom gravity configuration
  - `CreateWithConfig(PhysicsConfig)` → full solver config override
  - `CreateStatic/Dynamic/Kinematic(position, rotation)` → create bodies via interfaces (delegates to world)

- [ ] **T2: Implement `SpatialShapeFactory.cs` (1 pt)** 🔒
  - `CreateCircle(radius)` → returns `IShape` (CircleShape instance)
  - `CreateRectangle(size)` → returns `IShape` (RectangleShape instance)
  - `CreatePolygon(vertices)` → returns `IShape` (PolygonShape instance)
  - `CreateConvexHull(points)` → computes hull, returns `IShape`

- [ ] **T3: Implement Body Pooling (1 pt)** 🔒
  - Create a pool class that recycles bodies instead of letting GC collect them (reduces allocation pressure)
  - Pattern similar to existing `WorldPool.cs`: maintain a queue of inactive bodies, return them on create, enqueue on destroy
  - Integrate with `PhysicsEngine` so pooling is automatic

---

## Acceptance Criteria

- [ ] Factory classes can create all body types and shape types through interfaces only (no Aether exposed)
- [ ] Body pool correctly recycles bodies: create → destroy → reuse same instance
- [ ] Pool integrates seamlessly with PhysicsEngine (user doesn't need to know pooling exists)
- [ ] Project builds cleanly

---

## Deliverables

| File | Purpose |
|------|---------|
| `factory/PhysicsFactory.cs` | Creates worlds and bodies via interfaces |
| `factory/SpatialShapeFactory.cs` | Creates shapes via interfaces |
| `factory/BodyPool.cs` | Object pooling for PhysicsBody instances (wraps old WorldPool concept) |

---

## Notes & Risks

- The existing `WorldPool.cs` in CoreEssentials is the predecessor to this — we're migrating its functionality into the new project.
- Pooling should be opt-in or automatic; don't force users to manage pools manually.
- **Risk:** If pooled bodies retain stale state (old position, old fixtures), collisions could behave weirdly. Make sure `OnRecycle()` resets body state thoroughly.

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
