# SendMessage — Scene-Wide Messaging

`SendMessage` broadcasts a message to **every** entity and component in the scene, Unity-style. Any public instance method whose name matches the message is invoked — so components can talk to each other (and to the scene) without holding references or knowing about each other's types.

```csharp
// From anywhere with access to the system:
entitySystem.SendMessage("OnPlayerDied");

// Or from an entity / component (Unity-style one-liners):
myEntity.SendMessage("OnPlayerDied");
myComponent.SendMessage("OnPlayerDied", 42);
```

## Semantics

| Behavior | Detail |
|----------|--------|
| Scope | Scene-wide — every root entity **and** its full child subtree (messages reach `AddChild`ed children, matching how the update loop reaches them). |
| Multi-cast | **All** matching handlers fire. There is no "first match wins" — if three entities each define `OnPlayerDied()`, all three run. |
| Handlers | Public instance methods named exactly like the message, with **0 parameters** or **1 parameter** (receives the payload). Generic method definitions are ignored. |
| Targets | Both the entity itself and every component on it (and on every descendant). |
| Return value | The number of handlers invoked. `0` for an unknown/empty message; `-1` from the `Entity`/`Component` convenience methods when there is no system to reach. |
| Exceptions | A handler that throws is caught, unwrapped from `TargetInvocationException`, and logged as `[EntitySystem] SendMessage handler '...' threw: ...` — the broadcast continues to every remaining handler. |
| Reentrancy | Iteration is snapshot-based, so a handler may spawn or destroy entities mid-broadcast without corrupting the walk (newly spawned entities are *not* part of the in-flight broadcast). |

### Payloads

```csharp
system.SendMessage("OnHealed", 42);
```

- A zero-parameter `OnHealed()` still fires.
- A one-parameter `OnHealed(int amount)` receives `42`.
- There is no type checking — the payload is delivered as `object`; a handler with an incompatible parameter type simply fails to invoke and is skipped (logged).

## API Reference

### EntitySystem.SendMessage(string message, object? payload = null)

```csharp
public int SendMessage(string message, object? payload = null)
```

Broadcasts the message across the whole scene. Returns the number of handlers invoked (`0` if none matched or the message is empty/whitespace).

### Entity.SendMessage / Component.SendMessage

```csharp
// On Entity:
public int SendMessage(string message, object? payload = null);

// On EntityComponent (broadcasts from the owning entity's system):
public int SendMessage(string message, object? payload = null);
```

Both return `-1` when the entity is not attached to a system.

### Entity.GetEntitySystem()

```csharp
public EntitySystem? GetEntitySystem();
```

Returns the `EntitySystem` this entity lives in (or `null` if detached). Components reach it through the convenience properties below. This is what makes all of the one-liners possible — no static references, no service locators.

### Component convenience properties

```csharp
public EntitySystem? EntitySystem => Owner?.GetEntitySystem();   // the system (spawn, destroy, queries, SendMessage)
public MainGame? Game => EntitySystem?.Game;                     // the game / scene manager chain
```

> **Naming note:** the property is intentionally named `EntitySystem`, not `System` — a member named `System` would shadow the `System` namespace for every component subclass.

## Unity-Style Entity Management One-Liners

These pair with each other exactly like Unity's `GameObject.Create` / `Instantiate` / `Destroy`:

```csharp
// Spawn (typed) — returns null instead of throwing when detached:
Ball ball = CreateGameObject<Ball>();

// Spawn from a registered template ("prefab") at a position:
Entity popup = InstantiateTemplate("popup", position);

// Destroy — marks the entity (and its children) for removal next update:
Destroy();                    // on Entity
DestroyOwner();               // on EntityComponent
```

| Call site | Typed spawn | Prefab spawn | Destroy |
|-----------|-------------|--------------|---------|
| `Entity` | `CreateGameObject<T>(args)` | `InstantiateTemplate(name, position)` | `Destroy()` |
| `EntityComponent` | `CreateGameObject<T>(args)` | `InstantiateTemplate(name, position)` | `DestroyOwner()` |

All spawn conveniences return `null` when the caller is not in a system (they never throw). Prefab spawning requires a registered template — see [Entity Templates](./EntityTemplates.md).

## Example: Cross-Subtree Broadcast

A player dies; three unrelated parts of the scene react, with zero references between them:

```csharp
// Somewhere in gameplay code:
player.SendMessage("OnPlayerDied");

// Anywhere else — each fires because its method name matches:
public class HealthBarComponent : EntityComponent
{
    public void OnPlayerDied() => SetFill(0f);
}

public class CameraShakeComponent : EntityComponent
{
    public void OnPlayerDied() => Shake(1.5f, 0.4f);
}

public class ScoreEntity : Entity
{
    public void OnPlayerDied(int? penalty = null) { /* ... */ }
}
```

Because the walk is scene-wide and multi-cast, the death ripple needs no event registry, no channel names shared between systems, and no `FindById` lookups.

## When to Use What

| Need | Use |
|------|-----|
| "Tell everyone in this scene" (game events, state changes) | `SendMessage` |
| One specific component reacting to another's event, wired at load time | `<Bind>` declarative wiring — see [XML Entity Definitions](./XMLEntityDefinitions.md#declarative-command-binding) |
| Narrow, typed, bidirectional communication between two known systems | A direct method call or a dedicated `GameSystem` |

## Testing

Covered in `CoreEssentials.Tests/GameSystems/EntitySystems/EntityOOPsystem/SendMessageTests.cs`: broadcast scope (entity, component, cross-subtree), payload delivery, unknown/empty messages, handler exception isolation, spawn-during-broadcast safety, detached-caller returns, and the create/destroy/prefab conveniences.
