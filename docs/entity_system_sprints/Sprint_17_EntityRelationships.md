# Sprint 17 — Entity Relationships 🔗

**Points:** 4  
**Status:** Not Started  
**Sprint Goal:** Weak-reference relationships between entities (target, owner, follower, etc.).

---

## Tasks

- [ ] **T1: Create `EntityRelationship` class (1 pt)** ⭐ User-facing
  - Named relationship with entity reference
  - `SetRelationship(string name, Entity target)` method
  - `GetRelationship<T>(string name)` method
  - `RemoveRelationship(string name)` method

- [ ] **T2: Add relationship storage to `Entity` (1 pt)** ⭐ User-facing
  - `Dictionary<string, Entity>` for relationship storage
  - `OnRelationshipChanged` event
  - Auto-clean relationships when target is destroyed

- [ ] **T3: Add relationship events (1 pt)** ⭐ User-facing
  - Event when relationship is added/removed/changed
  - Callback with old and new entity references
  - Prevent circular relationships

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test relationship creation and retrieval
  - Test relationship removal
  - Test auto-clean on entity destroy
  - Test relationship change events

---

## Acceptance Criteria

- [ ] Entities can have named relationships
- [ ] Relationships are weak references (auto-clean on destroy)
- [ ] Relationship change events are fired
- [ ] Circular relationships are prevented
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new relationship tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `EntityRelationship.cs` | New | ⭐ PUBLIC | Relationship class |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add relationship storage |
| `EntityRelationshipTests.cs` | New | 🔒 Internal | Unit tests for relationships |

---

## Notes & Risks

- **Medium risk** — need to handle relationship cleanup properly
- Weak references to prevent memory leaks
- Consider relationship serialization for save/load

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
