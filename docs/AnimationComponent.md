# Animation Component

`AnimationComponent` drives one or more **named animations** on an entity. Each animation is an `AnimationState` backed by a unified `Sprite` (a `spritesheet` frame sequence). The component advances playing animations each frame and pushes the current frame into the entity's `SpriteComponent`.

It is a **pure controller**: it owns *which* frame is showing, but all **rendering and geometry** (drawing, `GetSize()`, `GetOrigin()`) live on the `SpriteComponent`. An entity that uses `AnimationComponent` **must** also attach a `SpriteComponent`.

It replaces the old pattern of hand-rolling an `AnimatedSprite` + `AnimationState` inside an entity and overriding `Update`/`Render`/`GetSize`. With `AnimationComponent` + `SpriteComponent`, an animated entity needs **no overrides** — the base `Entity` handles updating, rendering, and sizing.

## Quick Start

```csharp
public class AnimatedCharacterEntity : Entity
{
    public override void OnStart()
    {
        base.OnStart();

        var sprite = AssetManager.LoadAsset<Sprite>("character_anim_walk.xml");
        AddComponent(new SpriteComponent(sprite));      // owns rendering + geometry
        var animation = AddComponent(new AnimationComponent()); // pure controller
        animation.AddAnimation("walk", sprite);
        animation.Play("walk");
    }
}
```

That's it. No `Render`, `Update`, or `GetSize` overrides. The base `Entity`:
- calls `AnimationComponent.Update` every frame (advances the animation and pushes the frame),
- renders the current frame via the `SpriteComponent`,
- resolves `GetSize()` / `GetOrigin()` through the `SpriteComponent`.

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

`AnimationComponent` never draws itself. `AnimationComponent.Update` writes the current frame into `SpriteComponent.AnimationFrame`, and the `SpriteComponent` (the sole `IDrawableComponent`) draws it. This keeps a single render path and enables instanced rendering.

```csharp
// Animated entity: SpriteComponent renders, AnimationComponent drives the frames.
AddComponent(new SpriteComponent(sprite));
var animation = AddComponent(new AnimationComponent());
animation.AddAnimation("walk", sprite);
animation.Play("walk");
```

## Entity Sizing

The base `Entity.GetSize()` / `Entity.GetOrigin()` resolve from the entity's `SpriteComponent` (the single source of truth for geometry). So an animated entity reports its current frame size/origin × `Scale` with no override.

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
