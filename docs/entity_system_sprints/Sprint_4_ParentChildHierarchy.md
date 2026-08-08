# Sprint 4 — Parent-Child Hierarchy 🌳

**Points:** 5  
**Status:** Not Started  
**Sprint Goal:** Transform inheritance so child entities follow their parent.

---

## Tasks

- [x] **T1: Add parent-child storage to `Entity` (1 pt)** 🔒 Internal
  - `Entity Parent` property
  - `List<Entity> Children` collection
  - `Vector2 LocalPosition` for child offset
  - `AddChild(Entity child)` and `RemoveChild(Entity child)`

- [x] **T2: Implement transform inheritance (2 pts)** ⭐ User-facing
  - Child `Position` returns `Parent.Position + LocalPosition`
  - Child `Rotation` returns `Parent.Rotation + LocalRotation`
  - Recursive transform calculation for nested hierarchies
  - Invalidate transform cache on parent change

- [x] **T3: Add hierarchy lifecycle management (1 pt)** ⭐ User-facing
  - Auto-remove children when parent is destroyed
  - Children follow parent when parent is deactivated
  - Prevent circular parent references

- [x] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test child position follows parent
  - Test nested hierarchy (child of child)
  - Test child removal on parent destroy
  - Test circular reference prevention

---

## Acceptance Criteria

- [ ] Child entities inherit parent transform
- [ ] Transform inheritance works for nested hierarchies
- [ ] Children are cleaned up when parent is destroyed
- [ ] Circular parent references are prevented
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new hierarchy tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Hierarchy/EntityParent.cs` | New | ⭐ PUBLIC | Parent-child relationship |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add parent/child properties |
| `EntityHierarchyTests.cs` | New | 🔒 Internal | Unit tests for hierarchy |

---

## Notes & Risks

- **Medium risk** — transform calculation can get expensive for deep hierarchies
- Consider caching world transforms to avoid recalculation
- Need to handle entity pooling with hierarchy (detach on release)

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
