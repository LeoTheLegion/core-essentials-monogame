# Sprint 10 — XML Entity Definitions 📄

**Points:** 7  
**Status:** Not Started  
**Sprint Goal:** Declarative entity definitions using XML, matching existing asset and GUI layout patterns.

**Dependencies:** Sprint 0 (Tags), Sprint 6 (Components), Sprint 12 (IDs)

---

## Tasks

- [ ] **T1: Create `EntitySerializer` class (2 pts)** ⭐ User-facing
  - `LoadEntity<T>(string xmlData, EntitySystem system)` method
  - `SaveEntity(Entity entity, string filePath)` method
  - Parse `<EntityType>`, `<Position>`, `<Rotation>`, `<Sort>`, `<Tag>` elements
  - Reuse `XDocument`/`XElement` parsing from `GuiSerializer`

- [ ] **T2: Add component loading (2 pts)** ⭐ User-facing
  - Parse `<Components>` section with `<Component Type="...">` elements
  - Parse `<Property Name="..." Value="...">` for simple types
  - Support int, float, string, bool, Vector2, Color
  - Component registration via `IComponentFactory` dictionary

- [ ] **T3: Add scene loading (1.5 pts)** ⭐ User-facing
  - `LoadSceneFromXml(string filePath)` method
  - Parse multiple `<EntityDefinition>` elements
  - Support `<Children>` for parent-child hierarchy
  - Support `<Reference>` for entity linking by ID

- [ ] **T4: Write unit tests (1.5 pts)** 🔁 Validation
  - Test load single entity from XML
  - Test load entity with components
  - Test load scene from XML
  - Test save/load round-trip
  - Test invalid XML handling

---

## Acceptance Criteria

- [ ] Entities can be loaded from XML files
- [ ] Components are parsed and attached
- [ ] Scenes can be loaded from XML
- [ ] Parent-child hierarchy is supported in XML
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new XML tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/EntitySerializer.cs` | New | ⭐ PUBLIC | XML serialization |
| `Serialization/IComponentFactory.cs` | New | ⭐ PUBLIC | Component factory interface |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add `LoadEntityFromXml`, `LoadSceneFromXml` |
| `EntitySerializerTests.cs` | New | 🔒 Internal | Unit tests for XML serialization |

---

## Notes & Risks

- **High risk** — complex parsing logic with many edge cases
- XML schema should be versioned for future compatibility
- Error handling for missing components or invalid properties

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
