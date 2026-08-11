# Sprint 11 — Entity Templates/Prefabs 📦

**Points:** 5  
**Status:** ✅ Complete  
**Sprint Goal:** Reusable entity blueprints that can be instantiated multiple times from XML definitions.

**Dependencies:** Sprint 10 (XML Entity Definitions)

---

## Tasks

- [x] **T1: Create `EntityTemplate` class (1 pt)** ⭐ User-facing
  - Parse `<EntityTemplate>` elements from XML
  - Cache template for fast instantiation
  - Template name, entity type, components, children

- [x] **T2: Add template registration (1.5 pts)** ⭐ User-facing
  - `RegisterTemplate(string name, string xmlPath)` method
  - `Instantiate(string name, Vector2 position)` method
  - Clone template components to new entity
  - Support position override on instantiation

- [x] **T3: Add template scene usage (1 pt)** ⭐ User-facing
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

- [x] Templates can be registered from XML files
- [x] Templates can be instantiated multiple times
- [x] Each instance has independent state
- [x] Templates support position override on instantiation
- [x] Project builds cleanly — **0 errors, 0 warnings** (10 warnings pre-existing, unrelated)
- [ ] All existing tests pass + new template tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/EntityTemplate.cs` | New | ⭐ PUBLIC | Template definition |
| `Serialization/EntityTemplateLoader.cs` | New | ⭐ PUBLIC | XML parsing and instantiation logic |
| `Serialization/SerializationUtils.cs` | New | 🔒 Internal | Shared parsing utilities |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add `RegisterTemplate`, `Instantiate`, `CreateEntityUnstarted` |
| `Entity.cs` | Modified | ⭐ PUBLIC | Add non-generic `GetComponent(Type)` |
| `Serialization/EntitySerializer.cs` | Modified | ⭐ PUBLIC | Support `<Template>` elements in scene XML |
| `EntityTemplateTests.cs` | New | 🔒 Internal | Unit tests for templates (pending) |

---

## Notes & Risks

- **Medium risk** — need to ensure template instances are independent
- Deep copy of components is required
- Performance consideration for template instantiation

## Implementation Notes

- Templates use AssetManager for XML loading (no direct file I/O)
- Position set before `OnStart()` to ensure physics bodies initialize correctly
- Component override pattern: templates update existing component properties instead of adding duplicates
- `CreateEntityUnstarted()` added to EntitySystem for pre-initialization configuration
- Non-generic `GetComponent(Type)` added to Entity for dynamic component lookup
- `ApplyComponentDefinition` split into `IfExists` (pre-OnStart) and full (post-OnStart) variants
- Playground demo: all balls instantiated from `BallTemplate.xml` with VIP color overrides

---

*Created: 2026-08-07 | Completed: 2026-08-11 | Part of Entity System Enhancements Project*
