# Sprint 15 — Debug Visualization 🔍

**Points:** 3.5  
**Status:** Not Started  
**Sprint Goal:** Draw entity metadata in debug mode for visual debugging.

**Dependencies:** Sprint 0 (Tags), Sprint 16 (Lifecycle Hooks)

---

## Tasks

- [ ] **T1: Create `DebugConfig` class (0.5 pt)** ⭐ User-facing
  - `ShowEntityBounds` property
  - `ShowEntityIds` property
  - `ShowEntityTags` property
  - `ShowEntityHierarchy` property
  - `ShowEntityPosition` property

- [ ] **T2: Create `EntityDebugDraw` class (1.5 pts)** 🔒 Internal
  - Draw bounding boxes using `PrimitiveBatch`
  - Draw entity IDs as text
  - Draw tags as text
  - Draw parent-child lines
  - Draw position markers

- [ ] **T3: Add debug mode to `EntitySystem` (0.5 pt)** ⭐ User-facing
  - `DebugMode` property to enable/disable
  - Auto-render debug overlays in `Render()`
  - Configurable debug colors

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test debug mode enable/disable
  - Test individual debug overlays
  - Test performance impact of debug mode

- [ ] **T5: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/EntityDebugVisualization.md` user guide
  - Document debug configuration
  - Document debug overlays
  - Provide debugging examples

---

## Acceptance Criteria

- [ ] Debug mode can be enabled/disabled
- [ ] Entity bounds are drawn in debug mode
- [ ] Entity IDs and tags are displayed
- [ ] Parent-child hierarchy is visualized
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new debug tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Debug/DebugConfig.cs` | New | ⭐ PUBLIC | Debug configuration |
| `Debug/EntityDebugDraw.cs` | New | 🔒 Internal | Debug drawing logic |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add debug mode |
| `EntityDebugTests.cs` | New | 🔒 Internal | Unit tests for debug visualization |
| `docs/EntityDebugVisualization.md` | New | ⭐ PUBLIC | User guide for debug visualization |

---

## Notes & Risks

- **Low risk** — additive feature with no impact on production code
- Performance impact should be minimal (only when debug mode is enabled)
- Consider debug mode as developer-only feature

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
