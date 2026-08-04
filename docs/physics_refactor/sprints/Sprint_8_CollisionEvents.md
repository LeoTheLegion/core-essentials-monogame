# Sprint 8 — Per-Body Collision Events (OnCollision / OnSeparation) 🔔

**Points:** 3  
**Status:** ✅ Complete — all tasks done, 6/6 tests pass, zero CS1591 warnings.  
**Sprint Goal:** Wire Aether's `ContactManager` at the engine level and expose per-body collision/separation events via `IPhysicsBody`.

---

## Background

During the physics refactor (Sprint 0–8), direct access to Aether's `ContactManager.BeginContact/EndContact` was removed from the abstraction layer. Users can no longer subscribe to collision callbacks on individual bodies:

```csharp
body.OnCollision += (bodyA, bodyB) => { /* collision started */ return true; };
body.OnSeparation += (bodyA, bodyB) => { /* collision ended */ };
```

**Legacy reference:** `docs/AdvancedTopics.md` lines 338–368 show the original pattern using `_physicsEngine.World.ContactManager.BeginContact += OnBeginContact`.

---

## Architecture Decision

Aether fires contact events at the **World level**, not per-body. The implementation must:
1. Subscribe to `PhysicsEngine._world.ContactManager.BeginContact/EndContact` once (at engine level)
2. Route each contact event to the relevant `IPhysicsBody` instances based on fixture ownership
3. Allow per-body subscriptions via delegates on `IPhysicsBody`

---

## Tasks

### T1: Define collision event types and add to IPhysicsBody (1 pt)

Create event argument records in `CoreEssentials/src/gameSystems/physics/types/`:

```csharp
/// <summary>
/// Arguments for body-level collision events.
/// </summary>
public record BodyCollisionEventArgs(IPhysicsBody BodyA, IPhysicsBody BodyB);

/// <summary>
/// Arguments for body-level separation events.
/// </summary>
public record BodySeparationEventArgs(IPhysicsBody BodyA, IPhysicsBody BodyB);
```

Add event delegates to `IPhysicsBody`:
```csharp
/// <summary>
/// Fired when this body starts colliding with another body.
/// Return true from the handler to allow the collision; return false to reject it.
/// </summary>
event Func<BodyCollisionEventArgs, bool>? OnCollision;

/// <summary>
/// Fired when this body stops colliding with another body.
/// Fires once per separated body pair (not per-collider).
/// If a body has multiple colliders in contact, this fires only after the last pair separates.
/// </summary>
event Action<BodySeparationEventArgs>? OnSeparation;
```

### T2: Wire up Aether ContactManager in PhysicsEngine (1 pt)

- In `PhysicsEngine` constructor, subscribe to `_world.ContactManager.BeginContact/EndContact`:
  ```csharp
  _world.ContactManager.BeginContact += OnWorldBeginContact;
  _world.ContactManager.EndContact += OnWorldEndContact;
  ```

- Implement routing logic:
  - For each `BeginContact`, resolve the two fixtures → their owner bodies (via `_physicsBodies` dictionary lookup)
  - Notify both bodies' `OnCollision` events with `BodyCollisionEventArgs(this, other)`
  - If any handler returns `false`, call `contact.Enabled = false` to reject the collision
  - For `EndContact`, notify both bodies' `OnSeparation` events

- Handle edge cases:
  - Body already disposed during callback (null checks)
  - Contact between a body and a non-physics object (e.g., terrain with no wrapper)
  - World disposal: unsubscribe from Aether events in `PhysicsEngine.Dispose()`

### T3: Implement collision callbacks in PhysicsBody (0.5 pt)

- In `PhysicsBody`, expose the event delegates as auto-events:
  ```csharp
  public event Func<BodyCollisionEventArgs, bool>? OnCollision;
  public event Action<BodySeparationEventArgs>? OnSeparation;
  ```

- Provide an internal method for `PhysicsEngine` to invoke:
  ```csharp
  internal void RaiseOnCollision(IPhysicsBody other)
  {
      var args = new BodyCollisionEventArgs(this, other);
      OnCollision?.Invoke(args); // Returns bool? — caller must check result.
  }

  internal void RaiseOnSeparation(IPhysicsBody other)
  {
      OnSeparation?.Invoke(new BodySeparationEventArgs(this, other));
  }
  ```

**Design decision — collision rejection:** Return `bool?` from handler. If any returns `false`, the contact is rejected via `contact.Enabled = false`.

### T4: Add tests for per-body collision events (0.5 pt)

Create `CoreEssentials.Tests/GameSystems/Physics/CollisionEventsTests.cs`:
- **Test 1:** Two dynamic bodies collide → both receive `OnCollision` with correct body references
- **Test 2:** Handler returns `false` → collision is rejected (`contact.Enabled = false`)
- **Test 3:** Bodies separate → both receive `OnSeparation`
- **Test 4:** Static body collides with dynamic body → static also receives event
- **Test 5:** Body disposed while collision active → no null reference exception

### T5: Update documentation (0 pt)

- `docs/PhysicsSystem.md`: Add new section "Collision Events" with code examples showing per-body usage
- `docs/Migration_Guide_Physics.md`: Remove the "NOT part of current abstraction layer" note; update Section 7 to show working collision event code (body-level only for now — collider-level coming in Sprint 9)

---

## Acceptance Criteria

- [ ] `IPhysicsBody` has `OnCollision` / `OnSeparation` events
- [ ] Events fire correctly when bodies collide/separate during simulation step
- [ ] Returning `false` from `OnCollision` handler rejects the collision
- [ ] All 5 tests in `CollisionEventsTests.cs` pass
- [ ] Zero CS1591 warnings on build

---

## Deliverables

| Artifact | Purpose |
|----------|---------|
| `CoreEssentials/src/gameSystems/physics/types/CollisionEventArgs.cs` (new) | Event argument types: `BodyCollisionEventArgs`, `BodySeparationEventArgs` |
| `IPhysicsBody` | Updated with `OnCollision` / `OnSeparation` delegates |
| `PhysicsEngine.cs` | World-level ContactManager wiring + per-body routing |
| `PhysicsBody.cs` | Event delegate storage + internal raise methods |
| `CoreEssentials.Tests/GameSystems/Physics/CollisionEventsTests.cs` (new) | 5 unit tests for per-body collision events |
| `docs/PhysicsSystem.md` | Updated with Collision Events section |

---

## Notes & Risks

- **Threading:** Aether runs on the game thread (via `IFixedUpdateGameSystem.FixedUpdate`), so no concurrent access issues — standard event invocation is safe.
- **Separation semantics:** Body-level `OnSeparation` fires when the *last* contact between two bodies ends. If a body has multiple colliders in contact with another, this fires only after all collider pairs have separated (Sprint 9 will add per-collider separation events).
- **Breaking change:** Adding events to `IPhysicsBody` is a breaking interface change — any existing implementation would need updating. Since this is still pre-release, acceptable.

---

*Created: 2026-07-18 | Part of Physics System Refactoring Project*
