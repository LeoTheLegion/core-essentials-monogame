# Sprint 19 — Collision Groups 💥

**Points:** 5.5  
**Status:** Not Started  
**Sprint Goal:** Assign entities to collision groups for filtered interaction.

**Dependencies:** Sprint 13 (GameStateSerialization)

**Existing Physics API (from Sprint 13):**
- `RigidbodyComponent` manages `IPhysicsBody` with component-level properties: `Position`, `Rotation`, `LinearVelocity`, `AngularVelocity`
- `ColliderComponent` manages colliders attached to physics bodies
- Both components are serialized/deserialized via entity-driven approach
- Physics engine accessed via `EntitySystem.GetGameSystem<PhysicsEngine>()`

---

## Tasks

- [ ] **T1: Create `CollisionGroup` class (1 pt)** ⭐ User-facing
  - Group name and entity list
  - `CreateCollisionGroup(string name)` method
  - `AddToCollisionGroup(string groupName)` method
  - `RemoveFromCollisionGroup(string groupName)` method

- [ ] **T2: Create `CollisionMatrix` class (2 pts)** ⭐ User-facing
  - Define which groups collide with each other
  - `SetCollisionEnabled(string groupA, string groupB, bool enabled)` method
  - `IsCollisionEnabled(string groupA, string groupB)` method
  - Default all groups collide

- [ ] **T3: Add collision query to `EntitySystem` (1 pt)** ⭐ User-facing
  - `GetCollidingEntities(string groupA, string groupB)` method
  - Filter by collision matrix
  - Return colliding entity pairs

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test group creation and assignment
  - Test collision matrix
  - Test collision queries
  - Test collision filtering

- [ ] **T5: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/CollisionGroups.md` user guide
  - Document collision groups
  - Document collision matrix
  - Provide filtering examples

---

## Acceptance Criteria

- [ ] Collision groups can be created
- [ ] Entities can be assigned to groups
- [ ] Collision matrix controls which groups collide
- [ ] Collision queries respect matrix
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new collision tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Collision/CollisionGroup.cs` | New | ⭐ PUBLIC | Collision group definition |
| `Collision/CollisionMatrix.cs` | New | ⭐ PUBLIC | Collision filtering |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add collision query methods |
| `CollisionGroupTests.cs` | New | 🔒 Internal | Unit tests for collision groups |
| `docs/CollisionGroups.md` | New | ⭐ PUBLIC | User guide for collision groups |

---

## Notes & Risks

- **Medium risk** — need to integrate with existing collision detection
- Performance consideration for large numbers of entities
- Consider layer-based collision optimization

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
