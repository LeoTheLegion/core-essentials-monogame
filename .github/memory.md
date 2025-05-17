# Core Essentials MonoGame - Development Notes

## Game Access in Systems

Game systems can access the main Game instance through a property chain:

1. `GameSystem.Game` property provides access to the `MainGame` instance
2. Internally, this follows the path: `GameSystem -> Scene -> SceneManager -> MainGame` 
3. The scene reference is automatically set by the Scene when registering game systems

This is useful for systems that need direct access to game resources (Content, GraphicsDevice, etc.)
without having to explicitly pass references around.

## Running Tests

To run all tests in the solution:
```bash
dotnet test
```

To run tests for a specific project:
```bash
dotnet test CoreEssentials.Tests/CoreEssentials.Tests.csproj
```

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