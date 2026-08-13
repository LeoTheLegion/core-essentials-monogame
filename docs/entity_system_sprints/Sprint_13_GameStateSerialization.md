# Sprint 13 — Game State Serialization 💾

**Points:** 6.5  
**Status:** Complete  
**Sprint Goal:** Serialize and restore the full entity state for save games, with clean component lifecycle management.

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
  - Test physics scene serialization (entity IDs, positions) ✓
  - Test component lifecycle cleanup on entity destroy ✓
  - Created: `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/Serialization/GameStateSerializerTests.cs`
  - Created: `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/Serialization/EntityDrivenSerializationTests.cs` (9 tests)
  - Created: `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/Serialization/PhysicsSceneSerializationTests.cs`
  - Updated: `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/EntityComponentTests.cs`

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
- [x] Project builds cleanly — **0 errors** (pre-existing warnings only)
- [x] All existing tests pass + new serialization tests added (668 total, 2 skipped)

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Serialization/GameStateSerializer.cs` | New | ⭐ PUBLIC | Single-pass CreateEntity → RestoreState flow ✓ |
| `Serialization/ISerializableComponent.cs` | New | 🔒 INTERNAL | Component serialization interface (kept for backward compat) ✓ |
| `Entity.cs` | Modified | ⭐ PUBLIC | Virtual `SerializeToXml()` / `RestoreState(mergeTags)`, Scale property, improved lifecycle ✓ |
| `EntitySystem.cs` | Modified | ⭐ PUBLIC | Entity type registration, improved CreateEntity ✓ |
| `Serialization/EntitySerializer.cs` | Modified | ⭐ PUBLIC | Scale migration for backward compatibility ✓ |
| `Components/BuiltIn/SpriteComponent.cs` | Modified | 🔒 INTERNAL | ISerializableComponent implementation, Color serialization ✓ |
| `Components/BuiltIn/RigidbodyComponent.cs` | Modified | 🔒 INTERNAL | Component-level Position/Rotation/LinearVelocity properties, Body → internal, OnDetach cleanup ✓ |
| `Components/BuiltIn/ColliderComponent.cs` | Modified | 🔒 INTERNAL | ISerializableComponent implementation ✓ |
| `physics/types/IPhysicsBody.cs` | Modified | ⭐ PUBLIC | Settable `Position { get; set; }` property, WorldPosition [Obsolete] alias ✓ |
| `physics/engines/aether/PhysicsBody.cs` | Modified | 🔒 INTERNAL | Position setter using SetTransform ✓ |
| `physics/engines/aether/PhysicsDebugRenderer.cs` | Modified | 🔒 INTERNAL | Updated to use Position instead of WorldPosition ✓ |
| `Ball.cs` (Playground) | Modified | 🔒 Demo | Physics + color round-trip via RestoreState, component-level API usage ✓ |
| `PhysicsEntityScene.cs` (Playground) | Modified | 🔒 Demo | Save/load integration with physics scene ✓ |
| `WorldBorder.cs` (Playground) | Modified | 🔒 Demo | Updated for serialization compatibility ✓ |
| `GameStateSerializerTests.cs` | New | 🔒 Internal | Save/load round-trip, merge mode tests ✓ |
| `EntityDrivenSerializationTests.cs` | New | 🔒 Internal | 9 tests: transform, color uint, merge mode, deferred pattern ✓ |
| `PhysicsSceneSerializationTests.cs` | New | 🔒 Internal | Entity ID preservation, position/rotation round-trip ✓ |
| `EntityComponentTests.cs` | Modified | 🔒 Internal | Component lifecycle cleanup tests ✓ |
| `docs/GameStateSerialization.md` | New | ⭐ PUBLIC | Entity-driven API docs with examples ✓ |

---

## Progress Notes

### Completed
- GameStateSerializer uses clean single-pass CreateEntity → RestoreState flow
- Entity base class provides virtual `SerializeToXml()` / `RestoreState(mergeTags)`
- Components guaranteed to exist when RestoreState() is called (post-OnStart)
- Merge mode preserves runtime tags via `mergeTags` parameter
- Ball physics velocity + sprite color round-trip working
- Scale migration in EntitySerializer for backward compatibility with old XML format
- **IPhysicsBody.Position** settable property added (replaces read-only WorldPosition)
- **RigidbodyComponent encapsulation** — Body property internal, component-level Position/Rotation/LinearVelocity properties
- **Component lifecycle cleanup** — OnDetach() → DestroyBody() handles physics body cleanup; no manual cleanup needed in entity OnDestroy()
- 668 tests passing (2 skipped), including new serialization and lifecycle tests
- User documentation updated with entity-driven approach and examples

### Issues Fixed
- ~~ISerializableComponent~~ → Replaced with entity-driven approach (entities declare what to save)
- ~~Components null during deserialization~~ → Eliminated: RestoreState() runs AFTER OnStart(), components always exist
- ~~Color round-trip failing for white/red~~ → Use `Color.PackedValue` (uint) instead of signed int
- ~~Merge mode clearing runtime tags~~ → `mergeTags` parameter on `RestoreState()` preserves existing tags
- ~~Old XML Scale on SpriteComponent~~ → EntitySerializer redirects to Entity.Scale
- ~~Ball spawns at (0,0) after load~~ → Sync physics body in RestoreState via component-level Position/Rotation properties
- ~~RigidbodyComponent.Body publicly accessible~~ → Made internal; added component-level proxy properties
- ~~Manual DestroyBody() calls in entity OnDestroy()~~ → Handled by component lifecycle (OnDetach → DestroyBody)

### Next Steps
1. Consider removing `ISerializableComponent` interface (deprecated but kept for backward compat)
2. Create PR from `feature/entity-driven-serialization` to `development`
3. Mark sprint complete ✅

---

## Additional Work Completed

### Physics API Improvements
- **IPhysicsBody.Position** — Added settable `Position { get; set; }` property to IPhysicsBody interface, implemented in PhysicsBody using SetTransform internally
- **WorldPosition deprecation** — Kept as `[Obsolete]` alias for backward compatibility
- **RigidbodyComponent encapsulation** — Made Body property internal; added component-level Position, Rotation, LinearVelocity properties that delegate to the internal body
- **PhysicsDebugRenderer** — Updated to use new Position property

### Component Lifecycle Improvements
- **OnDetach cleanup** — RigidbodyComponent.OnDetach() calls DestroyBody(), ensuring physics bodies are cleaned up when entities are destroyed
- **No manual cleanup needed** — Entity.OnDestroy() calls OnDetach() on all components, so subclass OnDestroy() doesn't need to manually destroy physics bodies
- **Coroutine cleanup** — Non-component resources (like CoroutineOwner) still cleaned up explicitly in entity OnDestroy()

### Scale Property Migration
- Added `Scale` property to Entity base class (was previously only on SpriteComponent)
- EntitySerializer handles backward compatibility for old XML format with Scale on SpriteComponent

