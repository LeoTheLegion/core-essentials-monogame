# Scene Manifest — Scrum Sprints 🗂️

Declare a game's scenes up front in a single XML file — Unity-style "Scenes In Build" — and get ordered navigation (`Next()`/`Previous()`) for free. The manifest is the **single source of truth** for what the game contains: which scenes exist, their order, and which loading screens are available.

> **Stacks on `feature/scene-as-data`.** All three sprints land on a single branch: `feature/scene-manifest`. Follows the code-as-data principle: the **core enforces** the manifest. `SceneManager` cannot load a scene that isn't registered in it, and if the manifest file is not provided or cannot be resolved, boot **errors out** — there is no glob fallback.

## The Format

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

**Rules:**
- **Startup scene** = the first `<Scene>` in `<GameScenes>` (Unity's index 0). An empty list is a boot error.
- **Navigation** — `Next()`/`Previous()` move ±1 through `<GameScenes>`; they clamp at the ends.
- **Loading screen for a transition into scene X** = X's `LoadingScreen` attribute, else the `Default="true"` entry in `<LoadingScenes>`, else none.
- **Validation (fail fast)** — duplicate names within either list, first scene missing from disk, or a `LoadingScreen` attribute referencing an undeclared loading screen → boot error. Scene files in `Content/` not listed anywhere → console warning only.
- **Enforced by the core** — `SceneManager.LoadScene(name)` throws if `name` is not in `<GameScenes>`; `SetLoadingScene(name)` throws if not in `<LoadingScenes>`. The manifest is resolved after `AssetManager` init (deferred, like scene XML). A missing or malformed file is a hard error (non-zero exit), so the smoke-run harness reports it as FAIL.

## Sprint Roadmap

| Sprint | Name | Points | Status | Description |
|--------|------|--------|--------|-------------|
| 1 | [Scene Manifest Parsing](Sprint_1_Scene_Manifest_Parsing.md) | 3 | ✅ Done (2026-09-04) | `SceneManifest`: parse + validate the two-list format, pure data, no behavior change |
| 2 | [Core Enforcement + Migration](Sprint_2_Core_Enforcement_And_Migration.md) | 7 | ✅ Done (2026-09-04) | Core enforces the manifest (unregistered scenes can't load); per-scene loading screens; full test/playground migration |
| 3 | [Navigation API + Tooling](Sprint_3_Navigation_API_And_Tooling.md) | 5 | ⬜ Not started | `SceneManager.NextScene()`/`PreviousScene()` over the manifest (clamped, with events); harness reads the manifest as the authoritative list; docs |

## Why This?

Today "what scenes are in my game" is answered by globbing `Content/*.xml` — implicit, unordered, and the startup scene (`HomeScene.xml`) plus the loading screen (`loading.xml`) are hardcoded strings in `Program.cs`. An explicit manifest gives:

- **Deterministic contents** — you control exactly what ships; forgetting to register a new scene is caught at boot (Unity's Build Settings lesson).
- **Ordered navigation** — "next level" / "back" become index moves on the list instead of hand-written `LoadScene("Level02")` call sites. Use cases: level progression, menu Back buttons, tutorials, cutscene chains, attract-mode cycling.
- **Code-as-data consistency** — scenes are data; the list of scenes is data too. No file → error out, loudly.

## Point Summary

- Sizing: 1 = small, 2 = medium, 5 = large
