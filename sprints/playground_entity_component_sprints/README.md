# Playground Entity → Components — Scrum Sprints 🧩

The `CoreEssentials.Playground/Entities/` folder still holds 8 hand-written `Entity` subclasses that each carry behavior in `OnStart`/`Update`/`Render`/`OnDestroy` overrides. The playground is now data-driven (Scene-as-Data), so this behavior should live in **components declared from scene/prefab XML**, not in bespoke entity classes. This project converts every entity to a plain component-hosting entity and deletes the 8 classes.

> **Branch:** work lands on `feature/scene-as-data` (same branch as the Scene-as-Data + organization sprints). Zero behavior change: build clean, full suite green, and all 7 scenes smoke-run PASS before and after each sprint.

## Why This?

After the code-organization sprint, `Entities/` became a home for classes that are really "a bag of components + some inline logic." That logic (movement, bounce tweens, button wiring, border physics, save/load) belongs in reusable, declaratively-configurable components — matching how everything else in the playground already works. It also removes inheritance (`PlayerEntity : CharacterEntity`), which is incompatible with a purely data-driven entity model.

## Current State — the 8 classes

| Class | Behavior it carries today | Where it should go |
|-------|---------------------------|--------------------|
| `TextEntity` | Custom font load + aligned text rendering in `Render()` | New `TextComponent : IDrawableComponent` |
| `AnimatedCharacterEntity` | Loads animated sprite, plays `AnimationComponent`, pause-scale | Existing `SpriteComponent` + `AnimationComponent` + a small pause behavior |
| `CharacterEntity` | Sprite + looping Y-bounce tween in `Update()`, pause-scale | New `BounceTweenComponent` (+ pause behavior) |
| `PlayerEntity : CharacterEntity` | Inherits character, adds arrow-key movement in `Update()` | Character components + new `MoveByKeysComponent`; **drops inheritance** |
| `CameraEntity` | Owns `CameraComponent`, WASD/QE/R input, follow target | Mostly existing `CameraInputComponent` + `CameraFollowToggleComponent` |
| `SoundButtonEntity` | Myra `Canvas` + text button → plays one-shot sound | New `SoundButtonComponent` |
| `VolumeButtonEntity` | Myra `Canvas` + text button → sets master volume | New `VolumeButtonComponent` |
| `Ball : ISaveableEntity` | Sprite/rigidbody/collider setup, random-movement coroutine, ~150 lines of custom save/load | Built-in components + new `BallMovementComponent` + **save/load decision** |
| `WorldBorder` | Builds 4 static physics bodies from `Size` + `PhysicsConfig` in `OnStart()` | New `WorldBorderComponent` (created by `PhysicsSpawnComponent`) |

## Target State

```
CoreEssentials.Playground/
├── Entities/
│   └── GameEntity.cs   # single concrete Entity subclass (parameterless ctor) — the instantiable type for all XML entities
├── Components/               # + TextComponent, BounceTweenComponent, MoveByKeysComponent,
                              #   SoundButtonComponent, VolumeButtonComponent, WorldBorderComponent, BallMovementComponent
└── Content/                  # scene/prefab <Type=...> FQNs repointed at GameEntity; behavior moved into <Component> blocks
```

- **One shared concrete entity** replaces all 8 (because `Entity` is abstract and XML `Type=` must resolve to a concrete, instantiable type). All 8 class files are deleted.
- Every scene/prefab `<EntityDefinition Type=...>` / `<Prefab Type=...>` that referenced one of the 8 FQNs is repointed at the shared entity, with the former inline behavior expressed as declarative `<Component>` entries (with any needed properties).

**Design invariant:** `GameEntity` is a **plain game object** — it carries *no* behavior of its own. From now on the only thing a scene can instantiate is a game object that gets everything from attached components. No new `Entity` subclasses are introduced for gameplay; if a behavior needs to exist, it becomes a component.

## Framework Facts That Make This Possible

- **`Entity.Render()` auto-draws every `IDrawableComponent`** — so entities that only draw a sprite/text need no `Render()` override once their visual is a component.
- **Components receive lifecycle hooks** (`OnAttach`/`Update`/`OnDetach`, and `OnApplicationPause` is forwarded to components by `Entity.OnApplicationPause`). Behavior in `OnStart`/`Update`/`OnDestroy` maps onto component attach/update/detach.
- **Built-in `SpriteComponent`, `AnimationComponent`, `RigidbodyComponent`, `ColliderComponent` all implement `ISerializableComponent`** — relevant to the Ball save/load decision (Sprint 4).
- **Prefab/scene loader instantiates by FQN** (`Activator.CreateInstance`) and wires `<Component>` children — so a component-only entity is just "concrete Entity + XML components."

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 1 | [Foundation & Text](Sprint_1_Foundation_And_Text.md) | 5 | ⬜ Not started | Introduce the shared concrete `GameEntity` + new `TextComponent`; migrate and delete `TextEntity`. Proves the end-to-end pattern. |
| 2 | [Character Entities](Sprint_2_Character_Entities.md) | 5 | ⬜ Not started | Migrate `AnimatedCharacterEntity`, `CharacterEntity`, `PlayerEntity` to components (`BounceTweenComponent`, `MoveByKeysComponent`, pause behavior); delete the 3 classes and drop inheritance. |
| 3 | [GUI Buttons & Camera](Sprint_3_GUI_And_Camera.md) | 5 | ⬜ Not started | New `SoundButtonComponent` + `VolumeButtonComponent`; migrate `CameraEntity` onto existing camera components; delete the 3 classes. |
| 4 | [Physics: Ball & WorldBorder](Sprint_4_Physics_Ball_And_Border.md) | 5 | ⬜ Not started | New `WorldBorderComponent` + `BallMovementComponent`; resolve Ball's save/load (generic component serialization vs. format-preserving); delete both classes. |

## Point Summary

- **Total:** 20 points across 4 sprints (~5 each).
- **Timeline estimate:** ~1 focused day per sprint → ~4–5 working days.
- **Ordering rationale:** Sprint 1 establishes the shared entity + one new drawable component and deletes a class end-to-end, de-risking the pattern before scaling. The riskiest work (Ball save/load) is isolated in the final sprint so earlier sprints stay low-risk.

## Workflow Phases

1. **Foundation** (Sprint 1): shared concrete entity + text rendering; prove delete-a-class end-to-end.
2. **Core migration** (Sprints 2–3): visual/animation, GUI, and camera entities → components.
3. **Physics** (Sprint 4): the save/load-sensitive Ball + WorldBorder.
4. **Quality gate** (each sprint): build clean, full suite green (expect 1174/0/3), all 7 scenes smoke-run PASS.

## Sprint Structure (sizing guide)

- 1 point = small (~2 h) · 2 points = medium (~4 h) · 5 points = large (~1 day).
- Every sprint ends with a validation task: build + full suite + `scripts/run-all-scenes.ps1` smoke-run.

## Conventions & Invariants

- **No behavior change.** Only relocation of logic into components, XML `Type=`/component updates, and class deletion. The 7-scene smoke run must pass before and after each sprint.
- **Namespaces mirror folders** — new components live in `CoreEssentials.Playground.Components`.
- **Tests + docs per change** (repo convention): each new component gets unit tests; affected behaviors are covered by the existing scene/serialization suites. Add/update a `docs/` Markdown page where a user-facing capability changes.
- **No issue/PR numbers in code or docs.**

## Confirmed Decisions

1. **Shared concrete entity: `GameEntity : Entity`.** A single, behavior-less concrete class in `CoreEssentials.Playground.Entities` that exists only so the XML serializer can instantiate a game object (base `Entity` is abstract). It carries no logic — every capability comes from attached components. This is the one class that remains after all 8 are deleted; it is intentionally just "a game object with components." No further entity subclasses are introduced.
2. **Ball save/load: Option (a) — generic component serialization.** `GameEntity` implements `ISaveableEntity`, and its `SaveState()/LoadState()` serialize the transform + tags + each attached `ISerializableComponent`'s state. This is the clean, future-proof path and matches the component-only goal. **Consequence:** the on-disk save format changes, so existing `GameStateSerialization` tests and any checked-in `*_Save.xml` fixtures must be updated in Sprint 4.

---
*Created: 2026-09-05 | Part of Playground Entity → Components Project*
