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

### Transition lifecycle & canvas registration

When a loading screen is set (`SetLoadingScene`), a transition runs this sequence:

1. The current scene is unloaded.
2. The **loading screen** is loaded and becomes the active scene — its label/progress cover the screen.
3. The **target scene** loads in the background while the loading screen stays on top.
4. Once the target finishes loading, the loading screen is **unloaded** and the target becomes current.

The loading screen is retained (not discarded) so it can be reloaded cheaply on the next transition — but it is *unloaded* after each swap rather than left loaded. This matters because of how canvases register:

- A canvas joins the **global GUI render list** only when its owning scene first pumps it (its first `Update`), and leaves when the scene unloads (the canvas's `CleanUp`).
- Because a still-loading target scene is not yet current, its components are never pumped, so **its canvases do not register while it loads** — the new scene cannot show through during the load.
- Because the loading screen is unloaded after the swap, **its canvas detaches** and stops rendering on top of the new scene.

This is why neither a black-box overlay nor manual hide/show bookkeeping is needed: the loading screen's own label covers the screen for exactly as long as it should.

## Data-Driven Scenes

A scene can run entirely from a data file — no C# subclass needed. `SceneManager.LoadScene("MyScene.xml")` parses the file into a `DataDrivenScene`, which reflects its game systems, registers prefabs, and instantiates entities. The same file can also serve as the loading screen:

```csharp
// Configure the scene manifest first (see below), then load by name.
SceneManager.SetManifestAsset("scenes.xml");   // or SetManifest(parsed)
SceneManager.SetLoadingScene("loading.xml");   // data-driven loading screen
SceneManager.LoadScene("MainMenu.xml");        // data-driven scene
```

The full strict schema — `Type=` vs `Source=`, flat/precise/entity overrides, binds, references, the data-driven loading screen with `TransitionProgressComponent`, and the 0.19 → 0.20 breaking changes — is documented in [Scene-as-Data](./SceneAsData.md).

### File structure

```xml
<Scene>
    <GameSystems>
        <System Type="EntitySystem">
            <Prefabs>
                <Prefab Name="Ball" Asset="BallTemplate.xml" />
            </Prefabs>
            <Entities>
                <EntityDefinition Source="Ball" Id="ball1">
                    <Position X="100" Y="200" />
                </EntityDefinition>
            </Entities>
        </System>
    </GameSystems>
</Scene>
```

Rules:

- `<Scene>` contains exactly one `<GameSystems>` element, which holds one or more `<System>` entries in document order (systems are created in that order).
- Only an `EntitySystem` may declare `<Prefabs>` and `<Entities>`; all other systems must be self-closing.
- Unknown elements and attributes are rejected at parse time.

## Scene Manifest

A data-driven game declares its scenes up front in a **scene manifest** — Unity's "Scenes In Build Settings", as data. The manifest is the single source of truth for which scenes exist, their order, and which loading screens are available, and **the core enforces it**: `SceneManager.LoadScene(name)` / `SetLoadingScene(name)` refuse to load anything that is not registered, and a missing or malformed manifest errors out (no glob fallback).

### Format

```xml
<!-- Content/scenes.xml -->
<Scenes>
    <GameScenes>
        <Scene Name="HomeScene.xml" />
        <Scene Name="CharacterScene.xml" LoadingScreen="loading_main.xml" />
    </GameScenes>
    <LoadingScenes>
        <LoadingScene Name="loading_main.xml" Default="true" />
        <LoadingScene Name="loading_physics.xml" />
    </LoadingScenes>
</Scenes>
```

- **Startup scene** = the first `<Scene>` in `<GameScenes>`. An empty list is a boot error.
- **Per-scene loading screen** — transitioning into scene X uses X's `LoadingScreen` attribute, else the `Default="true"` entry, else no loading screen (direct transition).
- `<LoadingScenes>` is optional. Parsing is strict: unknown elements/attributes, duplicate names, a `LoadingScreen` referencing an undeclared loading screen, or multiple defaults all throw `FormatException` naming the offender.

Full format reference and validation-error table: [SceneManifest.md](./SceneManifest.md).

### Configuring it

```csharp
// Deferred (recommended for data-driven games): register by asset name; the file is read
// and parsed on the first transition, once the AssetManager is initialized. Can be called
// right after game construction, ahead of Run().
game.SceneManager.SetManifestAsset("scenes.xml");

// Or supply a parsed manifest (e.g. from an embedded string) — available immediately.
game.SceneManager.SetManifest(SceneManifest.Parse(xml));
```

### Enforcement errors

| Situation | Result |
|-----------|--------|
| Name-based load with no manifest configured | `InvalidOperationException` thrown synchronously at the call site. |
| Manifest asset missing/unreadable or malformed | The transition coroutine is unfailable, so the parse/validation failure propagates and the process exits non-zero. |
| Scene name not in `<GameScenes>` | `InvalidOperationException` naming the offending scene and the registered list; the process exits non-zero. |
| Loading-screen name not in `<LoadingScenes>` | Rejected at manifest parse time (`FormatException`). |

Object-based loads — `LoadScene(Scene)` / `SetLoadingScene(Scene)` — carry no asset name and remain an **escape hatch** for scenes that are not part of the manifest (e.g. tests, debug overlays).

### Navigation: NextScene() / PreviousScene()

With an ordered list, "next" and "back" have a well-defined meaning. The navigation API lives on `SceneManager` itself — it already holds the manifest and knows the current scene:

```csharp
// Advance one entry in <GameScenes> (clamped at the end), transitioning via the normal path.
game.SceneManager.NextScene();

// Retreat one entry (clamped at the start).
game.SceneManager.PreviousScene();

// Optional: react to completed navigation without polling.
game.SceneManager.SceneAdvanced += sceneName => Console.WriteLine($"Now in {sceneName}");
game.SceneManager.SceneRetreated += sceneName => Console.WriteLine($"Back to {sceneName}");
```

Behavior:

- **Clamped** — `NextScene()` on the last scene (or `PreviousScene()` on the first) is a no-op with a console note.
- **Normal transition path** — navigation reuses the standard transition, so per-scene loading screens apply exactly as for an explicit load.
- **Untracked current scene** — if the current scene is not a registered entry (e.g. loaded object-based), navigation is a no-op with a console note until a listed scene becomes current.
- **While transitioning** — navigation during an in-progress transition is ignored.
- **Events** — `SceneAdvanced` / `SceneRetreated` fire once the new scene has actually been swapped in, passing its asset name (or type name for unnamed scenes).

Use cases: level progression, menu Back buttons, tutorials, cutscene chains, attract-mode cycling. Deliberately thin in v1 — no history stack, branching, or gating predicates.

### Migration notes (library consumers)

Upgrading to a manifest-enforcing version is a **breaking change** for data-driven games:

1. Create `scenes.xml` listing every scene you load by name (first entry = startup) and any loading screens.
2. Register it before your first name-based load: `SceneManager.SetManifestAsset("scenes.xml")`.
3. Anything still loaded object-based (`LoadScene(new MyScene())`) keeps working unchanged.

The smoke-run harness (`scripts/run-all-scenes.ps1`) reads its scene list from the manifest too — a missing manifest aborts it, and unregistered `<Scene>` files on disk are surfaced as a warning.

### System attributes

| Attribute | Type | Required | Description |
|-----------|------|----------|-------------|
| `Type` | string | **Yes** | Built-in name (`EntitySystem`, `PhysicsEngine`) or a class name resolvable in a loaded assembly (must derive from `GameSystem`). |
| `Config` | string | No | Name of an XML configuration asset the system is created from. The system must expose exactly one public single-argument constructor whose parameter type is a known configuration type (`PhysicsConfig` today). |

Without `Config`, the system is created through its **public parameterless constructor**. Systems that need sibling systems at construction time — e.g. `PhysicsDebugRenderer`, which resolves the scene's `PhysicsEngine` lazily on first draw — work with the parameterless form:

```xml
<GameSystems>
    <System Type="PhysicsEngine" Config="PhysicsConfig.xml" />
    <System Type="EntitySystem">
        <!-- ... -->
    </System>
    <System Type="PhysicsDebugRenderer" />
</GameSystems>
```

### Nested child positions

A nested `<Children>` entry's `<Position>` is an **offset from its parent**, not a world position. A child without a `<Position>` sits at the parent's position (zero local offset). World position of a child is `parent.Position + child.LocalPosition`.

## Scene Transitions

The framework handles scene transitions automatically. When loading a new scene:

1. The current scene is unloaded (if any)
2. The new scene's `LoadGameSystems` method is called to instantiate game systems.
3. Each registered game system has its `SetScene` method called.
4. After all systems are registered, each game system's `OnStart()` virtual method is called. This is the ideal place for systems to perform initialization that might depend on other systems being available (e.g., fetching a reference to another system using `Scene.GetGameSystem<T>()`).
5. The new scene's `OnStartCoroutine` is executed, showing loading progress. This coroutine is for scene-specific asynchronous setup.
6. When loading is complete, the scene becomes active

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