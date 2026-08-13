# Sprint 13 — Game State Serialization 💾

**Points:** 6.5  
**Status:** Complete  
**Sprint Goal:** Serialize and restore the full entity state for save games.

**Dependencies:** Sprint 10 (XML Entity Definitions), Sprint 12 (Entity IDs)

---

## Tasks

- [x] **T1: Create `GameStateSerializer` class (2 pts)** ⭐ User-facing
  - `SaveState(EntitySystem system, string filePath)` method ✓
  - `LoadState(EntitySystem system, string filePath, bool mergeExisting)` method ✓
  - Serialize entity positions, rotations, components ✓
  - Support `<GameState>` XML schema ✓
  - Created: `CoreEssentials/src/GameSystems/EntitySystems/EntityOOPSystem/Serialization/GameStateSerializer.cs`

- [x] **T2: Entity-driven serialization (2 pts)** ⭐ User-facing
  - Virtual `SerializeToXml()` and `DeserializeFromXml(mergeExisting)` on Entity base class ✓
  - Base methods save Id, Type, Position, Rotation, Scale, Sort, Active, Tags ✓
  - Entities override to add custom state (physics velocity, sprite color, etc.) ✓
  - Deferred component restoration pattern for components created in `OnStart()` ✓
  - Merge mode preserves runtime tags ✓

- [x] **T3: Add merge mode support (1 pt)** ⭐ User-facing
  - Merge existing entities with saved state ✓
  - Handle entity creation/deletion during merge ✓
  - Preserve runtime-only entities ✓
  - Implemented in GameStateSerializer.LoadStateFromXml()

- [x] **T4: Write unit tests (1 pt)** 🔁 Validation
  - Test save/load round-trip ✓
  - Test merge mode ✓
  - Test entity-driven serialization (custom state, deferred components) ✓
  - Test color round-trip with uint packed values ✓
  - Test merge mode tag preservation ✓
  - Created: `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/Serialization/GameStateSerializerTests.cs`
  - Created: `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/Serialization/EntityDrivenSerializationTests.cs` (9 tests)

- [x] **T5: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/GameStateSerialization.md` user guide ✓
  - Document save/load API ✓
  - Document merge mode usage ✓
  - Provide save game examples ✓
  - Created: `docs/GameStateSerialization.md`

---

## Acceptance Criteria

- [x] Full entity state can be saved to XML
- [x] Saved state can be loaded and restored
- [x] Merge mode preserves runtime entities
- [x] Components are serialized/deserialized
- [ ] Project builds cleanly — **0 errors, 0 warnings** (builds with warnings only)
- [x] All existing tests pass + new serialization tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/GameStateSerializer.cs` | Modified | ⭐ PUBLIC | Refactored to entity-driven approach ✓ |
| `Entity.cs` | Modified | ⭐ PUBLIC | Virtual `SerializeToXml()` / `DeserializeFromXml(mergeExisting)` ✓ |
| `Serialization/EntitySerializer.cs` | Modified | ⭐ PUBLIC | Scale migration for backward compatibility ✓ |
| `Ball.cs` (Playground) | Modified | 🔒 Demo | Physics + color round-trip with deferred restore ✓ |
| `GameStateSerializerTests.cs` | Modified | 🔒 Internal | Updated for entity-driven approach ✓ |
| `EntityDrivenSerializationTests.cs` | New | 🔒 Internal | 9 tests: transform, color uint, merge mode, deferred pattern ✓ |
| `docs/GameStateSerialization.md` | Modified | ⭐ PUBLIC | Entity-driven API docs with examples ✓ |

---

## Progress Notes

### Completed
- GameStateSerializer refactored from component-driven to entity-driven approach
- Entity base class provides virtual `SerializeToXml()` / `DeserializeFromXml(mergeExisting)`
- Merge mode preserves runtime tags (was clearing them before)
- Deferred component restoration pattern for components created in `OnStart()`
- Ball physics velocity + sprite color round-trip working
- Scale migration in EntitySerializer for backward compatibility with old XML format
- 9 new entity-driven serialization tests added (664 total passing)
- User documentation updated with entity-driven approach and examples

### Issues Fixed
- ~~ISerializableComponent~~ → Replaced with entity-driven approach (entities declare what to save)
- ~~Components null during deserialization~~ → Deferred restore pattern stores XML, applies after OnStart()
- ~~Color round-trip failing for white/red~~ → Use `Color.PackedValue` (uint) instead of signed int
- ~~Merge mode clearing runtime tags~~ → Added `mergeExisting` flag to `DeserializeFromXml()`
- ~~Old XML Scale on SpriteComponent~~ → EntitySerializer redirects to Entity.Scale

### Next Steps
1. Consider removing `ISerializableComponent` interface (deprecated but kept for backward compat)
2. Create PR from `feature/entity-driven-serialization` to `development`
3. Mark sprint complete ✅

---

## Notes & Risks

- **High risk** — complex serialization with many edge cases
- Versioning is critical for save game compatibility
- Need to handle entity creation/deletion between saves
- Entity type resolution needs testing with real entities

---

*Created: 2026-08-07 | Updated: 2026-08-12 | Part of Entity System Enhancements Project*

---

## Architecture Decision: Entity-Driven Serialization

**Why not `ISerializableComponent`?** Components are internal implementation details. The entity knows what matters for gameplay state, so it should declare what to save. This makes serialization:

- **Transparent** — read the entity class to see exactly what's saved
- **Testable** — call `entity.SerializeToXml()` directly in unit tests
- **Easy for devs** — just override two methods, no interfaces to implement on components

**How deferred restoration works:**

```
LoadState → CreateEntity (no OnStart yet) → DeserializeFromXml() → OnStart()
                                                          ↑
                                              Components are null here!
                                              So we store XML elements and apply them after OnStart()
```

