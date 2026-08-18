# Sprint 8 — Entity Groups/Layers 📚

**Points:** 4  
**Status:** Not Started  
**Sprint Goal:** Logical grouping for independent update/render control.

---

## Tasks

- [ ] **T1: Create `EntityLayer` class (1 pt)** ⭐ User-facing
  - Layer name, update enabled/render enabled flags
  - Entity list for this layer
  - Sort order within layer

- [ ] **T2: Add layer management to `EntitySystem` (2 pts)** ⭐ User-facing
  - `CreateLayer(string name)` method
  - `CreateEntity<T>(position, layerName)` overload
  - `UpdateLayer(layerName, gameTime)` method
  - `RenderLayer(layerName, spriteBatch)` method
  - Default layer for entities without explicit layer

- [ ] **T3: Write unit tests (1 pt)** 🔁 Validation
  - Test layer creation and entity assignment
  - Test selective layer update/render
  - Test default layer behavior
  - Test layer visibility toggle

---

## Acceptance Criteria

- [ ] Layers can be created and named
- [ ] Entities can be assigned to layers
- [ ] Layers can be selectively updated/rendered
- [ ] Default layer exists for unassigned entities
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new layer tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Layers/EntityLayer.cs` | New | ⭐ PUBLIC | Layer definition |
| `Layers/LayerManager.cs` | New | 🔒 Internal | Layer management |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add layer-aware methods |
| `EntityLayerTests.cs` | New | 🔒 Internal | Unit tests for layers |

---

## Notes & Risks

- **Low risk** — simple additive feature
- Consider layer inheritance (e.g., "enemies" inherits from "foreground")
- Performance impact of layer iteration

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
