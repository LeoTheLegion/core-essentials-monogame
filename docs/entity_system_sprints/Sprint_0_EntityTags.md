# Sprint 0 — Entity Tags 🏷️

**Points:** 3  
**Status:** Not Started  
**Sprint Goal:** Add string-based tagging to entities for easy grouping and lookup without type-checking.

---

## Tasks

- [ ] **T1: Add tag storage to `Entity.cs` (1 pt)** ⭐ User-facing
  - Add `HashSet<string> Tags` property to `Entity` base class
  - Implement `SetTag(string tag)`, `RemoveTag(string tag)`, `HasTag(string tag)`
  - Update `Entity` constructor to initialize tags collection

- [ ] **T2: Add tag lookup to `EntitySystem.cs` (1 pt)** ⭐ User-facing
  - Add `Dictionary<string, List<Entity>>` for O(1) tag-based lookups
  - Implement `GetEntitiesByTag(string tag)` method
  - Update `CreateEntity<T>()` and `DestroyEntity()` to maintain tag index
  - Add `FindByTag(string tag)` convenience method

- [ ] **T3: Write unit tests (1 pt)** 🔁 Validation
  - Test `SetTag`, `RemoveTag`, `HasTag` on individual entities
  - Test `GetEntitiesByTag` returns correct entities
  - Test tag index updates when entities are created/destroyed
  - Test edge cases (null tags, duplicate tags, empty tag strings)

---

## Acceptance Criteria

- [ ] `Entity` class has `Tags` property and tag management methods
- [ ] `EntitySystem` maintains tag index for fast lookups
- [ ] `GetEntitiesByTag()` returns all entities with the specified tag
- [ ] Tag index is automatically updated when entities are created/destroyed
- [ ] Project builds cleanly (`dotnet build CoreEssentials`) — **0 errors, 0 warnings**
- [ ] All existing tests pass + new tag tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Entity.cs` | Modified | ⭐ PUBLIC | Add `Tags` property and tag methods |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add tag index and lookup methods |
| `EntityTagsTests.cs` | New | 🔒 Internal | Unit tests for tagging functionality |

---

## Notes & Risks

- **Low risk** — this is a simple additive feature with no breaking changes
- Consider whether tags should be case-sensitive (recommend: case-insensitive for usability)
- Tag index maintenance in `EntitySystem` should be O(1) for common operations

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
