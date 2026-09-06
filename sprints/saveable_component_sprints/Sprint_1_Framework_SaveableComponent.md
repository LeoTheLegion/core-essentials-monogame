# Sprint 1 — Framework: Component-Based Saving ⚙️

**Points:** 5 | **Status:** ⬜ Not Started | **Goal:** Make saving a component capability: an entity is saveable iff it has an `ISaveableComponent`, the serializer records/uses the entity's prefab, and `ISaveableEntity` is retired. No playground changes in this sprint — `GameEntity` keeps working until Sprint 2 deletes it.

## Why This Shape

- **Detection moves from interface to component:** `GameStateSerializer` currently filters `e is ISaveableEntity`. The new rule: an entity is saveable when it has a component implementing `ISaveableComponent`. This is the core of the entity-component direction — no entity subclassing for saving, ever.
- **Prefab is required for saving:** saves already store `Type` (a code smell that couples saves to classes). They will now store `Prefab="BallPrefab"`; load instantiates from that prefab and hands the element to the save component. Entities created outside `Instantiate` have no prefab → save throws with a clear message (confirmed: hard error, not a fallback).
- **Backward compatibility is transitional only:** during this sprint both paths coexist in the serializer (an entity with an `ISaveableComponent` is handled by the new path; an `ISaveableEntity` without such a component still works via the old path) so the suite stays green. Sprint 2 removes the last real `ISaveableEntity` implementer (`GameEntity`); the obsolete marker + remaining test migrations finish the switch.

## Tasks

- [ ] T1 ⭐ Create `Serialization/ISaveableComponent.cs` — component-level save interface: `XElement SaveState(); void LoadState(XElement element);`. Docs: an entity is saveable iff it has a component implementing this; the component decides **exactly** what to save (typically transform + tags + the public properties of whichever sibling components matter).
- [ ] T2 ⭐ Add `Entity.PrefabName` (string?) — set by `EntitySystem.Instantiate` after instantiation, so every entity created from a registered prefab knows which one. No public setter abuse: the system assigns it internally.
- [ ] T3 ⭐ Rewrite `GameStateSerializer` save path — an entity is saveable iff it has an `ISaveableComponent`. The serializer calls the component's `SaveState()` (which returns the full `<Entity>` element with everything the owner needs to restore), then stamps `Prefab="..."` on it and appends `<Children>`. Throw `InvalidOperationException` when a saveable entity's `PrefabName` is null/unknown.
- [ ] T4 ⭐ Rewrite `GameStateSerializer` load path — resolve `Prefab`, require it to be registered (`KeyNotFoundException` otherwise), `system.Instantiate(prefab, position)`, set the saved Id, then call the save component's `LoadState(element)`. Keep the Type-by-reflection creation as a clearly-marked legacy fallback only for saves without a Prefab attribute (old files).
- [ ] T5 🔁 Mark `ISaveableEntity` `[Obsolete("Use ISaveableComponent — attach a save component instead of implementing this interface.")]`; migrate all framework test entities (`PhysicsSceneSerializationTests.TestEntity`, `GameStateSerializerTests.TestEntity`, `EntityDrivenSerializationTests` entities, the Ball-load repro tests) to the component path: plain `GameObjectEntity` + a tiny test save component.
- [ ] T6 ⭐ **Delete `ISerializableComponent`** — remove the interface file and its `SerializeToXml`/`DeserializeFromXml` implementations from `SpriteComponent`, `RigidbodyComponent`, `ColliderComponent`, and `AnimationComponent`. Saving is now solely the save component's job: it reads sibling components' public properties (`Color`, `LinearVelocity`, `Restitution`, …) explicitly. Update the three affected unit tests (sprite asset round-trip, animation round-trip, collider filter round-trip) to plain property assertions.
- [ ] T7 ⭐ Add parameterless ctor + settable `ShapeType` (with shape-appropriate validation in `CreateCollider`) to `ColliderComponent` so colliders are XML-declarable via the prefab loader's property reflection. Existing ctors unchanged.
- [ ] T8 🔒 New framework tests: saveable-component detection (entity with/without the component), Prefab attribute written + required (throws without), load instantiates from prefab and restores state, legacy Type-only save still loads, `ColliderComponent` parameterless ctor + XML-declared circle/rectangle.
- [ ] T9 🔒 Build clean + full suite green + smoke-run all 7 scenes PASS.

## Acceptance Criteria

- A plain `GameObjectEntity` with a save component attached is saved and loaded by the framework serializer — no entity subclass anywhere in the path.
- Saves carry `Prefab="..."`; loading recreates entities via `Instantiate`, so prefab-declared components (sprite/rigidbody/collider) come back automatically.
- `ISerializableComponent` no longer exists; no built-in component implements any serialization interface — a save component reads sibling state through public properties only.
- Saving an entity without a known prefab throws with an actionable message.
- `ISaveableEntity` is `[Obsolete]`; no non-test code implements it; all framework tests use the component path.
- Full suite green; all 7 scenes smoke-run PASS.

## Deliverables

| File | Action | Purpose |
|------|--------|---------|
| `CoreEssentials/src/.../Serialization/ISaveableComponent.cs` | Create | Component-level save interface |
| `CoreEssentials/src/.../EntitySystem.cs` | Modify | Record `PrefabName` at `Instantiate` |
| `CoreEssentials/src/.../Entity.cs` | Modify | Expose `PrefabName` |
| `CoreEssentials/src/.../Serialization/GameStateSerializer.cs` | Modify | Prefab-based detection, save shape, load path |
| `CoreEssentials/src/.../Serialization/ISaveableEntity.cs` | Modify | `[Obsolete]` + doc pointer to the component path |
| `CoreEssentials/src/.../Serialization/ISerializableComponent.cs` | Delete | Per-component serialization interface retired |
| `CoreEssentials/src/.../Components/BuiltIn/**` (Sprite/Rigidbody/Collider/Animation) | Modify | Remove `ISerializableComponent` + `SerializeToXml`/`DeserializeFromXml` |
| `CoreEssentials/src/.../Components/BuiltIn/Physics/ColliderComponent.cs` | Modify | Parameterless ctor + settable `ShapeType` |
| `CoreEssentials.Tests/**` (serialization tests) | Add/Modify | New behavior tests + migrate test entities |

## Notes & Risks

- **Both paths coexist for one sprint:** the serializer must handle `ISaveableComponent` entities and legacy `ISaveableEntity` entities in the same save file without ambiguity. Rule: component path wins if a save component is present; otherwise fall back to the entity interface (Sprint 2 removes the last implementer).
- **Prefab registration at load time:** loading a save requires its prefabs registered — true today for scenes (prefabs register before entity spawn) but tests must register explicitly. The error message should say exactly that.
- **`Entity.PrefabName` and overrides:** `Instantiate` with per-instance overrides still records the *registered* prefab name — correct, since the save only needs to recreate the base shape; component state carries the rest.
- **ColliderComponent ShapeType setter:** changing shape on an already-created collider must not silently leak the old fixture — document that `ShapeType` is a creation-time property (set before attach / before the body exists).

---
*Created: 2026-09-06 | Part of Component-Based Save System Project*
