# Sprint 2 — Playground: Ball Save Component 🎱

**Points:** 5 | **Status:** ✅ Complete | **Goal:** Replace the `GameEntity` save shim with a playground-owned `BallSaveComponent`, return the ball prefab to plain `GameObjectEntity`, and delete `Entities/GameEntity.cs`. After this sprint, `CoreEssentials.Playground/Entities/` is empty — every declaration is data + components.

## Target Outcome

- New `Components/BallSaveComponent.cs : EntityComponent, ISaveableComponent` — the playground owns its own saving: it explicitly serializes the owner's transform + tags + the public properties of the sibling components it cares about (sprite `Color`/`SpriteAsset`, rigidbody velocity/mass, collider state). This is the generic logic from `GameEntity`, moved onto a component operating on `Owner` — but with no per-component serialization interface; each value is read by name.
- `Content/Templates/BallTemplate.xml` — root back to plain `GameObjectEntity`; declares `SpriteComponent` (with `SpriteAsset`), `RigidbodyComponent`, **`ColliderComponent` fully in XML** (now possible after Sprint 1's parameterless ctor), `BallMovementComponent`, and the new `BallSaveComponent`.
- `Entities/GameEntity.cs` — **deleted**. The playground's `Entities/` folder is empty; if it becomes an unused directory, remove it.
- Random display scale + any remaining OnStart hydration that had no XML home moves into a component (working assumption: `BallMovementComponent.OnAttach` — create collider if absent is no longer needed once the prefab declares it; random scale rolls only when scale is still `One`, so VIP balls configured by the spawner afterward keep their explicit scale, and loaded saves restore scale after instantiation).

## Tasks

- [x] T1 ⭐ Create `Components/BallSaveComponent.cs` — implement `ISaveableComponent`: `SaveState()` writes transform + tags + explicit child elements for sprite color/asset, rigidbody velocity/mass, and collider state (read from each sibling component's public properties); `LoadState(element)` restores them (including creating the rigidbody body at the restored position before velocity is applied).
- [x] T2 🔁 Update `Content/Templates/BallTemplate.xml` → `GameObjectEntity` root; add XML-declared `ColliderComponent` (ShapeType=Circle, Restitution=1, Offset=0,1), `SpriteAsset`, and `<Component Type="BallSaveComponent" />`.
- [x] T3 🔁 Move the random-scale logic out of `GameEntity.OnStart` into a component (`BallMovementComponent.OnAttach`, guarded so explicit/VIP/loaded scales win); delete `Entities/GameEntity.cs` (the now-empty `Entities/` folder is removed).
- [x] T4 🔒 Update tests: `Sprint5cPhysicsDataSceneTests` (balls are plain `GameObjectEntity` with a `BallSaveComponent`), `Sprint4PhysicsComponentTests` round-trip test (now via the component + prefab instantiation, including the "no prefab → throws" case for hand-created entities). Also removed the now-dead `using CoreEssentials.Playground.Entities;` from 4 other test files.
- [x] T5 🔒 Docs: rewrite `docs/GameStateSerialization.md` around the component path (delete the bespoke-Ball example), update `docs/PhysicsBallAndWorldBorderComponents.md` (GameEntity section → BallSaveComponent). No `docs/README.md` index change needed.
- [x] T6 🔒 Build clean + full suite green (1217/0/3) + smoke-run all 7 scenes PASS (PhysicsEntityScene included).

- [x] **Extra (required framework fix):** `RigidbodyComponent` only had `RigidbodyComponent(RigidbodyType type = Dynamic)` — an *optional-arg* ctor is not a true parameterless ctor, so the prefab loader's `Activator.CreateInstance(type)` silently failed to create XML-declared rigidbodies (surfaced as "Entity must have a RigidbodyComponent before adding a ColliderComponent"). Added a true parameterless constructor (`RigidbodyComponent() : this(RigidbodyType.Dynamic)`) and made the typed ctor non-optional. Required for any plain-entity prefab that declares a `RigidbodyComponent` in XML.

## Acceptance Criteria

- No code references `GameEntity`; the class and file are deleted; `Entities/` is empty or removed.
- The ball is declared purely from XML (`GameObjectEntity` + components) and its save/load round-trip works through the framework serializer (position, velocity, sprite color preserved).
- A hand-created entity with a save component but no prefab fails fast on save with an actionable error.
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `CoreEssentials.Playground/Components/BallSaveComponent.cs` | Create | Playground-owned save component implementing `ISaveableComponent` |
| `CoreEssentials.Playground/Content/Templates/BallTemplate.xml` | Modify | Plain `GameObjectEntity` + XML collider + save component |
| `CoreEssentials.Playground/Entities/GameEntity.cs` | Delete | Save shim replaced by the component |
| `CoreEssentials.Playground/Components/BallMovementComponent.cs` | Modify | Hosts the random-scale logic (if confirmed) |
| `CoreEssentials.Tests/**` | Modify | Round-trip + prefab-required tests, Sprint 5c assertions |
| `docs/*.md` | Modify | Component-based save documentation |

## Notes & Risks

- **Load ordering:** the serializer instantiates from the prefab (OnStart/OnAttach all run), sets the saved Id, then calls `BallSaveComponent.LoadState`. The rigidbody body must exist before velocity is restored — the component handles this exactly as `GameEntity.LoadState` did today.
- **Random scale vs VIP balls:** `ConfigureVipBall` runs *after* instantiation, so an OnAttach-time random roll can't clobber a VIP scale; loaded saves restore scale after instantiation. Keep the "only if still One" guard.
- **Save format is unchanged from Sprint 4's generic shape** — only the `Type` attribute becomes redundant (kept for legacy readability) and `Prefab` is authoritative. The child element names (`SpriteState`, `RigidbodyState`, `ColliderState`) are now produced by `BallSaveComponent` directly, not by per-component serialization.

---
*Created: 2026-09-06 | Part of Component-Based Save System Project*
