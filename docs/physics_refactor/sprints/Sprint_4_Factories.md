# Sprint 4 — Factory Classes & Body Pooling 🔨

**Points:** 3  
**Status:** ✅ Completed
**Sprint Goal:** Create factory classes for object creation and implement body pooling to reduce GC pressure.

---

## Tasks

- [x] ~~**T1: Implement `PhysicsFactory.cs` (1 pt)**~~ ❌ **Cancelled — redundant with `PhysicsEngine`**
  - Users interact through `PhysicsEngine` as a `IFixedUpdateGameSystem`, which already wraps the world and exposes body creation methods. A separate factory was flawed because bodies always belong to a world, but the interface separated them (creating hidden default worlds). Removed from scope.

- [x] ~~**T2: Implement `SpatialShapeFactory.cs` (1 pt)**~~ ❌ **Cancelled — redundant with `IPhysicsBody`**
  - Shapes only exist as fixtures on bodies. Users create shapes via body methods (`CreateCircle`, `CreateRectangle`, etc.), never standalone. `IShape` is 🔒 internal use only. No use case for bare shape creation.

- [x] **T3: Implement Body Pooling (1 pt)** ✅
  - Added `_bodyPool` (Queue) to `PhysicsEngine` for recycling bodies
  - `CreateBody()` checks pool first — dequeues recycled body, resets position/type/fixtures/dynamics before reusing
  - `Destroy()` removes fixtures → resets state → enqueues back to pool instead of letting GC collect
  - Integrates seamlessly: users call `.Destroy()` and don't know pooling exists (automatic)

---

## Acceptance Criteria

- [x] Body pool correctly recycles bodies: create → destroy → reuse same instance
- [x] Pool integrates seamlessly with PhysicsEngine (user doesn't need to know pooling exists)
- [x] Project builds cleanly
  - All 172 tests pass, including 6 new body pooling-specific tests

---

## Deliverables

| File | Purpose |
|------|---------|
| `engines/aether/PhysicsBody.cs` | Add recycling methods (`OnRecycle`, `ResetForReuse`) |

---

## Notes & Risks

- The existing `WorldPool.cs` in CoreEssentials is the predecessor to this — we're migrating its functionality into the new project.
- Pooling should be opt-in or automatic; don't force users to manage pools manually.
- **Risk:** If pooled bodies retain stale state (old position, old fixtures), collisions could behave weirdly. Make sure `OnRecycle()` resets body state thoroughly.

---

*Created: 2026-07-16 | Part of Physics System Refactoring Project*
