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
    
    // Called when the system is initialized by the Scene
    // This is now primarily for internal setup before OnStart
    // public override void Initialize() // This method might be obsolete or repurposed
    // {
    //     base.Initialize();
    //     // Initialize your system
    // }

    /// <summary>
    /// Called after all game systems in a scene have been loaded and registered.
    /// Override this method to perform any setup that requires access to other
    /// game systems or when all systems are guaranteed to be available.
    /// </summary>
    public virtual void OnStart()
    {
        // Default implementation does nothing.
        // Example: OtherSystem otherSystem = Scene.GetGameSystem<OtherSystem>();
    }

    // Called every frame for logic updates
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        // Update your system
    }

    // Called every frame for rendering
    public override void Draw(GameTime gameTime)
    {
        base.Draw(gameTime);
        // Draw your system
    }
}
```

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
public class YourGameSystem : GameSystem
{
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        // Get another system
        EntitySystem entitySystem = Scene.GetGameSystem<EntitySystem>();
        
        // Interact with it
        if (entitySystem != null)
        {
            var entities = entitySystem.GetEntitiesOfType<YourEntity>();
            // Do something with the entities
        }
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
IEnumerable<YourEntity> entities = entitySystem.GetEntitiesOfType<YourEntity>();

// Count entities
int entityCount = entitySystem.EntityCount;
```

### PhysicsEngine

Provides 2D physics simulation using Aether.Physics2D:

```csharp
PhysicsEngine physics = GetGameSystem<PhysicsEngine>();

// Create physics bodies
Body circleBody = physics.CreateCircle(position, radius, density);
Body rectangleBody = physics.CreateRectangle(position, width, height, density);
```

### PhysicsDebugRenderer

Visualizes physics objects for debugging:

```csharp
PhysicsEngine physics = new PhysicsEngine();
PhysicsDebugRenderer debugRenderer = new PhysicsDebugRenderer(physics);

// Toggle visibility
debugRenderer.IsVisible = true;
```

### GUIManager

Manages user interface elements:

```csharp
GUIManager guiManager = GetGameSystem<GUIManager>();

// Set up a UI desktop
Desktop desktop = new Desktop();
desktop.Root = yourRootUIElement;
guiManager.SetDesktop(desktop);
```

## Creating Custom Game Systems

You can create custom game systems to encapsulate specific functionality:

```csharp
public class AISystem : GameSystem
{
    private List<AIAgent> _agents = new List<AIAgent>();
    
    public override void Initialize()
    {
        base.Initialize();
        // Set up the AI system
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
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