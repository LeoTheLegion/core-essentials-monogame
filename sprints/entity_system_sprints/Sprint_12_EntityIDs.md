# Sprint 12 — Entity IDs & References 🔖

**Points:** 4.5  
**Status:** Complete ✅  
**Sprint Goal:** Unique identifiers and cross-entity linking for XML-driven scenes.

---

## Tasks

- [x] **T1: Add ID storage to `Entity` (1 pt)** ⭐ User-facing
  - `string Id` property for unique identifier
  - `SetId(string id)` method
  - Auto-generate IDs if not provided

- [x] **T2: Add ID lookup to `EntitySystem` (1.5 pts)** ⭐ User-facing
  - `Dictionary<string, Entity>` for ID index
  - `FindById(string id)` method
  - `ResolveReferences()` method for cross-entity linking

- [x] **T3: Add reference resolution (1 pt)** ⭐ User-facing
  - `EntityReference` class for deferred entity lookup
  - Auto-resolve references after scene load
  - Handle missing references gracefully

- [x] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test ID assignment and lookup
  - Test reference resolution
  - Test duplicate ID handling
  - Test missing reference handling

- [x] **T5: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/EntityIDs.md` user guide
  - Document ID assignment and lookup
  - Document reference resolution
  - Provide XML examples with `<Reference>` elements

---

## Acceptance Criteria

- [x] Entities have unique IDs
- [x] `FindById()` returns correct entity
- [x] References are resolved after scene load
- [x] Duplicate IDs are handled (error or auto-fix)
- [x] Project builds cleanly — **0 errors** (10 pre-existing warnings unrelated to this sprint)
- [x] All existing tests pass + new ID tests added (636 passed, 2 skipped)

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/EntityReference.cs` | New | ⭐ PUBLIC | Entity reference class |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add `Id` property |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add ID index and lookup |
| `EntityIdTests.cs` | New | 🔒 Internal | Unit tests for IDs and references |
| `docs/EntityIDs.md` | New | ⭐ PUBLIC | User guide for entity IDs and references |

---

## Notes & Risks

- **Low risk** — simple ID system with clear requirements
- Consider ID naming conventions (e.g., "hero", "chest_1")
- Error handling for duplicate IDs

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
