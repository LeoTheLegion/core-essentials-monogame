# Component-Based Save System — Scrum Sprints 🚀

## Why This?

Sprint 4 of the playground entity→component migration introduced `GameEntity : GameObjectEntity, ISaveableEntity` — a hand-written entity subclass whose only job was generic save/load plumbing. That fights the grain: **saving should be a component capability, not an entity inheritance concern.** If someone wants to save an entity, they attach a save *component* that implements a small framework interface and saves whatever it needs to save.

This project makes saving component-based and prefab-first:

- An entity is saveable **iff it has a component implementing `ISaveableComponent`** — no entity subclassing, no per-entity code in the framework.
- **A prefab is required for saving.** The serializer records the entity's prefab name at save time; loading instantiates from that prefab and then hands the saved element to the save component. Entities created outside `Instantiate` (no known prefab) fail fast with a clear error.
- `ISaveableEntity` is retired (`[Obsolete]`) — the OOP save path gives way to the entity-component framework.

## Project Structure

```
Before:                                        After:
CoreEssentials/.../Serialization/              CoreEssentials/.../Serialization/
  ISerializableComponent.cs    (per-component)   ISerializableComponent.cs    (DELETED)
  ISaveableEntity.cs           (entity-level)    ISaveableComponent.cs        (NEW — component-level)
  GameStateSerializer.cs       (ISaveableEntity) GameStateSerializer.cs       (prefab-based save/load)
CoreEssentials.Playground/Entities/
  GameEntity.cs             (save shim)          (empty — deleted)
CoreEssentials.Playground/Components/
  BallMovementComponent.cs                             + BallSaveComponent.cs (NEW)
```

## Framework Facts That Make This Possible

- **`SpriteComponent.OnAttach` self-loads from its `SpriteAsset` string** — instantiating from the prefab hydrates sprites for free; no code needs to load assets on the save/load path.
- **Built-in components expose their runtime state as plain public properties** (`SpriteComponent.Color`, `RigidbodyComponent.LinearVelocity`, `ColliderComponent.Restitution`, …). A single save component reads exactly what it needs from these — there is **no per-component serialization interface**. `ISerializableComponent` (and its `SerializeToXml`/`DeserializeFromXml` on the built-ins) is deleted.
- **The prefab loader sets any writable public property from XML** (`SerializationUtils.ParseValue` handles enums, Vector2, float) — once `ColliderComponent` gets a parameterless ctor + settable `ShapeType`, colliders become fully XML-declarable too.
- **`EntitySystem.Instantiate` is the single choke point** for prefab instantiation — one assignment records the prefab name on every instantiated entity.
- **`GameStateSerializer` is the single choke point** for save/load — detection, save shape, and creation all live there.

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 1 | [Framework: Component-Based Saving](Sprint_1_Framework_SaveableComponent.md) | 5 | ✅ Done | New `ISaveableComponent` interface; `Entity.PrefabName` recorded at `Instantiate`; prefab-based `GameStateSerializer` (save writes `Prefab=`, load instantiates from prefab, throws without one); `[Obsolete] ISaveableEntity` + test migration; `ColliderComponent` parameterless ctor + settable `ShapeType`. |
| 2 | [Playground: Ball Save Component](Sprint_2_Playground_BallSaveComponent.md) | 5 | ✅ Done | New playground `BallSaveComponent` (generic transform + tags + per-component serialization); ball prefab back to plain `GameObjectEntity` with an XML-declared collider; **delete `GameEntity.cs`**; tests, docs, smoke-run. |

## Point Summary

- **Total:** 10 points across 2 sprints (~5 each) — both complete.
- **Timeline estimate:** ~1 focused day per sprint → ~2 working days.
- **Ordering rationale:** the framework core (Sprint 1) is independently shippable and keeps the suite green with the old save path still exercised by migrated tests; Sprint 2 then deletes the `GameEntity` shim it replaces, so no sprint ends in a broken intermediate state.

## Workflow Phases

1. **Framework** (Sprint 1): interface + prefab tracking + serializer + obsolete marker + collider ctor.
2. **Migration** (Sprint 2): playground save component + template cleanup + delete the shim.
3. **Quality gate** (each sprint): build clean, full suite green, all 7 scenes smoke-run PASS.

## Key Decisions (confirmed with developer)

- **Component-only saving:** `ISaveableEntity` is deprecated and fully switched away from — this is part of the broader move to an entity-component framework.
- **Prefab name captured by the framework** at `Instantiate` time (not repeated as a property in every prefab).
- **Missing prefab = hard error:** saving throws if a saveable entity has no known prefab — a prefab is strictly required for saving.
- **Vocabulary:** "prefab" everywhere (the codebase already standardized on `RegisterPrefab`/`HasPrefab`) — not "template".
- **Single save component, no per-component interface:** `ISerializableComponent` is deleted. A code owner writes one save component (`ISaveableComponent`) that reads the public properties of whatever sibling components it cares about and saves exactly that — nothing more, nothing less.
