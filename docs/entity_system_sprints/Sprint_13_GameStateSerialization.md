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
  - Virtual `SerializeToXml()` on Entity base class for saving state ✓
  - Virtual `RestoreState(element, mergeTags)` on Entity base class for post-OnStart restoration ✓
  - Base methods save/restore Id, Type, Position, Rotation, Scale, Sort, Active, Tags ✓
  - Entities override to add custom state (physics velocity, sprite color, etc.) ✓
  - Clean single-pass flow: `CreateEntity()` → `OnStart()` → `RestoreState()` — components guaranteed to exist ✓
  - Merge mode preserves runtime tags via `mergeTags` parameter ✓

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
| `Serialization/GameStateSerializer.cs` | Modified | ⭐ PUBLIC | Single-pass CreateEntity → RestoreState flow ✓ |
| `Entity.cs` | Modified | ⭐ PUBLIC | Virtual `SerializeToXml()` / `RestoreState(mergeTags)` ✓ |
| `Serialization/EntitySerializer.cs` | Modified | ⭐ PUBLIC | Scale migration for backward compatibility ✓ |
| `Ball.cs` (Playground) | Modified | 🔒 Demo | Physics + color round-trip via RestoreState ✓ |
| `GameStateSerializerTests.cs` | Modified | 🔒 Internal | Updated for entity-driven approach ✓ |
| `EntityDrivenSerializationTests.cs` | New | 🔒 Internal | 9 tests: transform, color uint, merge mode, deferred pattern ✓ |
| `docs/GameStateSerialization.md` | Modified | ⭐ PUBLIC | Entity-driven API docs with examples ✓ |

---

## Progress Notes

### Completed
- GameStateSerializer uses clean single-pass CreateEntity → RestoreState flow
- Entity base class provides virtual `SerializeToXml()` / `RestoreState(mergeTags)`
- Components guaranteed to exist when RestoreState() is called (post-OnStart)
- Merge mode preserves runtime tags via `mergeTags` parameter
- Ball physics velocity + sprite color round-trip working
- Scale migration in EntitySerializer for backward compatibility with old XML format
- 9 new entity-driven serialization tests added (664 total passing)
- User documentation updated with entity-driven approach and examples

### Issues Fixed
- ~~ISerializableComponent~~ → Replaced with entity-driven approach (entities declare what to save)
- ~~Components null during deserialization~~ → Eliminated: RestoreState() runs AFTER OnStart(), components always exist
- ~~Color round-trip failing for white/red~~ → Use `Color.PackedValue` (uint) instead of signed int
- ~~Merge mode clearing runtime tags~~ → `mergeTags` parameter on `RestoreState()` preserves existing tags
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

**How the clean lifecycle works:**

```
LoadState → CreateEntity(type) → OnStart() runs → Components exist → RestoreState(element)
```

No deferred state needed — `RestoreState()` is called AFTER `OnStart()`, so all components created during initialization are fully initialized and ready to use. For merge mode, pre-existing entities get `mergeTags: true` to preserve their runtime tags.

