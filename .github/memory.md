# Core Essentials MonoGame - Development Notes

## Game Access in Systems

Game systems can access the main Game instance through a property chain:

1. `GameSystem.Game` property provides access to the `MainGame` instance
2. Internally, this follows the path: `GameSystem -> Scene -> SceneManager -> MainGame` 
3. The scene reference is automatically set by the Scene when registering game systems

This is useful for systems that need direct access to game resources (Content, GraphicsDevice, etc.)
without having to explicitly pass references around.

## Physics Configuration

The PhysicsEngine supports runtime configuration of solver settings through the `Config` property:

### Recommended Approach (New)
```csharp
var physicsEngine = new PhysicsEngine();
physicsEngine.Config.Scale = 100;  // Set scale via Config
physicsEngine.Config.VelocityIterations = 8;
physicsEngine.Config.PositionIterations = 3;
physicsEngine.Config.ContinuousPhysics = true;
```

### Legacy Approach (Deprecated)
```csharp
// The following are deprecated and will be removed in a future version:
var physicsEngine = new PhysicsEngine(scale: 100);  // Obsolete constructor
physicsEngine.SetScale(100);  // Obsolete method
int scale = physicsEngine.Scale;  // Obsolete property
```

### Default Settings (Balanced)
```csharp
var physicsEngine = new PhysicsEngine();
physicsEngine.Config.Scale = 100;
// Defaults: Scale=0, VelocityIterations=8, PositionIterations=3, ContinuousPhysics=true
```

### Particle Systems (1000+ bodies) - Performance Optimized
```csharp
var physicsEngine = new PhysicsEngine();
physicsEngine.Config.Scale = 100;
physicsEngine.Config.VelocityIterations = 4;      // Lower for speed
physicsEngine.Config.PositionIterations = 2;       // Lower for speed
physicsEngine.Config.ContinuousPhysics = false;    // Disable CCD
// Expected: 40-60% FPS improvement
```

### Precision Stacking - Accuracy Optimized
```csharp
var physicsEngine = new PhysicsEngine();
physicsEngine.Config.Scale = 100;
physicsEngine.Config.VelocityIterations = 10;     // Higher for accuracy
physicsEngine.Config.PositionIterations = 4;      // Higher for accuracy
physicsEngine.Config.ContinuousPhysics = true;    // Prevent tunneling
// Result: More stable but slower
```

**Note:** `ContinuousPhysics` is a global setting in Aether Physics2D. Multiple PhysicsEngine instances will share this setting.

## Running Tests

To run all tests in the solution:
```bash
dotnet test
```

To run tests for a specific project:
```bash
dotnet test CoreEssentials.Tests/CoreEssentials.Tests.csproj
```

**Note:** Tests require Windows desktop framework to run due to MonoGame dependencies. On Linux, you can build the test project with `-p:EnableWindowsTargeting=true`, but tests cannot be executed.

## Debugging Tips

When the Game property returns null in systems, check:
1. Is the system properly registered with a Scene?
2. Has the Scene been properly initialized with a SceneManager?
3. Is the SceneManager's Game property set correctly?

## Development Workflow

When making changes to the codebase, follow this workflow:

1. Create a new feature branch: `git checkout -b feature/xxx`
2. Make your changes with proper tests and documentation
3. Commit your changes: `git commit -m "Descriptive message"`
4. Push your changes to the remote repository: `git push -u origin feature/xxx`
5. Create a pull request on GitHub

Remember to always push your changes to the remote repository after committing. This ensures your 
work is backed up and available to other developers on the team.