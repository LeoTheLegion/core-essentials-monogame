# Entity Position and Rotation Properties

This document describes the usage of Position and Rotation properties added to the Entity system.

## Overview

All entities in the system now expose public properties to get and set their position and rotation in the game world:

- `Position`: Gets or sets the Vector2 position of the entity
- `Rotation`: Gets or sets the float rotation (in radians) of the entity

## Usage

### Getting Entity Position

```csharp
// Get the current position of an entity
Vector2 currentPosition = myEntity.Position;
```

### Setting Entity Position

```csharp
// Set a new position for an entity
myEntity.Position = new Vector2(100, 200);

// Or update just one component
myEntity.Position = new Vector2(myEntity.Position.X + 10, myEntity.Position.Y);
```

### Getting Entity Rotation

```csharp
// Get the current rotation (in radians)
float currentRotation = myEntity.Rotation;
```

### Setting Entity Rotation

```csharp
// Set a new rotation (in radians)
myEntity.Rotation = 1.5f; // About 90 degrees

// Or increase/decrease rotation
myEntity.Rotation += 0.1f; // Rotate a bit more
```

## Best Practices

- For physics-based entities, update the physics body position instead of directly setting the entity position
- Entities with physics bodies should update their Position/Rotation properties from the body position in their Update method
- Use Position property instead of accessing the protected _position field in derived classes

## Example

```csharp
public class YourEntity : Entity
{
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Move the entity to the right
        Position = new Vector2(Position.X + 1, Position.Y);
        
        // Rotate slowly
        Rotation += 0.01f;
    }
}
```
