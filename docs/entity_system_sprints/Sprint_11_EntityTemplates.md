# Sprint 11 — Entity Templates/Prefabs 📦

**Points:** 5  
**Status:** Not Started  
**Sprint Goal:** Reusable entity blueprints that can be instantiated multiple times from XML definitions.

**Dependencies:** Sprint 10 (XML Entity Definitions)

---

## Tasks

- [ ] **T1: Create `EntityTemplate` class (1 pt)** ⭐ User-facing
  - Parse `<EntityTemplate>` elements from XML
  - Cache template for fast instantiation
  - Template name, entity type, components, children

- [ ] **T2: Add template registration (1.5 pts)** ⭐ User-facing
  - `RegisterTemplate(string name, string xmlPath)` method
  - `Instantiate(string name, Vector2 position)` method
  - Clone template components to new entity
  - Support position override on instantiation

- [ ] **T3: Add template scene usage (1 pt)** ⭐ User-facing
  - Parse `<Template Source="..." Position="...">` in scene XML
  - Instantiate templates at specified positions
  - Support template overrides (e.g., different tags per instance)

- [ ] **T4: Write unit tests (1.5 pts)** 🔁 Validation
  - Test template registration and instantiation
  - Test multiple instances from same template
  - Test template with components
  - Test template with children

---

## Acceptance Criteria

- [ ] Templates can be registered from XML files
- [ ] Templates can be instantiated multiple times
- [ ] Each instance has independent state
- [ ] Templates support position override on instantiation
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new template tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/EntityTemplate.cs` | New | ⭐ PUBLIC | Template definition |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add `RegisterTemplate`, `Instantiate` |
| `EntityTemplateTests.cs` | New | 🔒 Internal | Unit tests for templates |

---

## Notes & Risks

- **Medium risk** — need to ensure template instances are independent
- Deep copy of components is required
- Performance consideration for template instantiation

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
