# Collision Groups & Filtering 💥

Collision filtering lets you control **which objects can collide with which others** without changing their shapes or positions. CoreEssentials exposes this through the engine-agnostic physics API, so the underlying engine (currently [Aether.Physics2D](https://github.com/nkast/Aether.Physics2D)) can be swapped without changing your code.

You configure filtering with two things:

- **`Categories`** — the "team(s)" a collider belongs to (a set of bits).
- **`CollidesWith`** — a *mask* of the categories it is willing to collide with.

Two colliders only collide when **each one's mask admits the other's category**.

---

## How It Works

Every collider has a `Categories` value (which category bits it carries) and a `CollidesWith` mask (which category bits it accepts). The engine computes, for a pair `A`/`B`:

```
collide = (A.CollidesWith & B.Categories) != 0  &&  (B.CollidesWith & A.Categories) != 0
```

So the relationship is **symmetric** — both sides must agree.

| A `Categories` | A `CollidesWith` | B `Categories` | B `CollidesWith` | Collide? |
|----------------|------------------|----------------|------------------|----------|
| `Cat1`         | `All`            | `Cat1`         | `All`            | ✅ Yes   |
| `Cat1`         | `Cat1`           | `Cat2`         | `Cat2`           | ❌ No    |
| `Cat1`         | `Cat2`           | `Cat2`         | `Cat1`           | ✅ Yes   |
| `Cat1, Cat2`   | `All`            | `Cat2`         | `Cat1`           | ✅ Yes   |

> **Defaults:** a new collider is `Categories = Cat1` and `CollidesWith = All`, so by default everything collides with everything.

---

## The `CollisionCategory` Flags

`CollisionCategory` is a `[Flags]` enum with 31 bits (`Cat1`…`Cat31`) plus `None` and `All`. Combine bits with `|`:

```csharp
using CoreEssentials.GameSystems.Physics.Types;

CollisionCategory.Cat1
CollisionCategory.Cat1 | CollisionCategory.Cat2   // belongs to two categories
CollisionCategory.All                              // accepts every category
```

---

## Usage

### 1. Via `ColliderComponent` (recommended)

`ColliderComponent` exposes `Categories` and `CollidesWith`. Set them before (or after) the collider is created — they are applied to the underlying collider automatically.

```csharp
var entity = entitySystem.CreateEntity<MyEntity>();
entity.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));

var collider = new ColliderComponent(radius: 1f);
collider.Categories   = CollisionCategory.Cat1;          // I am a "player"
collider.CollidesWith = CollisionCategory.Cat1 | CollisionCategory.Cat2; // collide with players & walls
entity.AddComponent(collider);
```

### 2. Via the `ICollider` directly (advanced)

For code that works at the physics layer, set the properties on the `ICollider`:

```csharp
ICollider collider = body.CreateCircleCollider(radius: 1f);
collider.Categories   = CollisionCategory.Cat1;
collider.CollidesWith = CollisionCategory.All;
```

Setting either property re-filters existing contacts automatically.

---

## Setting the Filter via XML (like tags)

The filter is declarable in entity XML, using the same `<Properties>` reflection path that powers component configuration and entity tags. No extra schema is needed — `Categories` and `CollidesWith` are parsed as flags enums.

```xml
<Entity>
  <Position X="0" Y="0" />
  <Components>
    <Component Type="RigidbodyComponent">
      <Properties>
        <Property Name="Type" Value="Dynamic" />
      </Properties>
    </Component>
    <Component Type="ColliderComponent">
      <Properties>
        <Property Name="Radius" Value="1" />
        <Property Name="Categories"   Value="Cat1" />
        <Property Name="CollidesWith" Value="Cat1, Cat2" />
      </Properties>
    </Component>
  </Components>
</Entity>
```

- `Value="Cat1"` — a single category.
- `Value="Cat1, Cat2"` — a combination of categories (flags).
- `Value="All"` — accept every category.

`ColliderComponent` also round-trips the filter through its own `SerializeToXml` / `DeserializeFromXml` (the `<ColliderState>` element carries `Categories` and `CollidesWith` attributes).

---

## Example: Player vs. Walls vs. Pickups

Say you want the **player** to collide with **walls** and **pickups**, but **pickups** to collide with nothing else, and **walls** to collide with each other and the player.

| Object   | `Categories` | `CollidesWith`            |
|----------|--------------|---------------------------|
| Player   | `Cat1`       | `Cat1, Cat2, Cat3`        |
| Wall     | `Cat1`       | `Cat1, Cat2`              |
| Pickup   | `Cat3`       | `Cat1`                    |

- Player (`Cat1`) vs Wall (`Cat1`): player accepts `Cat1` ✅ and wall accepts `Cat1` ✅ → **collide**.
- Player (`Cat1`) vs Pickup (`Cat3`): player accepts `Cat3` ✅ and pickup accepts `Cat1` ✅ → **collide**.
- Pickup (`Cat3`) vs Wall (`Cat1`): pickup accepts `Cat1` ✅ but wall does **not** accept `Cat3` ❌ → **no collide**.

---

## Notes

- **Engine-agnostic:** `CollisionCategory` mirrors the underlying engine's category bits one-for-one, so the adapter can cast directly. No Aether types leak into the public API.
- **No "always/never" group:** the underlying engine also supports a signed group value that forces "always collide" / "never collide". CoreEssentials intentionally does **not** expose it — the two-bitmask approach covers the general case and keeps the API portable across engines.
- **Live contact detection:** filtering decides *whether* two colliders *can* collide. Actual contact detection (and the `OnCollision` / `OnSeparation` events) is handled by the engine and covered by the physics event system — see [Physics System](PhysicsSystem.md).

---

*Part of the Entity System Enhancements Project · Sprint 19*
