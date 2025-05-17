# Sprite Scaling

This document explains how to use the scaling feature in the sprite rendering system.

## Overview

The sprite system now supports scaling sprites during rendering, allowing for dynamic size changes without modifying the original asset. This feature is useful for:

- Creating size variations of the same sprite (small, medium, large entities)
- Implementing zoom effects
- Creating growth or shrinking animations
- Creating perspective effects

## Usage

### Basic Scaling

To draw a sprite with uniform scaling (same scale for both width and height):

```csharp
// Draw with uniform scale (1.5x normal size)
sprite.Draw(
    spriteBatch, 
    position, 
    Color.White, 
    rotation, 
    1.5f,           // Scale factor (1.0f is normal size)
    SpriteEffects.None, 
    0f
);
```

### Non-Uniform Scaling

For different scaling on X and Y axes (stretching in one direction):

```csharp
// Draw with different X and Y scaling
sprite.Draw(
    spriteBatch, 
    position, 
    Color.White, 
    rotation, 
    new Vector2(2.0f, 1.0f),  // 2x width, normal height
    SpriteEffects.None, 
    0f
);
```

### Example: Dynamic Entity Sizes

The Ball entity in the playground demonstrates how to integrate scaling with physics:

```csharp
public class ScalableEntity : Entity
{
    private Sprite _sprite;
    private float _scale = 1.0f;
    
    // Add a property to control scale
    public float Scale
    {
        get => _scale;
        set
        {
            _scale = value;
            // Update physics if needed when scale changes
        }
    }
    
    public override void Render(SpriteBatch spriteBatch)
    {
        _sprite.Draw(
            spriteBatch,
            Position,
            Color.White,
            Rotation,
            _scale,
            SpriteEffects.None,
            0f
        );
    }
}
```

## Best Practices

- For physics-based entities, remember to update collision shapes when changing scale
- For performance reasons, avoid changing scale every frame if possible
- Consider using non-uniform scaling (different X/Y values) for special effects like squash and stretch
- For animations with scaling, use smooth interpolation between scale values

## Implementation Details

The scale parameter affects:
- The sprite's rendered size
- The debug outline drawn around the sprite
- The origin point's effective distance from the sprite's position
