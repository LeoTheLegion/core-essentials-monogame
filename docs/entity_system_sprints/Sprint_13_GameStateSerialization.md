# Sprint 13 — Game State Serialization 💾

**Points:** 6  
**Status:** Not Started  
**Sprint Goal:** Serialize and restore the full entity state for save games.

**Dependencies:** Sprint 10 (XML Entity Definitions), Sprint 12 (Entity IDs)

---

## Tasks

- [ ] **T1: Create `GameStateSerializer` class (2 pts)** ⭐ User-facing
  - `SaveState(EntitySystem system, string filePath)` method
  - `LoadState(EntitySystem system, string filePath, bool mergeExisting)` method
  - Serialize entity positions, rotations, components
  - Support `<GameState>` XML schema

- [ ] **T2: Add component serialization (2 pts)** ⭐ User-facing
  - `ISerializableComponent` interface for save/load
  - `SerializeToXml()` and `DeserializeFromXml()` methods
  - Auto-serialize built-in components
  - Handle component-specific state

- [ ] **T3: Add merge mode support (1 pt)** ⭐ User-facing
  - Merge existing entities with saved state
  - Handle entity creation/deletion during merge
  - Preserve runtime-only entities

- [ ] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test save/load round-trip
  - Test merge mode
  - Test component serialization
  - Test missing entity handling

---

## Acceptance Criteria

- [ ] Full entity state can be saved to XML
- [ ] Saved state can be loaded and restored
- [ ] Merge mode preserves runtime entities
- [ ] Components are serialized/deserialized
- [ ] Project builds cleanly — **0 errors, 0 warnings**
- [ ] All existing tests pass + new serialization tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/GameStateSerializer.cs` | New | ⭐ PUBLIC | Save/load game state |
| `Serialization/ISerializableComponent.cs` | New | ⭐ PUBLIC | Component serialization interface |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Add `SaveState`, `LoadState` |
| `GameStateSerializerTests.cs` | New | 🔒 Internal | Unit tests for game state |

---

## Notes & Risks

- **High risk** — complex serialization with many edge cases
- Versioning is critical for save game compatibility
- Need to handle entity creation/deletion between saves

---

*Created: 2026-08-07 | Part of Entity System Enhancements Project*
