# Scene Management

The Scene Management system in CoreEssentials-MonoGame provides a structured way to organize your game into discrete scenes that can be loaded, unloaded, and transitioned between.

## Key Components

### SceneManager

The `SceneManager` class is responsible for handling scene transitions and maintaining the currently active scene.

```csharp
// Load a new scene with a transition
SceneManager.LoadScene(new YourGameScene());

// Access the currently active scene
Scene currentScene = SceneManager.CurrentScene;

// Access the game instance
Game gameInstance = SceneManager.Game;
```

### Scene Class

The `Scene` abstract class serves as the base for all your game scenes. Override its methods to implement your scene's behavior.

```csharp
public class MyGameScene : Scene
{
    // Load game systems needed for this scene
    protected override GameSystem[] LoadGameSystems()
    {
        return new GameSystem[]
        {
            new EntitySystem(),
            new PhysicsEngine()
        };
    }
    
    // Set up the scene using coroutines (allows for async operations)
    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.1f, "Initializing scene...");
        
        // Create entities, set up the scene
        EntitySystem entitySystem = GetGameSystem<EntitySystem>();
        entitySystem.CreateEntity<YourEntity>(new Vector2(100, 100));
        
        UpdateLoadingProgress(1.0f, "Scene ready!");
        yield return null;
    }
    
    // Clean up resources when scene is unloaded
    public override void Unload()
    {
        base.Unload();
        // Unregister event handlers, dispose resources
    }
}
```

## Loading Screen

Scenes can display loading progress using the `UpdateLoadingProgress` method:

```csharp
// Update loading progress (0.0f - 1.0f) with an optional status message
UpdateLoadingProgress(0.5f, "Loading assets...");
```

## Scene Transitions

The framework handles scene transitions automatically. When loading a new scene:

1. The current scene is unloaded (if any)
2. The new scene's `LoadGameSystems` method is called
3. The new scene's `OnStartCoroutine` is executed, showing loading progress
4. When loading is complete, the scene becomes active

## Example from Playground

The `PhysicsEntityScene` demonstrates effective scene management:

```csharp
// Inside PhysicsEntityScene.cs
protected override IEnumerator OnStartCoroutine()
{
    UpdateLoadingProgress(0.5f, "Initializing physics scene...");
    yield return new WaitForSeconds(0.2f);
    
    // Scene setup code...
    
    // Switch scenes on key press
    Input.Keyboard.KeyReleased += (sender, args) =>
    {
        if (args.Key == Keys.Right)
        {
            SceneManager.LoadScene(new CharacterScene());
        }
    };
    
    UpdateLoadingProgress(1.0f, "Scene ready!");
}
```

## Best Practices

- Use `OnStartCoroutine` for time-consuming initialization tasks
- Update loading progress regularly to provide feedback
- Clean up resources in the `Unload` method
- Use `GetGameSystem<T>()` to access registered game systems
- Register and unregister event handlers appropriately