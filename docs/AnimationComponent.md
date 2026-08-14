# Animation Component

`AnimationComponent` drives one or more **named animations** on an entity. Each animation is an `AnimationState` backed by a unified `Sprite` (a `spritesheet` frame sequence). The component advances playing animations each frame and pushes the current frame into the entity's `SpriteComponent` (when one is present).

It replaces the old pattern of hand-rolling an `AnimatedSprite` + `AnimationState` inside an entity and overriding `Update`/`Render`/`GetSize`. With `AnimationComponent`, an animated entity needs **no overrides** — the base `Entity` handles updating, rendering, and sizing.

## Quick Start

```csharp
public class AnimatedCharacterEntity : Entity
{
    public override void OnStart()
    {
        base.OnStart();

        var sprite = AssetManager.LoadAsset<Sprite>("character_anim_walk.xml");
        var animation = AddComponent(new AnimationComponent());
        animation.AddAnimation("walk", sprite);
        animation.Play("walk");
    }
}
```

That's it. No `Render`, `Update`, or `GetSize` overrides. The base `Entity`:
- calls `AnimationComponent.Update` every frame (advances the animation),
- renders the current frame (via the `SpriteComponent`, or directly as a fallback),
- resolves `GetSize()` through the `AnimationComponent`.

## API

| Member | Description |
|--------|-------------|
| `AddAnimation(name, sprite)` | Registers a named animation backed by a `Sprite`. |
| `Play(name)` | Plays the named animation and pauses all others. Sets it as the current animation. |
| `Stop(name?)` | Stops one animation, or all when `name` is `null`. |
| `SetSpeed(name, speed)` | Sets the playback speed multiplier for a named animation. |
| `GetAnimation(name)` | Returns the `AnimationState` for a named animation, or `null`. |
| `Animations` | The names of all registered animations. |
| `CurrentAnimation` | The name of the active animation (get/set). |
| `CurrentAnimationState` | The `AnimationState` of the current animation, or `null`. |
| `Sprite` | The `Sprite` backing the current animation, or `null`. |
| `GetSize()` | The current frame size × the owning entity's `Scale`. |

### Switching Animations

```csharp
animation.AddAnimation("idle", idleSprite);
animation.AddAnimation("walk", walkSprite);

animation.Play("idle");   // start idle
// ...
animation.Play("walk");   // switch to walk (idle is paused)
```

### Playback Speed

```csharp
animation.SetSpeed("walk", 2f);   // play the walk animation at 2× speed
```

## How It Renders

- **With a `SpriteComponent`:** `AnimationComponent.Update` writes the current frame into `SpriteComponent.AnimationFrame`, and the `SpriteComponent` draws it. The component does **not** draw directly (to avoid double-drawing).
- **Without a `SpriteComponent`:** the component draws the current frame directly as a fallback.

```csharp
// Animated entity with a SpriteComponent (preferred — enables instanced rendering)
AddComponent(new SpriteComponent(sprite));
var animation = AddComponent(new AnimationComponent());
animation.AddAnimation("walk", sprite);
animation.Play("walk");
```

## Entity Sizing

The base `Entity.GetSize()` resolves size through a fallback chain:

```
SpriteComponent → AnimationComponent → Vector2.Zero
```

So an animated entity reports its current frame size × `Scale` with no override.

```csharp
entity.Scale = new Vector2(2, 2);
// A 32×32 animation frame → entity.GetSize() returns (64, 64)
```

## Serialization

`AnimationComponent` implements `ISerializableComponent`. It persists:
- the animation names and their sprite **asset names**,
- the current animation name,
- per-animation speed and loop state.

On restore, sprite assets are reloaded in `OnAttach` (after the component is attached to the entity), so the component works with XML entity/scene loading.

```xml
<Component Type="AnimationComponent">
  <Properties>
    <Property Name="CurrentAnimation" Value="walk" />
  </Properties>
</Component>
```

> **Note:** The component is registered with the `EntitySerializer` as `"AnimationComponent"`, so it can be declared in entity/scene XML like any other built-in component.

## Related

- [Sprite System](SpriteSystem.md) — the unified `Sprite` asset the component animates.
- [Entity System](EntitySystem.md) — components and the base `Entity` lifecycle.
- [Migration Guide](Migration_Guide_SpriteConsolidation.md) — migrating from `AnimatedSprite` + `AnimationState`.
