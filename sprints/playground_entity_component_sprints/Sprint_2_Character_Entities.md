# Sprint 2 — Character Entities 🏃

**Points:** 5 | **Status:** ✅ Completed (2026-09-05) | **Goal:** Migrate `AnimatedCharacterEntity`, `CharacterEntity`, and `PlayerEntity` onto components, delete all three classes, and eliminate the `PlayerEntity : CharacterEntity` inheritance.

## Why These Three Together

They share the same building blocks (a character sprite + optional animation + optional movement) and are coupled by inheritance. Converting them together removes the subclass relationship that is fundamentally incompatible with a data-driven model:

- `CharacterEntity` = `SpriteComponent` + a **looping Y-bounce tween** applied in `Update()` + pause-scale.
- `AnimatedCharacterEntity` = animated `SpriteComponent` + `AnimationComponent` (`walk`) + pause-scale. No movement.
- `PlayerEntity : CharacterEntity` = character visual + **arrow-key movement** in `Update()`.

The reusable pieces that don't exist yet are a **bounce tween component**, a **move-by-keys component**, and a small **pause-scale behavior**. Once those are components, each of the three entities is just `GameObjectEntity` + a different set of `<Component>` declarations.

## Target Outcome

- New `BounceTweenComponent` (looping Y offset via `TweenComponent`), `MoveByKeysComponent` (arrow-key movement), and a small pause-scale behavior (component or shared helper).
- `CharacterTemplate.xml`, `CharacterScene.xml`, and `CameraScene.xml` (`player`) repointed at `GameObjectEntity` with the appropriate component sets.
- `Entities/CharacterEntity.cs`, `AnimatedCharacterEntity.cs`, `PlayerEntity.cs` deleted; inheritance removed.

## Tasks

- [x] T1 ⭐ Create `BounceTweenComponent : EntityComponent` — encapsulate the `CharacterEntity` bounce (capture original Y on first frame, apply tweened offset). Expose amplitude/duration/easing/loop as properties.
- [x] T2 ⭐ Create `MoveByKeysComponent : EntityComponent` — arrow-key movement using `Input.Keyboard` + `Time.DeltaTime`; expose speed + key bindings as properties (mirrors the existing `CameraInputComponent` declarative style).
- [x] T3 ⭐ Add pause-scale behavior. `Entity.OnApplicationPause` forwards to components, so a small component (`PauseScaleComponent`) that scales the entity while paused covers both character entities. Forward path confirmed in `Entity.cs`.
- [x] T3b ⭐ Create `CharacterSpriteLoader` (loads a sprite asset → hands it to a pre-declared `SpriteComponent`) and `CharacterWalkAnimation` (loads an animated sprite → wires the pre-declared `SpriteComponent` + `AnimationComponent`). Needed because `ParseValue` cannot bind a `Sprite` property from an XML string — the asset must be loaded in code.
- [x] T4 🔁 Update `Content/Templates/CharacterTemplate.xml` → `GameObjectEntity` + `SpriteComponent` + `CharacterSpriteLoader` + `BounceTweenComponent` + `PauseScaleComponent`.
- [x] T5 🔁 Update `Content/Scenes/CharacterScene.xml`: `staticCharacter` and `animatedCharacter` → `GameObjectEntity` with the right component sets (animated uses `AnimationComponent` + `CharacterWalkAnimation`; static uses `BounceTweenComponent`).
- [x] T6 🔁 Update `Content/Scenes/CameraScene.xml` `player` → `GameObjectEntity` + character visual components + `MoveByKeysComponent`.
- [x] T7 🔁 Delete `Entities/CharacterEntity.cs`, `AnimatedCharacterEntity.cs`, `PlayerEntity.cs`; remove references/imports.
- [x] T8 🔒 Add unit tests (`CharacterComponentTests`, 11 tests): bounce (baseline capture + offset relative to Y, X untouched), move-by-keys (each key moves the expected axis/direction; no-op when idle), pause-scale (scale toggles on pause state; configured factor), sprite loader (assigns to pre-declared component; no-op without one), walk animation (wires siblings + plays). Also updated `Sprint5dDataSceneTests` for the new `GameObjectEntity` types.
- [x] T9 🔒 Build clean + full suite green (1191/0/3) + smoke-run all 7 scenes PASS.

> **Key design discovery:** a component must never *create* a sibling from `OnAttach` or `Update` — the entity iterates its live `_components` dictionary during both passes, so adding one throws "Collection was modified". The original entities added components in `OnStart` (the deferred-attach window), which is safe. Therefore the data-driven design **pre-declares** all sibling components in XML and has the behavior components only *configure* them (property sets + method calls). This is documented in [docs/CharacterComponents.md](../../docs/CharacterComponents.md).

## Acceptance Criteria

- No code references `CharacterEntity`, `AnimatedCharacterEntity`, or `PlayerEntity`; inheritance is gone.
- Character, animated character, and player behave identically when declared from XML (bounce, animation playback, arrow-key movement, pause-scale).
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `Components/BounceTweenComponent.cs` | Create | Looping Y-bounce behavior |
| `Components/MoveByKeysComponent.cs` | Create | Arrow-key movement |
| `Components/PauseScaleComponent.cs` | Create | Scale-while-paused behavior |
| `Components/CharacterSpriteLoader.cs` | Create | Load sprite asset → hand to pre-declared SpriteComponent |
| `Components/CharacterWalkAnimation.cs` | Create | Load animated sprite → wire SpriteComponent + AnimationComponent, play walk |
| `docs/CharacterComponents.md` | Create | Usage, parameters, and examples for the new components |
| `Content/Templates/CharacterTemplate.xml` | Modify | Repoint + declare components |
| `Content/Scenes/CharacterScene.xml` | Modify | static + animated characters → components |
| `Content/Scenes/CameraScene.xml` | Modify | player → components |
| `Entities/{CharacterEntity,AnimatedCharacterEntity,PlayerEntity}.cs` | Delete | Behavior now in components |
| `CoreEssentials.Tests/**` | Add/Modify | New component tests; adjust existing references |

## Notes & Risks

- **Bounce Y-capture timing:** the current code captures `_originalY` on the first `Update` because XML position is applied *after* `OnStart`. The component must preserve this ordering (capture on attach+first update, not at construction), or the bounce baseline will be wrong for data-driven entities.
- **PlayerEntity's empty overrides:** `PlayerEntity.OnStart/OnDestroy` are no-ops; only its `Update` movement is real — `MoveByKeysComponent` captures exactly that.
- **Animation wiring:** `AnimatedCharacterEntity` calls `animation.AddAnimation("walk", sprite)` + `Play("walk")`. Confirm this can be expressed declaratively (check `AnimationComponent`'s `ISerializableComponent` surface) or add a thin declarative hook; if not cleanly declarable, keep a minimal `OnAttach` wiring in a dedicated component rather than resurrecting an entity class.
- **Verify no scene depends on the concrete types** for casting/typing (e.g. `GetComponent` chains that assumed `PlayerEntity`). Grep for the three FQNs across code + XML before deleting.

## Follow-up (2026-09-05): loader components retired in favor of built-in `SpriteAsset`

The T3b glue components (`CharacterSpriteLoader`, `CharacterWalkAnimation`) existed only because the
framework's XML binding (`SerializationUtils.ParseValue`) cannot turn a string into a `Sprite` object.
That gap is now closed **in the framework**: the built-in `SpriteComponent` gained a `SpriteAsset`
string property (loaded via `AssetManager` on attach; an explicitly assigned sprite always wins) and
the built-in `AnimationComponent` gained `SpriteAsset` + `AnimationName` (loads, registers, and plays
on attach; code-registered animations win). Both round-trip through component serialization.

Consequently:
- `Components/CharacterSpriteLoader.cs` and `Components/CharacterWalkAnimation.cs` are **deleted**.
- The XML scenes/template declare visuals directly on the built-in components (see updated examples in
  [docs/SpriteSystem.md](../../docs/SpriteSystem.md), [docs/AnimationComponent.md](../../docs/AnimationComponent.md),
  [docs/CharacterComponents.md](../../docs/CharacterComponents.md)).
- New framework tests: `SpriteAssetTests` (9 tests) cover load-on-attach, code-wins precedence,
  missing-asset tolerance, and serialization round-trip.

---
*Created: 2026-09-05 | Updated: 2026-09-05 (built-in SpriteAsset follow-up) | Part of Playground Entity → Components Project*
