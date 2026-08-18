# Sprint 5 — Render Batching 🎨

**Points:** 4  
**Status:** Not Started  
**Sprint Goal:** Sort entities by texture before drawing to minimize state changes.

---

## Tasks

- [x] **T1: Add texture tracking to entities (1 pt)** 🔒 Internal
  - Entities expose current active texture for rendering
  - Track texture changes during render preparation

- [x] **T2: Implement texture-based sort in `EntitySystem.Draw()` (2 pts)** ⭐ User-facing
  - Group entities by active texture before rendering
  - Single `SpriteBatch.Begin()` per texture group
  - Maintain sort order within each texture group

- [x] **T3: Write unit tests (1 pt)** 🔁 Validation
  - Test entities are grouped by texture
  - Test sort order is preserved within groups
  - Test entities without textures render correctly

---

## Acceptance Criteria

- [x] Entities are sorted by texture before drawing
- [x] SpriteBatch is begun/ended per texture group
- [x] Sort order is preserved within texture groups
- [x] Project builds cleanly — **0 errors, 0 warnings**
- [x] All existing tests pass + new batching tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add texture-based render batching |
| `EntityRenderBatchTests.cs` | New | 🔒 Internal | Unit tests for render batching |

---

## Notes & Risks

- **Medium risk** — need to handle entities with multiple textures
- Performance gain depends on entity count and texture variety
- Consider blend state changes that might break batching

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
