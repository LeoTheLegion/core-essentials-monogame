# Entity System

The Entity System in CoreEssentials-MonoGame provides an object-oriented approach to managing game objects. It allows you to define, create, and manage entities in your game scenes.

## Feature Overview

The Entity System includes several advanced features:

| Feature | Documentation | Description |
|---------|---------------|-------------|
| **Entity Tags** | [EntityTags.md](./EntityTags.md) | Categorize entities with tags for flexible querying |
| **Query API** | [EntityQueryAPI.md](./EntityQueryAPI.md) | Find entities by type, position, and spatial queries |
| **Entity Pooling** | [EntityPooling.md](./EntityPooling.md) | Reuse entities to reduce garbage collection |
| **Hierarchy** | [EntityHierarchy.md](./EntityHierarchy.md) | Parent-child relationships with transform inheritance |
| **Spatial Partitioning** | [SpatialPartitioning.md](./SpatialPartitioning.md) | Grid-based optimization for spatial queries |
| **Lifecycle** | [EntityLifecycle.md](./EntityLifecycle.md) | Delayed destruction, spawning, and respawning |
| **Templates** | [EntityTemplates.md](./EntityTemplates.md) | Reusable entity blueprints from XML |
| **XML Definitions** | [XMLEntityDefinitions.md](./XMLEntityDefinitions.md) | Load entities from XML files |
| **Event System** | [EventSystem.md](./EventSystem.md) | Decoupled entity communication |

## Key Components

### EntitySystem

The `EntitySystem` is a `GameSystem` that manages the creation, updates, and destruction of entities.

```csharp
// Get the EntitySystem from a scene
EntitySystem entitySystem = GetGameSystem<EntitySystem>();

// Create an entity at a specific position
YourEntity entity = entitySystem.CreateEntity<YourEntity>(new Vector2(100, 100));

// Get all active entities of a specific type
List<YourEntity> entities = entitySystem.FindByType<YourEntity>();
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
    
    // Called once when the entity is added to the EntitySystem
    public override void OnAwake()
    {
        base.OnAwake();
        // One-time setup before the entity is active
    }

    // Called once when the entity first becomes active
    public override void OnStart()
    {
        base.OnStart();
        // Initialize your entity
    }

    // Called every frame for game logic
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Update your entity
    }

    // Called every frame for rendering
    public override void Render(SpriteBatch spriteBatch)
    {
        base.Render(spriteBatch);
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
2. **Awake**: The entity's `OnAwake()` method is called when it is added to the system
3. **Start**: The entity's `OnStart()` method is called once when it first becomes active
4. **Updates**: The entity's `Update()` method is called each frame
5. **Rendering**: The entity's `Render()` method is called each frame
6. **Destruction**: Call `entity.Destroy()` to remove the entity (fires `OnDestroy()`)

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
    private RigidbodyComponent _rigidbody;
    
    public override void OnStart()
    {
        base.OnStart();
        
        // Add a dynamic rigidbody component (creates the physics body lazily)
        _rigidbody = new RigidbodyComponent(RigidbodyType.Dynamic);
        AddComponent(_rigidbody);
        
        // Add a circle collider
        AddComponent(new ColliderComponent(radius: 1.0f));
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Position/Rotation are synced from the physics body automatically
        // by the RigidbodyComponent (SyncFromPhysics is on by default for Dynamic).
    }
}
```

## Best Practices

- Create specialized entity classes for different game objects
- Keep entity logic contained within the entity class
- Use the entity system to manage entity creation and retrieval
- Clean up physics bodies when destroying entities
- Group similar entities under common base classes for shared behavior
- Use tags for flexible categorization instead of deep inheritance
- Use pooling for high-frequency entities like bullets and particles
- Use spatial queries for performance-critical operations
- Use templates for reusable entity definitions

## See Also

- [Entity Tags](./EntityTags.md) — Tag-based entity categorization
- [Entity Query API](./EntityQueryAPI.md) — Finding entities
- [Entity Pooling](./EntityPooling.md) — Object pooling
- [Entity Hierarchy](./EntityHierarchy.md) — Parent-child relationships
- [Spatial Partitioning](./SpatialPartitioning.md) — Grid-based optimization
- [Entity Lifecycle](./EntityLifecycle.md) — Delayed operations
- [Entity Templates](./EntityTemplates.md) — Reusable blueprints
- [XML Entity Definitions](./XMLEntityDefinitions.md) — XML loading
- [Event System](./EventSystem.md) — Entity communication