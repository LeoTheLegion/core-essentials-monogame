# Game Systems

The Game Systems architecture in CoreEssentials-MonoGame provides a modular approach to organizing game functionality. Each game system is responsible for a specific aspect of functionality, such as physics, entities, or rendering.

## Core Concept

Game systems are modular components that can be added to scenes as needed. Each scene can use different combinations of game systems based on its requirements.

## GameSystem Base Class

All game systems inherit from the `GameSystem` abstract class:

```csharp
// Basic structure of a game system
public class YourGameSystem : GameSystem
{
    // Access the main game instance
    public void DoSomethingWithGame()
    {
        // Game property returns the MainGame instance
        MainGame game = Game;
        
        // Use the game instance to access game resources
        var content = game.Content;
        var graphics = game.GraphicsDevice;
    }
    
    /// <summary>
    /// Called after all game systems in a scene have been loaded and registered.
    /// Override this method to perform any setup that requires access to other
    /// game systems or when all systems are guaranteed to be available.
    /// </summary>
    public override void OnStart()
    {
        base.OnStart();
        // Example: OtherSystem otherSystem = GetGameSystem<OtherSystem>();
    }

    // Implement IUpdateGameSystem to run every frame
    public void Update(GameTime gameTime)
    {
        // Update your system
    }

    // Implement IDrawGameSystem to render every frame
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Draw your system
    }
}
```

> **Note:** per-frame callbacks are opt-in. A `GameSystem` only receives `Update`, `Draw`, or `FixedUpdate` if it implements the corresponding interface (`IUpdateGameSystem`, `IDrawGameSystem`, `IFixedUpdateGameSystem`).

## Using Game Systems in Scenes

Game systems are registered with a scene in the `LoadGameSystems` method:

```csharp
protected override GameSystem[] LoadGameSystems()
{
    return new GameSystem[] 
    {
        new PhysicsEngine(),
        new EntitySystem(),
        new PhysicsDebugRenderer(GetGameSystem<PhysicsEngine>())
    };
}
```

## Accessing Game Systems

Once registered, game systems can be accessed within the scene:

```csharp
// Get a reference to a specific game system
EntitySystem entitySystem = GetGameSystem<EntitySystem>();
PhysicsEngine physicsEngine = GetGameSystem<PhysicsEngine>();

// Use the system
Entity entity = entitySystem.CreateEntity<YourEntity>(new Vector2(100, 100));

// Access the MainGame instance from any game system
public class YourGameSystem : GameSystem
{
    public void SomeMethod()
    {
        // Access the Game property to get the MainGame instance
        MainGame game = Game;
        
        // Use it to access MonoGame resources
        ContentManager content = game.Content;
        GraphicsDevice graphics = game.GraphicsDevice;
        
        // Or to access application-wide resources
        var screenWidth = game.Graphics.PreferredBackBufferWidth;
        var screenHeight = game.Graphics.PreferredBackBufferHeight;
    }
}
```

## Communication Between Systems

Game systems can communicate with each other through the scene:

```csharp
public class YourGameSystem : GameSystem, IUpdateGameSystem
{
    public void Update(GameTime gameTime)
    {
        // Get another system
        EntitySystem entitySystem = GetGameSystem<EntitySystem>();
        
        // Interact with it
        var entities = entitySystem.FindByType<YourEntity>();
        // Do something with the entities
    }
}
```

## Built-in Game Systems

CoreEssentials-MonoGame includes several built-in game systems:

### EntitySystem

Manages game objects with component-based behavior:

```csharp
// Create entities
EntitySystem entitySystem = GetGameSystem<EntitySystem>();
YourEntity entity = entitySystem.CreateEntity<YourEntity>(new Vector2(100, 100));

// Query entities
List<YourEntity> entities = entitySystem.FindByType<YourEntity>();

// Count entities
int entityCount = entitySystem.GetEntities().Count;
```

### PhysicsEngine

Provides 2D physics simulation using Aether.Physics2D:

```csharp
PhysicsEngine physics = GetGameSystem<PhysicsEngine>();

// Create physics bodies
IPhysicsBody dynamicBody = physics.CreateDynamic(position);
dynamicBody.CreateCircleCollider(radius);

IPhysicsBody staticBody = physics.CreateStatic(position);
staticBody.CreateRectangleCollider(new Vector2(width, height));
```

### PhysicsDebugRenderer

Visualizes physics objects for debugging:

```csharp
PhysicsEngine physics = new PhysicsEngine();
PhysicsDebugRenderer debugRenderer = new PhysicsDebugRenderer(physics);

// Toggle visibility
debugRenderer.IsEnabled = true;
```

### GUIManager

Manages user interface elements (static API):

```csharp
// Initialize once with the game and canvas size
GUIManager.Init(game, width, height);

// Add and remove widgets
GUIManager.AddWidget(yourWidget);
GUIManager.RemoveWidget(yourWidget);

// Draw is called automatically by the game loop
```

## Creating Custom Game Systems

You can create custom game systems to encapsulate specific functionality:

```csharp
public class AISystem : GameSystem, IUpdateGameSystem
{
    private List<AIAgent> _agents = new List<AIAgent>();
    
    public override void OnStart()
    {
        base.OnStart();
        // Set up the AI system
    }
    
    public void Update(GameTime gameTime)
    {
        // Update all AI agents
        foreach (var agent in _agents)
        {
            agent.Update(gameTime);
        }
    }
    
    public void RegisterAgent(AIAgent agent)
    {
        _agents.Add(agent);
    }
    
    public void UnregisterAgent(AIAgent agent)
    {
        _agents.Remove(agent);
    }
}
```

## Example from Playground

The `PhysicsEntityScene` demonstrates the use of multiple game systems:

```csharp
// From PhysicsEntityScene.cs
protected override GameSystem[] LoadGameSystems()
{
    PhysicsEngine physicsEngine = new PhysicsEngine();
    PhysicsDebugRenderer physicsDebugRenderer = new PhysicsDebugRenderer(physicsEngine);
    EntitySystem entitySystem = new EntitySystem();

    GameSystem[] systems = new GameSystem[]
    {
        physicsEngine,
        entitySystem,
        physicsDebugRenderer,
    };

    return systems;
}
```

## Best Practices

- Create specialized systems for distinct areas of functionality
- Keep systems focused on a single responsibility
- Use systems to decouple different aspects of your game
- Register only the systems needed for each scene
- Consider the update order of systems when designing interactions
- Access other systems through the Scene reference when needed
- Use proper cleanup in system disposal to prevent memory leaks