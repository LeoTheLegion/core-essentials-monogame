# Sprint 9 — Per-Collider Collision Events 🎯

**Points:** 4  
**Status:** Not Started (depends on Sprint 8)  
**Sprint Goal:** Wire Aether's `ContactManager` to expose per-collider collision/separation events via `ICollider`, enabling fine-grained hit detection.

---

## Background

Sprint 8 added **per-body** collision events — useful for reacting when *any* collider on a body collides, but doesn't tell you *which* one was hit. This sprint adds per-collider events so you can differentiate between headshots, stomps, and other shape-specific logic.

```csharp
// Per-collider — know exactly which shape was hit!
headCollider.OnCollision += (colliderA, colliderB) => { 
    if (colliderB.OwnerBody.Type == "Enemy") 
        TakeHeadshotDamage(); // Special head collision logic
};

bodyCollider.OnSeparation += (colliderA, colliderB) => {
    if (!IsGrounded()) HandleLanding();
};
```

---

## Architecture Decision

Per-collider events sit on top of the per-body wiring from Sprint 8. The same `ContactManager` callbacks are routed to both levels:

```
Aether BeginContact(contact)
  ├─ FixtureA → collider.OnCollision(colliderA, colliderB)  ← NEW (per-collider)
  ├─ FixtureB → collider.OnCollision(colliderB, colliderA)  ← NEW (per-collider)
  ├─ BodyA    → body.OnCollision(bodyA, bodyB)              ← Already done in Sprint 8
  └─ BodyB    → body.OnSeparation(bodyB, bodyA)             ← Already done in Sprint 8
```

---

## Tasks

### T1: Define per-collider event types (0.5 pt)

Create event argument records in `CoreEssentials/src/gameSystems/physics/types/CollisionEventArgs.cs` (add to existing file from Sprint 8):

```csharp
/// <summary>
/// Arguments for collider-level collision events.
/// </summary>
public record ColliderCollisionEventArgs(ICollider ColliderA, ICollider ColliderB);

/// <summary>
/// Arguments for collider-level separation events.
/// </summary>
public record ColliderSeparationEventArgs(ICollider ColliderA, ICollider ColliderB);
```

### T2: Add event delegates to ICollider (0.5 pt)

Add event declarations to `ICollider`:
```csharp
/// <summary>
/// Fired when this collider starts colliding with another collider.
/// Return true from the handler to allow the collision; return false to reject it.
/// </summary>
event Func<ColliderCollisionEventArgs, bool>? OnCollision;

/// <summary>
/// Fired when this collider stops colliding with another collider.
/// Fires once per separated collider pair (independent of body-level OnSeparation).
/// </summary>
event Action<ColliderSeparationEventArgs>? OnSeparation;
```

### T3: Wire up per-collider routing in PhysicsEngine (1 pt)

In the existing `OnWorldBeginContact/EndContact` handlers from Sprint 8, add collider-level notification:

- For each `BeginContact`, after resolving fixtures to colliders:
  - Notify both colliders' `OnCollision`: `colliderA.RaiseOnCollision(colliderB)` and vice versa
  - If **any** handler (body or collider) returns `false`, call `contact.Enabled = false`
  
- For each `EndContact`, notify both colliders' `OnSeparation`

- Track which collider pairs are in contact to correctly fire separation events when contacts end.

### T4: Implement collision callbacks in Collider (1 pt)

In `Collider.cs`, add event delegates and internal raise methods:
```csharp
public event Func<ColliderCollisionEventArgs, bool>? OnCollision;
public event Action<ColliderSeparationEventArgs>? OnSeparation;

internal void RaiseOnCollision(ICollider otherCollider)
{
    var args = new ColliderCollisionEventArgs(this, otherCollider);
    OnCollision?.Invoke(args); // Returns bool? — caller must check result.
}

internal void RaiseOnSeparation(ICollider otherCollider)
{
    OnSeparation?.Invoke(new ColliderSeparationEventArgs(this, otherCollider));
}
```

### T5: Add tests for per-collider collision events (1 pt)

Extend `CoreEssentials.Tests/GameSystems/Physics/CollisionEventsTests.cs` with collider-specific tests:
- **Test 6:** Two bodies each with one collider → both colliders receive `OnCollision` with correct references
- **Test 7:** Collider handler returns `false` → contact rejected (`contact.Enabled = false`)
- **Test 8:** Bodies separate → both colliders receive `OnSeparation`
- **Test 9:** Body with 2 colliders (head + body) collides with single collider enemy → head event fires when only the head is in contact

### T6: Update documentation (0.5 pt)

- `docs/PhysicsSystem.md`: Add per-collider examples alongside existing per-body examples
- `docs/Migration_Guide_Physics.md`: Update Section 7 to show working code at both levels

---

## Acceptance Criteria

- [ ] `ICollider` has `OnCollision` / `OnSeparation` events
- [ ] Events fire correctly when colliders collide/separate during simulation step
- [ ] Returning `false` from collider-level handler rejects the collision
- [ ] All 4 new tests (6–9) in `CollisionEventsTests.cs` pass
- [ ] Zero CS1591 warnings on build

---

## Deliverables

| Artifact | Purpose |
|----------|---------|
| `CoreEssentials/src/gameSystems/physics/types/CollisionEventArgs.cs` | Added: `ColliderCollisionEventArgs`, `ColliderSeparationEventArgs` |
| `ICollider` | Updated with `OnCollision` / `OnSeparation` delegates |
| `PhysicsEngine.cs` | Extended routing to include per-collider notifications |
| `Collider.cs` | Event delegate storage + internal raise methods |
| `CoreEssentials.Tests/GameSystems/Physics/CollisionEventsTests.cs` | 4 new collider-level tests (tests 6–9) |
| `docs/PhysicsSystem.md` | Updated with collider-level examples |

---

## Notes & Risks

- **Separation semantics:** Per-collider `OnSeparation` fires independently per pair. If BodyA has head+body colliders and both are in contact with an enemy, two separate separation events will fire (one for each collider pair).
- **Performance:** Firing on every contact adds overhead. Consider whether to batch notifications or provide an optional "low-level" raw contact callback vs. the simplified per-collider API.
- **Breaking change:** Adding events to `ICollider` is a breaking interface change — any existing implementation would need updating. Since this is still pre-release, acceptable.

---

*Created: 2026-07-18 | Part of Physics System Refactoring Project*
