# Sprint 19 — Collision Groups 💥

**Points:** 5.5  
**Status:** ✅ **Complete** (see [Design Pivot](#-design-pivot-2026-08-15))  
**Sprint Goal:** Expose collision filtering (categories + mask) through the engine-agnostic physics adapter, settable declaratively via XML.

**Dependencies:** Sprint 13 (GameStateSerialization)

**Existing Physics API (from Sprint 13):**
- `RigidbodyComponent` manages `IPhysicsBody` with component-level properties: `Position`, `Rotation`, `LinearVelocity`, `AngularVelocity`
- `ColliderComponent` manages colliders attached to physics bodies
- Both components are serialized/deserialized via entity-driven approach
- Physics engine accessed via `EntitySystem.GetGameSystem<PhysicsEngine>()`

---

## ⚠️ Design Pivot (2026-08-15)

**Last updated: 2026-08-15** · Branch: `feature/collision-groups-sprint19` (from `origin/development`)

### Why the pivot
The original spec called for a **string-based** collision group + collision matrix (`CollisionGroup`, `CollisionMatrix`, `SetCollisionEnabled("player","enemy",false)`). While implementing it, we discovered that **Aether.Physics2D already provides native collision filtering** on its `Fixture`:

- `Fixture.CollisionCategories` — `[Flags] Category` (which categories this fixture belongs to). Default `Cat1`.
- `Fixture.CollidesWith` — `[Flags] Category` (which categories this fixture accepts). Default `Category.All`.
- `Fixture.CollisionGroup` — `short` (positive = always collide, negative = never collide; wins over the mask bits).
- `ContactManager.ShouldCollide` applies the group rule first, then the two-way mask rule.

Building our own string matrix would **duplicate** functionality the engine already has, and would force us to manually suppress contacts after the fact. The correct design is to **expose Aether's native filtering through the adapter**.

### The core constraint: this is an ADAPTER
The physics layer is an **adapter** — `IPhysicsBody` / `ICollider` / `IPhysicsWorld` hide Aether so the engine can be swapped later. Therefore:

- We must **NOT** leak Aether types (`nkast.Aether.Physics2D.Category`, `Fixture`) into the public API.
- We expose filtering through **engine-agnostic** types: a `CollisionCategory` `[Flags]` enum in `Physics.Types` that mirrors Aether's `Category` bit-for-bit, and new members on `ICollider`.
- The Aether `Collider` **forwards** to `Fixture.CollisionCategories` / `CollidesWith` (casting `CollisionCategory → Category`).

### Scope decision: drop the `short` group
Aether's `Fixture.CollisionGroup` (`short`) expresses "always collide" (`>0`) / "never collide" (`<0`) and overrides the mask bits. **We are intentionally NOT exposing this in the public API.** The two-bitmask approach (`Categories` + `CollidesWith`) covers the general use case and keeps the surface small and engine-portable. If a future engine lacks the `short` group, the adapter still works.

> **Note for the adapter:** the Aether `Collider` will simply *ignore* `CollisionGroup` (leave it at the engine default `0`). It is deliberately absent from `ICollider` / `ColliderComponent` / the XML surface.

### What happens to the string-based work
The following were built under the old approach and are **being removed** as part of the pivot:
- `Collision/CollisionGroup.cs`, `Collision/CollisionMatrix.cs`, `Collision/CollidingEntityPair.cs`
- `EntitySystem` string-based group methods (`CreateCollisionGroup`, `AddToCollisionGroup`, `GetCollidingEntities(groupA, groupB)`, etc.)
- `Entity.AddToCollisionGroup(string)` / `Entity.RemoveFromCollisionGroup(string)`
- `CollisionGroupTests.cs` and the temporary `CollisionDiagTests.cs`

**Kept:** the engine-agnostic active-contact tracking in `PhysicsEngine` (`GetActiveContacts()`). It lives in the physics system and is the physics-level query surface.

### Scope refinement (2026-08-15)
The feature is **scoped to the physics system**. An earlier iteration added an `EntitySystem.GetCollidingEntities(Entity)` convenience query, but that reached into the entity system for what is fundamentally a physics concern — and it was exactly what pulled in the flaky live-contact tests. **That `EntitySystem` query has been removed.** `EntitySystem` is untouched by this sprint. The physics-level query remains available as `PhysicsEngine.GetActiveContacts()`. The entity-facing surface is the `ColliderComponent` (which is a component, not the entity system) and the XML.

---

## New Plan

- **P1: Remove string-based system** — delete the files/methods listed above.
- **P2: Engine-agnostic filter types (`Physics.Types`)**
  - `CollisionCategory` — `[Flags]` enum mirroring Aether's `Category` (`Cat1`…`Cat31`, `All`, `None`).
  - `ICollider` gains:
    - `CollisionCategory Categories { get; set; }` (default `Cat1`)
    - `CollisionCategory CollidesWith { get; set; }` (default `All`)
  - (No `CollisionGroup` `short` — see scope decision.)
- **P3: Aether adapter** — `Collider` forwards `Categories`/`CollidesWith` to `Fixture.CollisionCategories`/`CollidesWith`. Setting them triggers Aether's `Refilter()` automatically.
- **P4: `ColliderComponent` exposes + serializes** — `Categories`/`CollidesWith` properties proxy to the internal `ICollider`; serialized in `SerializeToXml`/`DeserializeFromXml`.
- **P5: XML surface (like tags)** — no new machinery: `EntitySerializer` already parses `<Properties>` via reflection and `SerializationUtils.ParseValue` already handles **flags enums** (`"Cat1, Cat2"`).
  ```xml
  <EntityDefinition Type="PlayerEntity" Id="player">
    <Tags><Tag Name="Player" /></Tags>
    <Components>
      <Component Type="RigidbodyComponent">
        <Properties><Property Name="IsKinematic" Value="True" /></Properties>
      </Component>
      <Component Type="ColliderComponent">
        <Properties>
          <Property Name="Categories"   Value="Cat1" />
          <Property Name="CollidesWith" Value="Cat1, Cat2" />
        </Properties>
      </Component>
    </Components>
  </EntityDefinition>
  ```
- **P6: Tests** — `CollisionFilteringTests.cs`: deterministic tests only (enum bits, adapter forwards to `Fixture`, component exposure, XML round-trip). Live-contact detection is intentionally NOT asserted here (see [Notes](#notes--risks)); the existing `CollisionEventsTests` already covers it.
- **P7: Docs** — `docs/CollisionGroups.md` (usage, XML, category/mask semantics, example) + update this sprint doc.
- **P8: Build + full test suite** — 0 errors, 0 warnings.

---

## Tasks

- [x] **P1: Remove string-based system (1 pt)**
  - Delete `CollisionGroup.cs`, `CollisionMatrix.cs`, `CollidingEntityPair.cs`
  - Remove string-based `EntitySystem` methods + `Entity` convenience methods
  - Remove `CollisionGroupTests.cs`, `CollisionDiagTests.cs`

- [x] **P2: Engine-agnostic filter types (1 pt)** ⭐ User-facing
  - `CollisionCategory` `[Flags]` enum in `Physics.Types`
  - `ICollider.Categories` / `ICollider.CollidesWith`

- [x] **P3: Aether adapter implements filtering (1 pt)**
  - `Collider` forwards to `Fixture.CollisionCategories` / `CollidesWith`

- [x] **P4: `ColliderComponent` + XML (0.5 pt)** ⭐ User-facing
  - Expose `Categories`/`CollidesWith` on the component
  - Serialize/deserialize in XML

- [x] **P5: Write unit tests (0.5 pt)** 🔁 Validation
  - `CollisionFilteringTests.cs`: enum bits, adapter forwarding, component exposure, XML round-trip (7 tests, all passing)

- [x] **P6: Create user documentation (0.5 pt)** 📚 User-facing
  - Create `docs/CollisionGroups.md`
  - Document categories/mask, XML usage, examples

---

## Acceptance Criteria

- [x] Collision filtering is exposed through engine-agnostic `ICollider` (no Aether leak)
- [x] `ColliderComponent` exposes `Categories`/`CollidesWith`
- [x] Filter is settable declaratively via XML (like tags)
- [x] Feature is scoped to the physics system — `EntitySystem` is untouched
- [x] Project builds cleanly — **0 errors, 0 warnings**
- [x] All existing tests pass (776) + 7 new collision-filtering tests added

---

## Deliverables

| File | Type | Visibility | Notes |
|------|------|------------|-------|
| `Physics/Types/CollisionCategory.cs` | New | ⭐ PUBLIC | `[Flags]` category enum (engine-agnostic) |
| `Physics/Types/IFixture.cs` (`ICollider`) | Modified | ⭐ PUBLIC | Add `Categories`/`CollidesWith` |
| `Physics/Engines/Aether/Collider.cs` | Modified | 🔒 Internal | Forward to Aether `Fixture` |
| `.../Components/BuiltIn/ColliderComponent.cs` | Modified | ⭐ PUBLIC | Expose + serialize filter |
| `CollisionFilteringTests.cs` | New | 🔒 Internal | Unit tests for collision filtering |
| `docs/CollisionGroups.md` | New | ⭐ PUBLIC | User guide for collision filtering |

---

## Notes & Risks

- **Adapter purity** — the single most important constraint: no Aether types in the public API. `CollisionCategory` must mirror Aether's `Category` bit-for-bit so the cast in the adapter is safe.
- **Dropped `short` group** — deliberately not exposed (see [Scope decision](#scope-decision-drop-the-short-group)). The Aether adapter leaves `Fixture.CollisionGroup` at its default `0`.
- **XML parsing** — relies on the existing reflection `<Properties>` path + `SerializationUtils.ParseValue` flags-enum support. Verified with a round-trip test.
- **Contact-detection flakiness (realized)** — Aether's broad-phase contact registration is **order/state-sensitive** in this environment. A verbatim copy of the passing `CollisionEventsTests` setup fails when run as a *new* test (even in an isolated scratch file), while the original `CollisionEventsTests` passes 5/5. This is a pre-existing engine/environment quirk, **not** caused by this sprint's code. **Decision:** the new tests assert the *filter configuration* (adapter forwarding + XML round-trip) rather than a live contact; live contact detection remains covered by the existing `CollisionEventsTests`. The flaky positive-contact tests were skipped per developer direction.

---

*Created: 2026-08-07 | Pivoted: 2026-08-15 | Part of Entity System Enhancements Project*
