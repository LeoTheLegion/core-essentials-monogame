# Sprint 4 — Physics: Ball & WorldBorder ⚙️

**Points:** 5 | **Status:** ⬜ Not Started | **Goal:** Migrate `WorldBorder` and `Ball` onto components, delete both classes, and resolve Ball's save/load (the riskiest part of the project). This is the last sprint — after it, `Entities/` contains only the shared concrete `GameEntity`.

## Why Last & Why Risky

`WorldBorder` is straightforward (build 4 static physics bodies from `Size` + `PhysicsConfig`). **`Ball` is the hard one:** it implements `ISaveableEntity` with ~150 lines of custom `SaveState()`/`LoadState()` that serialize transform, tags, physics velocity, and sprite color. The framework's `GameStateSerializer` only saves entities implementing `ISaveableEntity` by calling *their* `SaveState()/LoadState()` — there is **no automatic per-component serialization**. The confirmed approach (see README) is **Option (a): generic component serialization** — `GameEntity` implements `ISaveableEntity` and serializes transform + tags + each attached `ISerializableComponent`.

## Target Outcome

- New `WorldBorderComponent : EntityComponent` — builds the 4 static borders from `Size` + the engine's `PhysicsConfig` in `OnAttach`.
- New `BallMovementComponent : EntityComponent` — the random-movement + spin coroutine (moved out of `Ball`).
- Ball declared as `GameEntity` + built-in `SpriteComponent`/`RigidbodyComponent`/`ColliderComponent` + `BallMovementComponent`, with save/load via generic component serialization.
- `Entities/Ball.cs` and `WorldBorder.cs` deleted.

## Save/Load Approach (CONFIRMED: Option a — generic component serialization)

`GameEntity` implements `ISaveableEntity`. Its `SaveState()` writes the entity transform (position/rotation/scale/sort/active), tags, and the serialized state of every attached `ISerializableComponent`; `LoadState()` restores them. This is clean and future-proof — any component that implements `ISerializableComponent` round-trips for free.

**Consequence:** the on-disk save format changes from Ball's bespoke shape to the generic per-component shape. Existing `GameStateSerialization` tests and any checked-in `*_Save.xml` fixtures must be updated to the new format as part of this sprint.

## Tasks

- [ ] T1 ⭐ Create `WorldBorderComponent : EntityComponent` — move `WorldBorder.CreateWorldBorder()` into `OnAttach` (reads `Size`, resolves the `"Player|Vip"` collision mask from `PhysicsConfig`). Expose `Size` as a property.
- [ ] T2 🔁 Update `PhysicsSpawnComponent.CreateWorldBorderEntity(...)` to create a `GameEntity` with a `WorldBorderComponent` (instead of `CreateEntity<WorldBorder>`). Keep the existing virtual seam so tests can still override it.
- [ ] T3 ⭐ Create `BallMovementComponent : EntityComponent` — move `RandomMovementCoroutine()` here (random impulse + angular spin on a `WaitForSeconds` loop); start in `OnAttach`, stop in `OnDetach`.
- [ ] T4 🔁 Update `Content/Templates/BallTemplate.xml` → `GameEntity` root, ensure the built-in sprite/rigidbody/collider components are declared (they already are) and add `<Component Type="BallMovementComponent">`.
- [ ] T5 ⭐ Make `GameEntity : ISaveableEntity` with generic serialization — `SaveState()` writes transform + tags + each attached `ISerializableComponent`'s state; `LoadState()` restores them. Verify `RigidbodyComponent`'s `ISerializableComponent` surface round-trips linear/angular velocity (extend it if not); verify `SpriteComponent` round-trips color.
- [ ] T6 🔁 Delete `Entities/Ball.cs` and `WorldBorder.cs`; remove references/imports.
- [ ] T7 🔒 Add unit tests: `WorldBorderComponent` (creates 4 static bodies with the resolved mask), `BallMovementComponent` (applies impulses over time; stops on detach), and a Ball save/load round-trip test under generic component serialization (position + physics velocity + sprite color survive). Update `GameStateSerialization` tests + any `*_Save.xml` fixtures to the new format.
- [ ] T8 🔒 Build clean + full suite green (expect 1174/0/3) + smoke-run all 7 scenes PASS.

## Acceptance Criteria

- No code references `Ball` or `WorldBorder`; both classes are deleted.
- A ball declared purely from XML spawns with sprite/rigidbody/collider/movement and **its save/load round-trip works** under the confirmed approach (physics velocity + sprite color + transform preserved).
- World borders still contain both regular and VIP balls (collision mask intact).
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `Components/WorldBorderComponent.cs` | Create | Builds the 4 static physics borders |
| `Components/BallMovementComponent.cs` | Create | Random-movement + spin coroutine |
| `Entities/GameEntity.cs` | Modify | Implement `ISaveableEntity` with generic transform + tags + per-component serialization |
| `Content/Templates/BallTemplate.xml` | Modify | Repoint + declare `BallMovementComponent` |
| `Components/PhysicsSpawnComponent.cs` | Modify | Create border entity as `GameEntity` + `WorldBorderComponent` |
| `Entities/{Ball,WorldBorder}.cs` | Delete | Behavior now in components |
| `CoreEssentials.Tests/**` (GameStateSerialization + fixtures) | Add/Modify | Round-trip tests + update fixtures to the generic per-component save format |

## Notes & Risks

- **Save format change (option a):** this is the one user-visible-ish change in the whole project. Any checked-in `*_Save.xml` (e.g. `PhysicsScene_Save.xml`) and the `GameStateSerialization` test suite encode the old shape — update them deliberately and re-run the physics save/load tests specifically.
- **Ball's "add component only if not present" guards:** the current `OnStart` re-hydrates components when a ball is deserialized (sprite may be null after load). With data-driven Ball, the prefab declares the components up front — confirm the load path doesn't double-add or lose the sprite.
- **Physics velocity restore:** `Ball.LoadState` sets linear/angular velocity on the `RigidbodyComponent`. Under option (a), ensure the generic serializer round-trips that via `RigidbodyComponent`'s `ISerializableComponent` surface (verify it actually serializes velocity — if not, extend it).
- **WorldBorder mask:** keep resolving `"Player|Vip"` from `PhysicsConfig` exactly as today; a regression here silently lets balls escape the arena (only visible in the PhysicsEntityScene smoke run with debug overlay).

---
*Created: 2026-09-05 | Part of Playground Entity → Components Project*
