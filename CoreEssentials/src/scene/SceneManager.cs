using CoreEssentials.Debugging;
using CoreEssentials.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections;

namespace CoreEssentials.SceneManagement;
/// <summary>
/// The SceneManager is responsible for managing the current scene and transitioning between scenes.
/// </summary>
public class SceneManager
{
    /// <summary>
    /// Reference to the MainGame instance that this SceneManager is associated with.
    /// </summary>
    MainGame _game;
    /// <summary>
    /// The current active scene.
    /// </summary>
    Scene _currentScene;
    /// <summary>
    /// The next scene to be loaded.
    /// </summary>
    Scene _nextScene;
    /// <summary>
    /// The loading screen scene used during transitions.
    /// </summary>
    Scene _loadingScene;
    /// <summary>
    /// CoroutineOwner for managing scene transition coroutines.
    /// </summary>
    private CoroutineOwner _coroutineOwner;
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
    public MainGame Game => _game;

    /// <summary>
    /// Gets the current active scene.
    /// </summary>
    /// <returns>The current scene.</returns>
    public Scene CurrentScene => _currentScene;
    
    /// <summary>
    /// Gets the next scene to be loaded.
    /// </summary>
    /// <returns>The next scene.</returns>
    public Scene NextScene => _nextScene;
    
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
    
    public SceneManager(MainGame game) : this()
    {
        _game = game;
    }

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
            Debug.Console.WriteLine($"Cannot load scene {scene.GetType().Name} - another scene is already loading");
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
        Debug.Console.WriteLine($"Started loading scene: {_nextScene.GetType().Name}");
    }
    
    /// <summary>
    /// Coroutine that handles direct scene transition without a loading screen.
    /// </summary>
    private IEnumerator DirectTransitionCoroutine()
    {
        Debug.Console.WriteLine("Starting direct scene transition");
        
        // Start loading the new scene
        _nextScene.Load();
        
        // Wait for the scene to finish loading
        while (_nextScene.IsLoading)
        {
            yield return null;
        }
        
        // Unload the current scene if it exists
        if (_currentScene != null)
        {
            Debug.Console.WriteLine($"Unloading scene: {_currentScene.GetType().Name}");
            _currentScene.Unload();
        }
        
        // Switch to the new scene
        _currentScene = _nextScene;
        _nextScene = null;
        
        // Transition complete
        _isTransitioning = false;
        Debug.Console.WriteLine("Direct scene transition complete");
    }
    
    /// <summary>
    /// Coroutine that handles scene transition with a loading screen.
    /// </summary>
    private IEnumerator TransitionWithLoadingScreenCoroutine()
    {
        Debug.Console.WriteLine("Starting scene transition with loading screen");
        
        // Step 1: Show loading screen
        if (_currentScene != null)
        {
            // Unload current scene
            Debug.Console.WriteLine($"Unloading scene: {_currentScene.GetType().Name}");
            _currentScene.Unload();
        }
        
        // Load the loading screen scene first (quickly)
        Debug.Console.WriteLine("Loading transition screen");
        _loadingScene.Load();
        
        // Wait for the loading screen to finish loading
        while (_loadingScene.IsLoading)
        {
            yield return null;
        }
        
        // Set loading screen as current scene
        _currentScene = _loadingScene;
        Debug.Console.WriteLine("Transition screen ready");
        
        // Step 2: Load the target scene in the background
        _nextScene.Load();
        
        // Wait for target scene to finish loading
        while (_nextScene.IsLoading)
        {
            // This allows the loading screen to update and display progress
            yield return null;
        }
        
        // Target scene is loaded, switch to it
        Debug.Console.WriteLine($"Target scene loaded, switching from loading screen to: {_nextScene.GetType().Name}");
        
        // No need to unload loading screen as we'll reuse it
        _currentScene = _nextScene;
        _nextScene = null;
        
        // Transition complete
        _isTransitioning = false;
        Debug.Console.WriteLine("Scene transition complete");
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
