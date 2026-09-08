# Scene Manifest

The scene manifest is a single XML file that declares, up front, what your game contains: which scenes exist, their order, and which loading screens are available. It follows the code-as-data principle — scenes are data, so *the list of scenes* is data too. The core enforces it: `SceneManager` cannot load a scene that is not registered in the manifest (see [Scene Management](SceneManagement.md)).

## File Location & Name

The playground looks for `scenes.xml` in `Content/`. If the file is missing or cannot be parsed, boot errors out — there is no fallback to discovering scenes by scanning the folder.

## Format

```xml
<!-- CoreEssentials.Playground/Content/scenes.xml -->
<Scenes>
    <GameScenes>
        <Scene Name="HomeScene.xml" />
        <Scene Name="CharacterScene.xml" LoadingScreen="loading_main.xml" />
        <Scene Name="CameraScene.xml" />
        <Scene Name="PhysicsEntityScene.xml" LoadingScreen="loading_physics.xml" />
    </GameScenes>

    <LoadingScenes>
        <LoadingScene Name="loading_main.xml" Default="true" />
        <LoadingScene Name="loading_physics.xml" />
    </LoadingScenes>
</Scenes>
```

## Elements & Attributes

| Element | Attribute | Required | Description |
|---------|-----------|----------|-------------|
| `<GameScenes>` | — | Yes | Ordered list of the game's scenes. Position is navigation order; **the first entry is the startup scene**. Must contain at least one `<Scene>`. |
| `<Scene>` | `Name` | Yes | The scene asset name (e.g. `HomeScene.xml`). Must be unique within the list. |
| `<Scene>` | `LoadingScreen` | No | A loading screen to show when transitioning *into* this scene. Must reference a name declared in `<LoadingScenes>`. Falls back to the default loading screen when omitted. |
| `<LoadingScenes>` | — | No | Registry of available loading screens. May be omitted entirely (a game with no loading screens). |
| `<LoadingScene>` | `Name` | Yes | The loading-screen asset name (e.g. `loading.xml`). Must be unique within the list. |
| `<LoadingScene>` | `Default` | No | `true` marks this as the default loading screen used when a game scene does not name one. At most one entry may be marked default. |

## Rules

- **Startup scene** = the first `<Scene>` in `<GameScenes>`. There is no separate startup attribute — reordering the list changes what boots.
- **Navigation** — `SceneManager.NextScene()` / `PreviousScene()` move ±1 through `<GameScenes>` and clamp at the ends (see [Navigation](SceneManagement.md#navigation)).
- **Loading screen for a transition into scene X** = X's `LoadingScreen` attribute, else the `Default="true"` entry in `<LoadingScenes>`, else none.

## Validation Errors

The parser is strict: unknown elements or attributes are errors that name the offender, so typos fail fast instead of being silently ignored.

| Condition | Result |
|-----------|--------|
| Malformed XML | `FormatException` — "Scene manifest XML is malformed" |
| Root element is not `<Scenes>` | `FormatException` naming the found element |
| Missing or empty `<GameScenes>` | `FormatException` |
| Duplicate `<Scene Name>` within the list | `FormatException` naming the scene |
| Duplicate `<LoadingScene Name>` within the registry | `FormatException` naming the screen |
| `<Scene LoadingScreen="...">` referencing an undeclared loading screen | `FormatException` naming both the scene and the missing screen |
| More than one `Default="true"` loading screen | `FormatException` naming the existing default |
| Unknown element under `<Scenes>` or inside either list | `FormatException` naming the element |

## Example: Minimal Game

A game with three levels and one shared loading screen:

```xml
<Scenes>
    <GameScenes>
        <Scene Name="MenuScene.xml" />
        <Scene Name="Level01.xml" />
        <Scene Name="Level02.xml" />
    </GameScenes>
    <LoadingScenes>
        <LoadingScene Name="loading.xml" Default="true" />
    </LoadingScenes>
</Scenes>
```

`MenuScene.xml` boots first; every transition shows `loading.xml`; `SceneManager.NextScene()` from `Level01.xml` loads `Level02.xml`.
