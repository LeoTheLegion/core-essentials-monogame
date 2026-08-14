# Entity Tweening

Entity tweening provides simple value interpolation for animating entity properties over time. Instead of directly modifying values, you create a tween that interpolates from a start value to an end value, then read the eased value each frame and apply it yourself.

## Quick Start

```csharp
// Add a TweenComponent to your entity
var tweenComponent = entity.AddComponent(new TweenComponent());

// Create a position tween
var posTween = tweenComponent.TweenToVector2(
    entity.Position,           // Start value
    new Vector2(100, 200),     // End value
    1f,                        // Duration in seconds
    EasingFunctions.InQuad     // Easing function (optional)
);

// In your entity's Update method:
entity.Position = posTween.GetValue();
```

## Tween Types

### TweenVector2

Interpolates a `Vector2` from start to end over time.

```csharp
var tween = component.TweenToVector2(
    Vector2.Zero,              // Start
    new Vector2(100, 200),    // End
    1f                         // Duration (seconds)
);
// Returns: TweenVector2
```

### TweenFloat

Interpolates a `float` from start to end over time. Useful for rotation, scale, opacity, or offsets.

```csharp
var tween = component.TweenToFloat(
    0f,                        // Start
    MathHelper.PiOver2,        // End (90 degrees)
    0.5f                       // Duration (seconds)
);
// Returns: TweenFloat
```

## Tween Properties

| Property | Type | Description |
|----------|------|-------------|
| `StartValue` | Vector2/float | The starting value of the tween |
| `EndValue` | Vector2/float | The target end value |
| `Duration` | float | Total duration in seconds |
| `IsComplete` | bool | `true` when the tween has finished |
| `Loop` | bool | Repeat animation on completion (default: `false`) |
| `Reverse` | bool | Ping-pong: smoothly reverse direction each cycle (default: `false`) |

## Looping

Set `Loop = true` to repeat the animation when it completes. The tween resets to the start and plays again.

```csharp
var bounceTween = component.TweenToFloat(0f, -50f, 1f, t => (float)Math.Sin(t * Math.PI));
bounceTween.Loop = true; // Repeats forever
```

## Reverse (Ping-Pong)

Set both `Loop` and `Reverse` to smoothly reverse direction instead of snapping back:

```csharp
var slideTween = component.TweenToVector2(Vector2.Zero, new Vector2(200, 0), 1f);
slideTween.Loop = true;
slideTween.Reverse = true; // start → end → start → end...
```

> **Note:** Reverse works best with monotonic easings (linear, in-out quad). Half-sine waves like `sin(t * π)` already do a round trip within one pass — use `Loop` alone for those.

## Easing Functions

Any `Func<float, float>` works as an easing function. If none is provided, the tween uses linear interpolation.

### Built-in easings (`CoreEssentials.Tweening.EasingFunctions`)

The `EasingFunctions` static class provides 30+ standard easing curves:

| Family | In | Out | InOut |
|--------|----|-----|-------|
| Linear | `Linear` | — | — |
| Quad | `InQuad` | `OutQuad` | `InOutQuad` |
| Cubic | `InCubic` | `OutCubic` | `InOutCubic` |
| Quart | `InQuart` | `OutQuart` | `InOutQuart` |
| Quint | `InQuint` | `OutQuint` | `InOutQuint` |
| Sine | `InSine` | `OutSine` | `InOutSine` |
| Expo | `InExpo` | `OutExpo` | `InOutExpo` |
| Circ | `InCirc` | `OutCirc` | `InOutCirc` |
| Elastic | `InElastic` | `OutElastic` | `InOutElastic` |
| Back | `InBack` | `OutBack` | `InOutBack` |
| Bounce | `InBounce` | `OutBounce` | `InOutBounce` |

```csharp
// Example usage
var tween = component.TweenToVector2(Vector2.Zero, new Vector2(100, 100), 1f, EasingFunctions.InOutSine);
```

### Custom easing

```csharp
// Half sine wave — smooth slow-in and slow-out round trip
var tween = component.TweenToFloat(0f, -50f, 1f, t => (float)Math.Sin(t * Math.PI));
```

## Common Patterns

### Animating Position

```csharp
var posTween = component.TweenToVector2(entity.Position, new Vector2(100, 200), 1f);
// In Update:
entity.Position = posTween.GetValue();
```

### Animating Rotation

```csharp
var rotTween = component.TweenToFloat(0f, MathHelper.Pi, 1f);
// In Update:
entity.Rotation = rotTween.GetValue();
```

### Animating Scale

```csharp
var scaleTween = component.TweenToVector2(Vector2.One, new Vector2(2, 2), 0.5f);
// In Update:
entity.Scale = scaleTween.GetValue();
```

### Offset Animation (relative to spawn position)

Tween an offset value and add it to the entity's base position:

```csharp
private float _originalY;
private TweenFloat? _yOffsetTween;
private bool _initialized;

public override void OnStart()
{
    var component = AddComponent(new TweenComponent());
    _yOffsetTween = component.TweenToFloat(0f, -50f, 1.5f, EasingFunctions.InOutSine);
    _yOffsetTween.Loop = true;
    _yOffsetTween.Reverse = true; // Ping-pong: up and down smoothly
}

public override void Update(GameTime gameTime)
{
    // Capture original position on first frame
    if (!_initialized)
    {
        _originalY = Position.Y;
        _initialized = true;
    }

    // Apply offset to original position
    Position = new Vector2(Position.X, _originalY + _yOffsetTween.GetValue());
}
```

### Multiple Simultaneous Tweens

A single `TweenComponent` can manage multiple tweens at once:

```csharp
var posTween = component.TweenToVector2(entity.Position, new Vector2(100, 200), 1f);
var rotTween = component.TweenToFloat(0f, MathHelper.Pi, 1f);

// In Update — both advance automatically:
entity.Position = posTween.GetValue();
entity.Rotation = rotTween.GetValue();
```

## Canceling Tweens

Stop all active tweens on a component:

```csharp
component.CancelAll();
```

## How It Works

The `TweenComponent` automatically advances all its tweens each frame via `Update(GameTime)`. You read the current eased value with `.GetValue()` and apply it to whatever property you want.

- **One-shot tweens** are automatically removed when complete
- **Looping tweens** reset on completion and continue
- **Reverse looping tweens** toggle direction instead of resetting

## API Reference

### TweenComponent

| Method | Returns | Description |
|--------|---------|-------------|
| `TweenToVector2(start, end, duration, easing?)` | `TweenVector2` | Creates a Vector2 tween |
| `TweenToFloat(start, end, duration, easing?)` | `TweenFloat` | Creates a float tween |
| `CancelAll()` | void | Removes all active tweens |

### TweenVector2 / TweenFloat

| Member | Type | Description |
|--------|------|-------------|
| `StartValue` | Vector2/float | Starting value (read-only) |
| `EndValue` | Vector2/float | End value (read-only) |
| `Duration` | float | Duration in seconds (read-only) |
| `IsComplete` | bool | `true` when finished |
| `Loop` | bool | Repeat on completion |
| `Reverse` | bool | Ping-pong animation |
| `GetValue()` | Vector2/float | Current eased value |
