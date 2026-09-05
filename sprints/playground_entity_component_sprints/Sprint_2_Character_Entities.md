# Sprint 2 — Character Entities 🏃

**Points:** 5 | **Status:** ⬜ Not Started | **Goal:** Migrate `AnimatedCharacterEntity`, `CharacterEntity`, and `PlayerEntity` onto components, delete all three classes, and eliminate the `PlayerEntity : CharacterEntity` inheritance.

## Why These Three Together

They share the same building blocks (a character sprite + optional animation + optional movement) and are coupled by inheritance. Converting them together removes the subclass relationship that is fundamentally incompatible with a data-driven model:

- `CharacterEntity` = `SpriteComponent` + a **looping Y-bounce tween** applied in `Update()` + pause-scale.
- `AnimatedCharacterEntity` = animated `SpriteComponent` + `AnimationComponent` (`walk`) + pause-scale. No movement.
- `PlayerEntity : CharacterEntity` = character visual + **arrow-key movement** in `Update()`.

The reusable pieces that don't exist yet are a **bounce tween component**, a **move-by-keys component**, and a small **pause-scale behavior**. Once those are components, each of the three entities is just `GameEntity` + a different set of `<Component>` declarations.

## Target Outcome

- New `BounceTweenComponent` (looping Y offset via `TweenComponent`), `MoveByKeysComponent` (arrow-key movement), and a small pause-scale behavior (component or shared helper).
- `CharacterTemplate.xml`, `CharacterScene.xml`, and `CameraScene.xml` (`player`) repointed at `GameEntity` with the appropriate component sets.
- `Entities/CharacterEntity.cs`, `AnimatedCharacterEntity.cs`, `PlayerEntity.cs` deleted; inheritance removed.

## Tasks

- [ ] T1 ⭐ Create `BounceTweenComponent : EntityComponent` — encapsulate the `CharacterEntity` bounce (capture original Y on first frame, apply tweened offset). Expose amplitude/duration/easing/loop as properties.
- [ ] T2 ⭐ Create `MoveByKeysComponent : EntityComponent` — arrow-key movement using `Input.Keyboard` + `Time.DeltaTime`; expose speed + key bindings as properties (mirrors the existing `CameraInputComponent` declarative style).
- [ ] T3 ⭐ Add pause-scale behavior. `Entity.OnApplicationPause` forwards to components, so a small component (e.g. `PauseScaleComponent`) that scales the entity while paused covers both character entities. Confirm the forward path in `Entity.cs`.
- [ ] T4 🔁 Update `Content/Templates/CharacterTemplate.xml` → `GameEntity` + `SpriteComponent` (character sprite) + `BounceTweenComponent` (+ pause-scale).
- [ ] T5 🔁 Update `Content/Scenes/CharacterScene.xml`: `staticCharacter` (`CharacterEntity`) and `animatedCharacter` (`AnimatedCharacterEntity`) → `GameEntity` with the right component sets (animated uses `AnimationComponent` + walk; static uses bounce).
- [ ] T6 🔁 Update `Content/Scenes/CameraScene.xml` `player` (`PlayerEntity`) → `GameEntity` + character visual components + `MoveByKeysComponent`.
- [ ] T7 🔁 Delete `Entities/CharacterEntity.cs`, `AnimatedCharacterEntity.cs`, `PlayerEntity.cs`; remove references/imports.
- [ ] T8 🔒 Add unit tests: `BounceTweenComponent` (offset applied relative to captured Y), `MoveByKeysComponent` (each key moves the expected axis/direction), pause-scale component (scale toggles on pause state).
- [ ] T9 🔒 Build clean + full suite green (expect 1174/0/3) + smoke-run all 7 scenes PASS.

## Acceptance Criteria

- No code references `CharacterEntity`, `AnimatedCharacterEntity`, or `PlayerEntity`; inheritance is gone.
- Character, animated character, and player behave identically when declared from XML (bounce, animation playback, arrow-key movement, pause-scale).
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `Components/BounceTweenComponent.cs` | Create | Looping Y-bounce behavior |
| `Components/MoveByKeysComponent.cs` | Create | Arrow-key movement |
| `Components/PauseScaleComponent.cs` (or shared helper) | Create | Scale-while-paused behavior |
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

---
*Created: 2026-09-05 | Part of Playground Entity → Components Project*
