using CoreEssentials.Assets;
using CoreEssentials.Coroutines;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;
using System.Linq;

namespace CoreEssentials.Scenes;
/// <summary>
/// The SceneManager is responsible for managing the current scene and transitioning between scenes.
/// </summary>
public class SceneManager
{
    /// <summary>
    /// Reference to the MainGame instance that this SceneManager is associated with.
    /// </summary>
    readonly MainGame? _game;
    /// <summary>
    /// The current active scene.
    /// </summary>
    Scene? _currentScene;
    /// <summary>
    /// The next scene to be loaded.
    /// </summary>
    Scene? _nextScene;
    /// <summary>
    /// The loading screen scene used during transitions.
    /// </summary>
    Scene? _loadingScene;
    /// <summary>
    /// CoroutineOwner for managing scene transition coroutines.
    /// </summary>
    private readonly CoroutineOwner _coroutineOwner;
    /// <summary>
    /// Tracks the current transition coroutine ID.
    /// </summary>
    private Guid _transitionCoroutineId;
    /// <summary>
    /// Flag indicating whether a transition is in progress.
    /// </summary>
    private bool _isTransitioning;

    /// <summary>
    /// The scene manifest that names the scenes this game may load by name, or null when none is set.
    /// When provided synchronously via <see cref="SetManifest"/>, it is validated immediately.
    /// </summary>
    private SceneManifest? _manifest;

    /// <summary>
    /// The scene manifest XML asset to resolve lazily (after the AssetManager is initialized) when the
    /// manifest was registered by asset name via <see cref="SetManifestAsset"/>. Null when a parsed
    /// manifest was supplied directly or no manifest is set.
    /// </summary>
    private string? _manifestAssetName;

    /// <summary>
    /// The scene manifest this manager enforces, or null when none has been provided. Name-based loads
    /// (<see cref="LoadScene(string)"/> / <see cref="SetLoadingScene(string)"/>) require a manifest.
    /// </summary>
    public SceneManifest? Manifest => _manifest;

    /// <summary>
    /// Gets the scene XML asset name this manifest was registered from, or null.
    /// </summary>
    public string? ManifestAssetName => _manifestAssetName;

    /// <summary>
    /// Gets the MainGame instance associated with this SceneManager.
    /// </summary>
    /// <returns>The MainGame instance.</returns>
    public MainGame? Game => _game;

    /// <summary>
    /// Gets the current active scene.
    /// </summary>
    /// <returns>The current scene.</returns>
    public Scene? CurrentScene => _currentScene;
    
    /// <summary>
    /// Gets the next scene to be loaded.
    /// </summary>
    /// <returns>The next scene.</returns>
    public Scene? NextScene => _nextScene;

    /// <summary>
    /// Gets the loading screen scene used during transitions, or null when none is set.
    /// The loading screen stays loaded after a transition so it can be reused for the next one.
    /// </summary>
    public Scene? LoadingScene => _loadingScene;
    
    /// <summary>
    /// Gets whether a scene transition is in progress.
    /// </summary>
    /// <returns>True if a scene is currently loading; otherwise, false.</returns>
    public bool IsTransitioning => _isTransitioning;
    
    /// <summary>
    /// Gets the loading progress of the transition to the next scene (0.0 to 1.0).
    /// While the next scene is still loading this mirrors its loading progress; once it has
    /// finished loading but not yet been switched in, the load work is complete, so this
    /// reports 1.0 for the final frame before the swap. Returns 0 when no transition is
    /// happening (no next scene).
    /// </summary>
    public float TransitionProgress => _nextScene == null ? 0f : (_nextScene.IsLoading ? _nextScene.LoadingProgress : 1f);
    
    /// <summary>
    /// Initializes a new instance of the SceneManager class with the specified MainGame instance.
    /// </summary>
    /// <param name="game">The MainGame instance to associate with this SceneManager.</param>
    public SceneManager(MainGame game) : this()
    {
        _game = game;
    }

    /// <summary>
    /// Initializes a new instance of the SceneManager class.
    /// </summary>
    public SceneManager()
    {
        _currentScene = null;
        _nextScene = null;
        _loadingScene = null;
        _isTransitioning = false;
        _coroutineOwner = new CoroutineOwner();
    }

    /// <summary>
    /// Sets the scene to be used as a loading screen during transitions. This is the object-based escape
    /// hatch: it sets a concrete loading scene directly and is not subject to manifest enforcement (it has
    /// no asset name to check). Prefer the manifest's <c>&lt;LoadingScenes&gt;</c> for data-driven games.
    /// </summary>
    /// <param name="loadingScene">The loading screen scene.</param>
    public void SetLoadingScene(Scene loadingScene)
    {
        _loadingScene = loadingScene;
        _loadingScene.SetSceneManager(this);
    }

    /// <summary>
    /// Sets a data-driven loading screen from a scene XML asset (see <see cref="SceneParser"/>). Requires a
    /// scene manifest to be configured (<see cref="SetManifest"/> or <see cref="SetManifestAsset"/>); the
    /// screen name must be registered in the manifest's <c>&lt;LoadingScenes&gt;</c>. The file is parsed when
    /// the scene loads, so this can be called before the <see cref="CoreEssentials.Assets.AssetManager"/> is
    /// initialized.
    /// </summary>
    /// <param name="sceneAssetName">The name/key of the loading-screen XML asset (e.g., "loading.xml").</param>
    public void SetLoadingScene(string sceneAssetName)
    {
        EnsureManifestConfigured();
        SetLoadingScene(new DataDrivenScene(sceneAssetName));
    }

    /// <summary>
    /// Loads a data-driven scene from a scene XML asset (see <see cref="SceneParser"/>), wrapping it in a
    /// <see cref="DataDrivenScene"/> and transitioning to it. Requires a scene manifest to be configured
    /// (<see cref="SetManifest"/> or <see cref="SetManifestAsset"/>) and the scene name must be registered in
    /// the manifest's <c>&lt;GameScenes&gt;</c>. The file is parsed when the scene loads, so this can be called
    /// before the <see cref="CoreEssentials.Assets.AssetManager"/> is initialized (e.g. right after game
    /// construction, ahead of <c>Run()</c>).
    /// </summary>
    /// <param name="sceneAssetName">The name/key of the scene XML asset in the AssetManager (e.g., "HomeScene.xml").</param>
    public void LoadScene(string sceneAssetName)
    {
        EnsureManifestConfigured();
        LoadScene(new DataDrivenScene(sceneAssetName));
    }

    /// <summary>
    /// Configures the scene manifest that gates all name-based loads. When supplied parsed, it is used
    /// immediately; a later <see cref="SetManifestAsset"/> call supersedes it.
    /// </summary>
    /// <param name="manifest">The parsed manifest (see <see cref="SceneManifest.Parse"/>).</param>
    public void SetManifest(SceneManifest manifest)
    {
        _manifest = manifest ?? throw new ArgumentNullException(nameof(manifest));
        _manifestAssetName = null;
    }

    /// <summary>
    /// Registers the scene manifest by its XML asset name (e.g. "scenes.xml"). The file is read and parsed
    /// lazily — on the first transition, once the <see cref="CoreEssentials.Assets.AssetManager"/> has been
    /// initialized — so this can be called right after game construction, ahead of <c>Run()</c>. This is the
    /// code-as-data entry point: without a manifest (or an unresolvable one), name-based loads error out.
    /// </summary>
    /// <param name="assetName">The name/key of the scene-manifest XML asset in the AssetManager.</param>
    public void SetManifestAsset(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
            throw new ArgumentNullException(nameof(assetName));
        _manifestAssetName = assetName;
        _manifest = null;
    }

    /// <summary>
    /// Throws when no manifest has been configured at all — i.e. neither a parsed manifest nor a deferred
    /// asset name. Called synchronously by the name-based load overloads so "no manifest provided" fails fast
    /// at the call site rather than deep in the transition coroutine.
    /// </summary>
    private void EnsureManifestConfigured()
    {
        if (_manifest == null && _manifestAssetName == null)
            throw new InvalidOperationException(
                "No scene manifest is configured. Call SetManifest(...) or SetManifestAsset(...) before loading a scene by name.");
    }

    /// <summary>
    /// Resolves the effective manifest, parsing it from its registered asset on first use (deferred until the
    /// <see cref="CoreEssentials.Assets.AssetManager"/> is initialized). Returns null when no manifest is
    /// configured at all. Throws when a deferred asset cannot be read or parsed — this runs inside an
    /// unfailable transition coroutine, so the failure propagates and the game errors out.
    /// </summary>
    private SceneManifest? ResolveManifest()
    {
        if (_manifest != null) return _manifest;
        if (_manifestAssetName == null) return null;

        var xml = AssetManager.LoadAsset<XMLAsset>(_manifestAssetName);
        if (xml.XMLContent == null)
            throw new InvalidOperationException($"Scene manifest asset '{_manifestAssetName}' has no content loaded.");

        _manifest = SceneManifest.Parse(xml.XMLContent);
        Console.WriteLine($"Scene manifest resolved from '{_manifestAssetName}': {string.Join(", ", _manifest.GameScenes.Select(e => e.Name))}");
        return _manifest;
    }

    /// <summary>
    /// Loads the specified scene with a transition.
    /// The transition process is fully handled by coroutines, not in the Update method.
    /// </summary>
    /// <param name="scene">The scene to be loaded.</param>
    public void LoadScene(Scene scene)
    {
        // If a transition is already in progress, don't start another one
        if (_isTransitioning)
        {
            Console.WriteLine($"Cannot load scene {scene.GetType().Name} - another scene is already loading");
            return;
        }
        
        _nextScene = scene;
        _nextScene.SetSceneManager(this);
        
        // Cancel any existing transition coroutine
        if (_transitionCoroutineId != Guid.Empty)
        {
            _coroutineOwner.StopCoroutine(_transitionCoroutineId);
        }
        
        // A single unified transition coroutine resolves the manifest, enforces membership, and picks the
        // per-scene loading screen. It is UNFAILABLE: a missing/unparseable manifest or an unregistered
        // scene must error out (propagate) rather than be silently logged and swallowed.
        _transitionCoroutineId = _coroutineOwner.StartCoroutine(RunTransition(), "SceneTransition", allowFailure: false);
        
        _isTransitioning = true;
        Console.WriteLine($"Started loading scene: {_nextScene.GetType().Name}");
    }

    /// <summary>
    /// The unified scene-transition coroutine. It (1) resolves the manifest — deferred until the AssetManager
    /// is available when registered by asset name — and enforces that a named scene is registered, then
    /// (2) runs either a direct transition or one routed through the resolved per-scene loading screen.
    /// </summary>
    private IEnumerator RunTransition()
    {
        if (_nextScene == null)
            throw new InvalidOperationException("Next scene is null during transition");

        // Step 0 — resolve + enforce (deferred so a manifest registered by asset name can be read after init).
        var manifest = ResolveManifest();
        EnforceMembership(manifest);

        // Step 1 — pick this transition's loading screen (may be null for a direct transition).
        var loadingScreen = ResolveTransitionLoadingScreen(manifest);

        if (loadingScreen == null)
        {
            // Direct transition without a loading screen.
            Console.WriteLine("Starting direct scene transition");
            _nextScene.Load();
            while (_nextScene.IsLoading)
                yield return null;

            if (_currentScene != null)
            {
                Console.WriteLine($"Unloading scene: {_currentScene.GetType().Name}");
                _currentScene.Unload();
            }

            _currentScene = _nextScene;
            _nextScene = null;
            _isTransitioning = false;
            Console.WriteLine("Direct scene transition complete");
            yield break;
        }

        // Transition with a loading screen.
        Console.WriteLine("Starting scene transition with loading screen");

        // Step 2 — show the loading screen (unloading the current scene first).
        if (_currentScene != null)
        {
            Console.WriteLine($"Unloading scene: {_currentScene.GetType().Name}");
            _currentScene.Unload();
        }

        Console.WriteLine("Loading transition screen");
        loadingScreen.Load();
        while (loadingScreen.IsLoading)
            yield return null;

        _currentScene = loadingScreen;
        Console.WriteLine("Transition screen ready");

        // Step 3 — load the target scene in the background.
        _nextScene.Load();
        while (_nextScene.IsLoading)
        {
            // This allows the loading screen to update and display progress.
            yield return null;
        }

        // Target scene is loaded, switch to it.
        Console.WriteLine($"Target scene loaded, switching from loading screen to: {_nextScene.GetType().Name}");

        // Unload the loading screen before swapping so its canvas detaches from the global GUI and stops
        // rendering on top of the new scene (a per-scene screen is re-resolved on the next transition).
        Console.WriteLine("Unloading loading screen");
        loadingScreen.Unload();

        _currentScene = _nextScene;
        _nextScene = null;
        _isTransitioning = false;
        Console.WriteLine("Scene transition complete");
    }

    /// <summary>
    /// Enforces that a named (data-driven) scene is registered in the manifest. Object-based scenes with no
    /// asset name are an escape hatch and are not checked. Throws a descriptive error naming the offending
    /// scene and the registered list when it is not found. Runs inside an unfailable transition coroutine, so
    /// the failure propagates and the game errors out.
    /// </summary>
    private void EnforceMembership(SceneManifest? manifest)
    {
        if (manifest == null) return;
        if (_nextScene is not DataDrivenScene { AssetName: { } name }) return;

        if (manifest.IndexOf(name) < 0)
            throw new InvalidOperationException(
                $"Cannot load scene '{name}': it is not registered in the scene manifest. " +
                $"Registered scenes: {string.Join(", ", manifest.GameScenes.Select(e => e.Name))}.");
    }

    /// <summary>
    /// Resolves the loading screen for this transition. When a manifest governs a named scene, the per-scene
    /// resolution applies (the scene's own attribute, else the default, else none); an already-configured
    /// matching loading scene is reused, otherwise a fresh one is created and stored so <see cref="LoadingScene"/>
    /// reflects what is actually in use. Without a manifest (or for unnamed scenes), the explicitly-set
    /// <c>_loadingScene</c> is used as-is (the object-based path).
    /// </summary>
    private Scene? ResolveTransitionLoadingScreen(SceneManifest? manifest)
    {
        if (manifest != null && _nextScene is DataDrivenScene { AssetName: { } name })
        {
            var screenName = manifest.LoadingScreenFor(name);
            if (screenName == null) return null; // this scene opts out of a loading screen

            if (_loadingScene is DataDrivenScene { AssetName: { } current } && current == screenName)
                return _loadingScene; // reuse the already-configured instance

            var fresh = new DataDrivenScene(screenName);
            fresh.SetSceneManager(this);
            _loadingScene = fresh;
            return fresh;
        }

        return _loadingScene;
    }

    /// <summary>
    /// Updates the current scene.
    /// This method should be called in the game's update loop.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public void Update(GameTime gameTime)
    {
        // Only update the current scene - scene transitions are handled by coroutines
        _currentScene?.Update(gameTime);
    }
    
    /// <summary>
    /// Performs a fixed update on the current scene.
    /// This method should be called in the game's fixed update loop.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public void FixedUpdate(GameTime gameTime)
    {
        _currentScene?.FixedUpdate(gameTime);
    }

    /// <summary>
    /// Notifies the current scene that the application has been paused or resumed.
    /// The scene forwards the call to all registered <see cref="CoreEssentials.GameSystems.IPausableGameSystem"/> instances.
    /// </summary>
    /// <param name="paused">True when the application is being paused, false when resuming.</param>
    public void OnApplicationPause(bool paused)
    {
        _currentScene?.OnApplicationPause(paused);
    }

    /// <summary>
    /// Draws the current scene.
    /// This method should be called in the game's draw loop.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _currentScene?.Draw(gameTime, spriteBatch);
    }
}
