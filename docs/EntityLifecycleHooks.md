# Entity Lifecycle Hooks

Fine-grained lifecycle callbacks for OOP entities, aligned with the Unity `MonoBehaviour` model. Use these hooks to initialize, enable/disable, and react to app pause at the right moment in an entity's life.

> **Related:** [`EntityLifecycle.md`](./EntityLifecycle.md) covers delayed destruction, spawning, and respawning. This document covers the *callback* hooks.

## Overview

The OOP `Entity` base class exposes the following overridable lifecycle hooks:

| Hook | When it fires |
|------|---------------|
| `OnAwake()` | Once, when the entity is added to its `EntitySystem` (before `OnStart`) |
| `OnEnable()` | When the entity transitions to active |
| `OnStart()` | Once, after `OnAwake` |
| `OnFixedUpdate(GameTime)` | Every fixed timestep, for active entities |
| `Update(GameTime)` | Every frame, for active entities |
| `OnLateUpdate(GameTime)` | Every frame, *after* `Update`, for active entities |
| `OnDisable()` | When the entity transitions to inactive |
| `OnDestroy()` | When the entity is removed from the system |
| `OnApplicationPause(bool)` | App-wide, when the game window loses/regains focus |

### Hook order

```
OnAwake() → OnEnable() → OnStart()
   → [ OnFixedUpdate() (fixed timestep) → Update() → OnLateUpdate() ]*
   → OnApplicationPause(bool) (app-wide, any time)
   → OnDisable() → OnDestroy()
```

Notes on ordering:
- `OnAwake` fully completes **before** `OnEnable` fires (matching Unity's `Awake → OnEnable`).
- `OnEnable`/`OnDisable` fire **only on real transitions** — calling `SetActive` with the current state is a no-op and does not re-trigger the hook.
- `OnFixedUpdate` runs on the fixed timestep (50 Hz by default), independent of frame rate.
- `OnLateUpdate` runs after all entities' `Update` for that frame, so it sees the final state of the frame (good for camera follow, end-of-frame corrections).
- `OnApplicationPause` is **app-wide** — every active entity receives it when the window loses or regains focus.

## API Reference

### OnAwake()

Called once when the entity is added to its `EntitySystem`, before `OnStart`. Use for one-time initialization that must happen before the entity starts.

```csharp
public virtual void OnAwake()
```

**Example:**
```csharp
protected override void OnAwake()
{
    base.OnAwake();
    // One-time setup that must happen before OnStart.
    _maxHealth = 100;
}
```

### OnEnable()

Called when the entity transitions from inactive to active (via `SetActive(true)`).

```csharp
public virtual void OnEnable()
```

**Example:**
```csharp
public override void OnEnable()
{
    base.OnEnable();
    // Resume behavior, re-register listeners, etc.
    _isMoving = true;
}
```

### OnDisable()

Called when the entity transitions from active to inactive (via `SetActive(false)`).

```csharp
public override void OnDisable()
```

**Example:**
```csharp
public override void OnDisable()
{
    base.OnDisable();
    // Pause behavior, release transient resources, etc.
    _isMoving = false;
}
```

### OnLateUpdate(GameTime gameTime)

Called after `Update` on every frame, for active entities. Use for logic that must run after all regular updates.

```csharp
public virtual void OnLateUpdate(GameTime gameTime)
```

**Example:**
```csharp
public override void OnLateUpdate(GameTime gameTime)
{
    base.OnLateUpdate(gameTime);
    // Camera follow — runs after the player has moved this frame.
    Position = _target.Position + _offset;
}
```

### OnFixedUpdate(GameTime gameTime)

Called on the fixed timestep for active entities. Use for logic that must run at a consistent rate regardless of frame rate.

```csharp
public virtual void OnFixedUpdate(GameTime gameTime)
```

**Example:**
```csharp
public override void OnFixedUpdate(GameTime gameTime)
{
    base.OnFixedUpdate(gameTime);
    // Frame-rate-independent movement.
    Position += _velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;
}
```

### OnApplicationPause(bool paused)

Called app-wide when the application loses or regains focus. Use to pause or resume entity-specific behavior (timers, background work, saving state).

> **Pausing audio:** `OnApplicationPause` is the place to pause or resume any audio the owner is responsible for. `AudioManager` exposes per-sound `PauseSound(id)` / `ResumeSound(id)` (as well as `PauseAll()` / `ResumeAll()` and per-clip `AudioClipInstance.Pause()`). Because a scene's background music is *scene-owned*, a scene should override `Scene.OnApplicationPause` (now `virtual`) and pause/resume its own track there — see `CharacterScene` for an example. Entities that own their own sound should do the same inside their `OnApplicationPause` override.

```csharp
public virtual void OnApplicationPause(bool paused)
```

**Parameters:**
- `paused` — `true` when the application is being paused, `false` when resuming.

**Example:**
```csharp
public override void OnApplicationPause(bool paused)
{
    base.OnApplicationPause(paused);
    if (paused)
        SaveState();
    else
        ResumeTimers();
}
```

## Complete example

```csharp
public class PowerUpEntity : Entity
{
    private float _timer;

    protected override void OnAwake()
    {
        base.OnAwake();
        _timer = 0f;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        // Only count down while active.
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_timer >= 10f)
            Destroy();
    }

    public override void OnApplicationPause(bool paused)
    {
        base.OnApplicationPause(paused);
        // Optionally freeze the timer while the app is backgrounded.
    }

    public override void Render(SpriteBatch spriteBatch) { }
}
```

## Comparison with Unity

| Unity hook | Our hook | Notes |
|-----------|----------|-------|
| `Awake()` | `OnAwake()` | Unity: on instantiation. Ours: on add-to-system |
| `OnEnable()` | `OnEnable()` | Ours fires only on real transitions (Unity fires every toggle) |
| `Start()` | `OnStart()` | Unchanged |
| `Update()` | `Update()` | Unchanged |
| `LateUpdate()` | `OnLateUpdate()` | New |
| `FixedUpdate()` | `OnFixedUpdate()` | New; driven by `IFixedUpdateGameSystem` |
| `OnDisable()` | `OnDisable()` | Ours fires only on real transitions |
| `OnDestroy()` | `OnDestroy()` | Unchanged |
| `OnApplicationPause(bool)` | `OnApplicationPause(bool)` | App-wide (window focus), matching Unity semantics |

Deliberate divergences:
- **Naming:** we keep the `On` prefix for consistency with existing `OnStart`/`OnDestroy`.
- **Enable/disable:** we guard against redundant hook calls on no-op `SetActive`; Unity does not.
- **Pause:** app-wide via `OnApplicationPause(bool)`, matching Unity (not per-entity).
