# Entity System

The Entity System in CoreEssentials-MonoGame provides an object-oriented approach to managing game objects. It allows you to define, create, and manage entities in your game scenes.

## Key Components

### EntitySystem

The `EntitySystem` is a `GameSystem` that manages the creation, updates, and destruction of entities.

```csharp
// Get the EntitySystem from a scene
EntitySystem entitySystem = GetGameSystem<EntitySystem>();

// Create an entity at a specific position
YourEntity entity = entitySystem.CreateEntity<YourEntity>(new Vector2(100, 100));

// Get all entities of a specific type
IEnumerable<YourEntity> entities = entitySystem.GetEntitiesOfType<YourEntity>();
```

### Entity Class

The `Entity` class is the base class for all game objects. Extend this class to create specific entity types.

```csharp
public class YourEntity : Entity
{
    // Constructor often sets initial position
    public YourEntity(Vector2 position)
    {
        Position = position; // Public Position property
    }
    
    // Called when the entity is created
    public override void Initialize()
    {
        base.Initialize();
        // Initialize your entity
    }

    // Called every frame for game logic
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Update your entity
    }

    // Called every frame for rendering
    public override void Draw(SpriteBatch spriteBatch)
    {
        base.Draw(spriteBatch);
        // Draw your entity
    }
}
```

The Entity class provides these key properties and methods:

- `Position`: Gets or sets the entity position as a Vector2
- `Rotation`: Gets or sets the entity rotation in radians
- `SetActive(bool)`: Activates or deactivates the entity
- `Destroy()`: Marks the entity for removal

## Entity Lifecycle

1. **Creation**: Use `entitySystem.CreateEntity<T>()` to instantiate an entity
2. **Initialization**: The entity's `Initialize()` method is called
3. **Updates**: The entity's `Update()` method is called each frame
4. **Rendering**: The entity's `Draw()` method is called each frame
5. **Destruction**: Call `entity.Destroy()` to remove the entity

## Example from Playground

The `CharacterScene` demonstrates effective entity usage:

```csharp
// Inside CharacterScene.cs
protected override IEnumerator OnStartCoroutine()
{
    // Get access to the entity system
    EntitySystem entitySystem = GetGameSystem<EntitySystem>();
    
    // Create a static character entity
    CharacterEntity staticCharacter = entitySystem.CreateEntity<CharacterEntity>(
        new Vector2(graphics.PreferredBackBufferWidth / 4, graphics.PreferredBackBufferHeight / 2)
    );

    // Create an animated character entity
    AnimatedCharacterEntity animatedCharacter = entitySystem.CreateEntity<AnimatedCharacterEntity>(
        new Vector2(graphics.PreferredBackBufferWidth * 3 / 4, graphics.PreferredBackBufferHeight / 2)
    );
}
```

## Physics Entities

When combined with the Physics System, entities can interact with the physical world:

```csharp
public class PhysicsEntity : Entity
{
    public Body Body { get; private set; }
    
    public override void Initialize()
    {
        base.Initialize();
        
        // Access the physics engine
        PhysicsEngine physics = Scene.GetGameSystem<PhysicsEngine>();
        
        // Create a physics body
        Body = physics.CreateCircle(Position, 1.0f, 1.0f);
        Body.BodyType = BodyType.Dynamic;
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Update entity position based on physics body
        Position = Body.Position;
        Rotation = Body.Rotation;
    }
}
```

## Best Practices

- Create specialized entity classes for different game objects
- Keep entity logic contained within the entity class
- Use the entity system to manage entity creation and retrieval
- Clean up physics bodies when destroying entities
- Group similar entities under common base classes for shared behavior