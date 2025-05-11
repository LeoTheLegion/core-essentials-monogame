# Debugging Tools

CoreEssentials-MonoGame provides a comprehensive suite of debugging tools to help you develop, test, and troubleshoot your games. These tools include logging capabilities, visual debugging aids, and diagnostic information displays.

## StickyLog

The `StickyLog` feature allows you to display persistent information on the screen, ideal for showing FPS, entity counts, or other stats. It uses the Canvas system to position and manage UI elements.

```csharp
// Create a sticky log entry that persists across frames
Debug.StickyLog.Log("FPS", $"{1 / gameTime.ElapsedGameTime.TotalSeconds:F1}");
Debug.StickyLog.Log("Entities", $"{entitySystem.EntityCount}");
Debug.StickyLog.Log("Memory", $"{GC.GetTotalMemory(false) / 1024 / 1024:F1} MB");

// Toggle visibility
Debug.StickyLog.IsVisible = !Debug.StickyLog.IsVisible;

// By default, StickyLog visibility can be toggled with the R key
```

The StickyLog is built on top of the Canvas system, which handles positioning and updating of UI elements. It's automatically updated in the game's main update loop.

### Positioning

StickyLog positions itself at the top-left corner of the screen by default, but you can customize its position:

```csharp
// Custom positioning (must be done after LoadGUI has been called)
// This will position the StickyLog at the top-right corner with 10px margin
var screenWidth = GraphicsDevice.Viewport.Width;
Debug.StickyLog.SetPosition(new Vector2(screenWidth - 210, 10));
```

### Managing Entries

You can remove specific entries or clear all entries:

```csharp
// Remove a specific entry by key
Debug.StickyLog.Remove("FPS");

// Clear all entries
Debug.StickyLog.Clear();
```

### Update Cycle

StickyLog needs to be updated each frame, but this is already handled in the MainGame class:

```csharp
// This is already implemented in MainGame.Update
Debug.StickyLog.Update(gameTime);
```

## Physics Debug Renderer

When working with the physics system, the `PhysicsDebugRenderer` visualizes physics bodies, joints, and contacts:

```csharp
// Set up the physics debug renderer in your scene
protected override GameSystem[] LoadGameSystems()
{
    PhysicsEngine physicsEngine = new PhysicsEngine();
    PhysicsDebugRenderer debugRenderer = new PhysicsDebugRenderer(physicsEngine);
    
    return new GameSystem[]
    {
        physicsEngine,
        debugRenderer
    };
}

// Toggle debug rendering visibility
PhysicsDebugRenderer debugRenderer = GetGameSystem<PhysicsDebugRenderer>();
debugRenderer.IsVisible = !debugRenderer.IsVisible;
```

## Primitive Drawing

The `Primitives` class allows you to draw simple shapes for debugging purposes:

```csharp
// Draw a debug circle
Primitives.DrawCircle(spriteBatch, position, radius, Color.Red);

// Draw a debug rectangle
Primitives.DrawRectangle(spriteBatch, new Rectangle(x, y, width, height), Color.Yellow);

// Draw a debug line
Primitives.DrawLine(spriteBatch, startPoint, endPoint, Color.Blue);

// Draw debug text
Primitives.DrawString(spriteBatch, "Debug Message", position, Color.White);
```

## Game Diagnostics

The `BaseGameDiagnostics` class provides performance monitoring and diagnostic information:

```csharp
// Create a diagnostics instance in your game
private BaseGameDiagnostics _diagnostics;

protected override void Initialize()
{
    base.Initialize();
    _diagnostics = new BaseGameDiagnostics(this);
}

protected override void Update(GameTime gameTime)
{
    base.Update(gameTime);
    _diagnostics.Update(gameTime);
}

protected override void Draw(GameTime gameTime)
{
    base.Draw(gameTime);
    _diagnostics.Draw(gameTime);
}
```

## Debug Shortcuts

You can set up keyboard shortcuts to toggle debugging features:

```csharp
protected override IEnumerator OnStartCoroutine()
{
    // Set up debug keyboard shortcuts
    Input.Keyboard.KeyReleased += ToggleDebugFeatures();
    
    yield return null;
}

private EventHandler<KeyboardEventArgs> ToggleDebugFeatures()
{
    return (sender, args) =>
    {
        if (args.Key == Keys.F1)
        {
            // Toggle console visibility
            Debug.Console.IsVisible = !Debug.Console.IsVisible;
        }
        
        if (args.Key == Keys.F2)
        {
            // Toggle physics debug rendering
            PhysicsDebugRenderer debugRenderer = GetGameSystem<PhysicsDebugRenderer>();
            if (debugRenderer != null)
                debugRenderer.IsVisible = !debugRenderer.IsVisible;
        }
        
        if (args.Key == Keys.F3)
        {
            // Toggle StickyLog visibility
            StickyLog.IsVisible = !StickyLog.IsVisible;
        }
    };
}
```

## Example from Playground

Debug tools are used throughout the Playground examples:

```csharp
// From PhysicsEntityScene
Console.WriteLine("Physics entity scene initialization complete!");

// From CharacterScene
Console.WriteLine("Character scene loaded successfully!");
Console.WriteLine($"Sound played with ID: {id}");
```

## Best Practices

- Use appropriate log categories for better filtering
- Implement debug-only rendering that can be disabled in release builds
- Use StickyLog for persistent information that should be visible during gameplay
- Create dedicated keyboard shortcuts for toggling debug features
- Clean up debug visualizations in release builds
- Consider different colors for different types of debug information
- Group related debug information for better readability
- Use consistent naming for debug elements