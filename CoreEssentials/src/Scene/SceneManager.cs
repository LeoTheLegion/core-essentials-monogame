using CoreEssentials.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

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
    /// Gets whether a scene transition is in progress.
    /// </summary>
    /// <returns>True if a scene is currently loading; otherwise, false.</returns>
    public bool IsTransitioning => _isTransitioning;
    
    /// <summary>
    /// Gets the loading progress of the next scene if a transition is in progress.
    /// Returns 0 if no transition is happening.
    /// </summary>
    public float TransitionProgress => (_nextScene != null && _nextScene.IsLoading) ? _nextScene.LoadingProgress : 0f;
    
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
    /// Sets the scene to be used as a loading screen during transitions.
    /// </summary>
    /// <param name="loadingScene">The loading screen scene.</param>
    public void SetLoadingScene(Scene loadingScene)
    {
        _loadingScene = loadingScene;
        _loadingScene.SetSceneManager(this);
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
        
        // Start the appropriate transition coroutine
        if (_loadingScene != null)
        {
            // Use loading screen for transition
            _transitionCoroutineId = _coroutineOwner.StartCoroutine(TransitionWithLoadingScreenCoroutine(), "SceneTransitionWithLoadingScreen");
        }
        else
        {
            // Direct transition without loading screen
            _transitionCoroutineId = _coroutineOwner.StartCoroutine(DirectTransitionCoroutine(), "DirectSceneTransition");
        }
        
        _isTransitioning = true;
        Console.WriteLine($"Started loading scene: {_nextScene.GetType().Name}");
    }
    
    /// <summary>
    /// Coroutine that handles direct scene transition without a loading screen.
    /// </summary>
    private IEnumerator DirectTransitionCoroutine()
    {
        Console.WriteLine("Starting direct scene transition");
        
        // Start loading the new scene
        if (_nextScene == null)
        {
            throw new InvalidOperationException("Next scene is null during direct transition");
        }
        _nextScene.Load();
        
        // Wait for the scene to finish loading
        while (_nextScene.IsLoading)
        {
            yield return null;
        }
        
        // Unload the current scene if it exists
        if (_currentScene != null)
        {
            Console.WriteLine($"Unloading scene: {_currentScene.GetType().Name}");
            _currentScene.Unload();
        }
        
        // Switch to the new scene
        _currentScene = _nextScene;
        _nextScene = null;
        
        // Transition complete
        _isTransitioning = false;
        Console.WriteLine("Direct scene transition complete");
    }
    
    /// <summary>
    /// Coroutine that handles scene transition with a loading screen.
    /// </summary>
    private IEnumerator TransitionWithLoadingScreenCoroutine()
    {
        Console.WriteLine("Starting scene transition with loading screen");
        
        // Step 1: Show loading screen
        if (_currentScene != null)
        {
            // Unload current scene
            Console.WriteLine($"Unloading scene: {_currentScene.GetType().Name}");
            _currentScene.Unload();
        }
        
        // Load the loading screen scene first (quickly)
        Console.WriteLine("Loading transition screen");
        if (_loadingScene == null)
        {
            throw new InvalidOperationException("Loading scene is null during transition with loading screen");
        }
        _loadingScene.Load();
        
        // Wait for the loading screen to finish loading
        while (_loadingScene.IsLoading)
        {
            yield return null;
        }
        
        // Set loading screen as current scene
        _currentScene = _loadingScene;
        Console.WriteLine("Transition screen ready");
        
        // Step 2: Load the target scene in the background
        if (_nextScene == null)
        {
            throw new InvalidOperationException("Next scene is null during transition with loading screen");
        }
        _nextScene.Load();
        
        // Wait for target scene to finish loading
        while (_nextScene.IsLoading)
        {
            // This allows the loading screen to update and display progress
            yield return null;
        }
        
        // Target scene is loaded, switch to it
        Console.WriteLine($"Target scene loaded, switching from loading screen to: {_nextScene.GetType().Name}");
        
        // No need to unload loading screen as we'll reuse it
        _currentScene = _nextScene;
        _nextScene = null;
        
        // Transition complete
        _isTransitioning = false;
        Console.WriteLine("Scene transition complete");
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
