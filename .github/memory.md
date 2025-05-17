# Core Essentials MonoGame - Development Notes

## Entity Position/Rotation Properties

Added public Position and Rotation properties to the Entity base class to allow for easier manipulation of entity positions and rotations.

### What We Learned
- Entities previously used protected `_position` field that couldn't be accessed from outside the entity class
- Adding public properties for Position and Rotation allows for external code to read and modify entity positions
- For physics-based entities, we should sync the Position/Rotation with the physics body in the Update method
- For consistency, entity constructor and methods should use Position property rather than directly modifying `_position` field

### Implementation
- Added public Position (Vector2) and Rotation (float) properties to Entity base class
- Updated all sample entities to use the properties instead of the protected fields
- Created tests to verify the properties work correctly
- Updated documentation in EntityPositionRotation.md and EntitySystem.md

### Sample Usage
```csharp
// Getting entity position
Vector2 currentPos = myEntity.Position;

// Setting entity position
myEntity.Position = new Vector2(100, 200);

// Getting entity rotation
float currentRotation = myEntity.Rotation;

// Setting entity rotation
myEntity.Rotation = 1.5f; // About 90 degrees
```

## Sprite Scaling

Added support for scaling in the Sprite class to allow for dynamic resizing of sprites without modifying the original assets.

### What We Learned
- Sprites needed a way to be rendered with different scales
- For physics-based entities, the physics bodies should match the visual size
- Scale can be applied uniformly (with a single float) or non-uniformly (with a Vector2)
- For best results, scale should be applied to the origin point as well

### Implementation
- Added two new Draw methods to the Sprite class: 
  - One with Vector2 scale parameter (for non-uniform scaling)
  - One with float scale parameter (for uniform scaling)
- Updated the existing Draw method to call the new method with a default scale of 1.0
- Made all rendering code scale-aware
- Created a test class to verify the scaling functionality
- Updated documentation in SpriteScaling.md and AssetManagement.md

### Sample Usage
```csharp
// Draw with uniform scale (1.5x normal size)
sprite.Draw(
    spriteBatch, 
    position, 
    Color.White, 
    rotation, 
    1.5f,           // Scale factor
    SpriteEffects.None, 
    0f
);

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
