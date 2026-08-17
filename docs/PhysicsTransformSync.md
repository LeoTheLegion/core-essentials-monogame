# Physics Transform Sync

`RigidbodyComponent` keeps an entity's `Position`/`Rotation` in sync with the physics body it manages. This page explains the two sync modes, and the **divergence detection** that lets a physics-driven entity adopt an externally set transform (save/load, teleport, debug) as the new source of truth.

---

## Sync Modes

The `SyncFromPhysics` flag decides which side drives the other:

| Mode | `SyncFromPhysics` | Direction | Typical use |
|------|-------------------|-----------|-------------|
| Physics-driven | `true` (default for `Dynamic`) | body → entity | Balls, projectiles, anything physics should move |
| Entity-driven | `false` | entity → body | Kinematic/user-controlled bodies you move by hand |

- **Physics-driven**: each frame the component copies the body's transform onto the entity, so the entity follows the simulation.
- **Entity-driven**: each frame the component copies the entity's transform onto the body, so your code moves the body.

---

## Divergence Detection (Physics-Driven Bodies)

For a physics-driven body the entity transform is normally *written by the component itself* (body → entity). The only other code that writes it is you — for example when restoring a save, teleporting, or nudging a body in a debugger.

`RigidbodyComponent` remembers the transform it last wrote to the entity. On the next `Update`:

```
if entity transform ≠ last transform I wrote   (beyond a small epsilon)
    → external code moved the entity
    → physics ADOPTS the entity transform:  body = entity
else
    → normal physics flow:  entity = body
```

This makes the entity transform the source of truth whenever *something else* changes it, without you having to call any sync method.

### Why this matters for save/load

Before this change, restoring a save set the **entity** position but left the **physics body** at its live position. On the next frame the component copied the stale body position back onto the entity, wiping out the restored state. Now the component detects the entity move and re-anchors the body, so the next physics step integrates from the saved position.

---

## Usage

### Save/load (no extra code needed)

In your `ISaveableEntity.LoadState`, just restore the entity transform. The component picks it up automatically:

```csharp
public void LoadState(XElement element)
{
    // Restore the entity transform from the save file.
    var pos = element.Element("Position");
    if (pos != null)
        Position = new Vector2(
            float.Parse(pos.Attribute("X")!.Value),
            float.Parse(pos.Attribute("Y")!.Value));

    // Restore velocity via the component (optional).
    _rigidbody?.SetLinearVelocity(new Vector2(1f, 0f));
    // No SyncBodyFromEntity() call required — the component detects the
    // transform change and re-anchors the physics body on the next Update.
}
```

### Teleporting a physics-driven entity

```csharp
ball.Position = new Vector2(500, 300);
// On the next Update, RigidbodyComponent adopts this as the body position.
```

### Forcing a sync immediately

If you need the body to match the entity *right now* (not on the next `Update`), call the explicit helper:

```csharp
rigidbody.SyncBodyFromEntity();
```

---

## Parameters

| Member | Kind | Description |
|--------|------|-------------|
| `SyncFromPhysics` | `bool` property | When `true`, physics drives the entity (with divergence detection). When `false`, the entity drives the body. Defaults to `true` for `Dynamic` bodies. |
| `SyncBodyFromEntity()` | method | Immediately copies the entity `Position`/`Rotation` onto the physics body. |
| `SetLinearVelocity(Vector2)` | method | Sets the body's linear velocity directly (bypasses the solver). |
| `AngularVelocity` | `float` property | Gets/sets the body's angular velocity (rad/s). |

> **Note:** Divergence detection uses a small epsilon (`0.0001`). Any external write to the entity transform on a physics-driven entity is treated as a deliberate move and re-anchors the body.

---

## Example

A ball that can be teleported and survives save/load with no explicit sync calls:

```csharp
public class Ball : Entity, ISaveableEntity
{
    private RigidbodyComponent? _rigidbody;

    public override void OnStart()
    {
        _rigidbody = new RigidbodyComponent(RigidbodyType.Dynamic);
        AddComponent(_rigidbody);
    }

    public void Teleport(Vector2 to) => Position = to; // adopted automatically

    public XElement SaveState() => new XElement("Ball",
        new XElement("Position",
            new XAttribute("X", Position.X),
            new XAttribute("Y", Position.Y)));

    public void LoadState(XElement element)
    {
        var pos = element.Element("Position");
        if (pos != null)
            Position = new Vector2(
                float.Parse(pos.Attribute("X")!.Value),
                float.Parse(pos.Attribute("Y")!.Value));
        // Physics body re-anchors itself on the next Update.
    }
}
```
