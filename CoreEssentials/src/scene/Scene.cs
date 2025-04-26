using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using CoreEssentials.Coroutines;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.SceneManagement;

public abstract class Scene
{
    /// <summary>
    /// Reference to the SceneManager that manages this scene.
    /// </summary>
    private SceneManager _sceneManager;

    public SceneManager SceneManager => _sceneManager;
    
    /// <summary>
    /// Collection of all registered game systems mapped by their type.
    /// </summary>
    private Dictionary<Type, GameSystem> _gameSystems = new Dictionary<Type, GameSystem>();

    /// <summary>
    /// Array of game systems that implement the IUpdateGameSystem interface.
    /// </summary>
    private IUpdateGameSystem[] _updateSystems;

    /// <summary>
    /// Array of game systems that implement the IDrawGameSystem interface.
    /// </summary>
    private IDrawGameSystem[] _drawSystems;

    /// <summary>
    /// Array of game systems that implement the IFixedUpdateGameSystem interface.
    /// </summary>
    private IFixedUpdateGameSystem[] _fixedUpdateSystems;
    
    /// <summary>
    /// Tracks the current loading progress of the scene, from 0.0 to 1.0
    /// </summary>
    protected float _loadingProgress = 0f;
    
    /// <summary>
    /// Current status/description of what the scene is loading
    /// </summary>
    private string _loadingStatus = "Initializing...";

    /// <summary>
    /// Abstract method that must be implemented by derived classes to load and return an array of game systems.
    /// </summary>
    /// <returns>An array of GameSystem objects to be registered with the game.</returns>
    protected abstract GameSystem[] LoadGameSystems();

    /// <summary>
    /// Indicates whether the scene has been loaded and initialized.
    /// </summary>
    public bool IsLoaded {get; private set; }
    
    /// <summary>
    /// Indicates whether the scene is currently in the process of loading.
    /// </summary>
    public bool IsLoading {get; private set; }

    /// <summary>
    /// Gets the current loading progress of the scene as a value between 0.0 and 1.0.
    /// </summary>
    public float LoadingProgress => _loadingProgress;
    
    /// <summary>
    /// Gets or sets the current status message during scene loading.
    /// </summary>
    public string LoadingStatus 
    { 
        get => _loadingStatus; 
        protected set 
        { 
            _loadingStatus = value;
        } 
    }

    /// <summary>
    /// Default constructor for the Scene class.
    /// Initializes the IsLoaded property to false.
    /// </summary>
    public Scene()
    {
        IsLoaded = false;
        IsLoading = false;
        _loadingProgress = 0f;
        LoadingStatus = "Initializing...";
    }

    /// <summary>
    /// Sets the SceneManager for this scene.
    /// This method is called by the SceneManager when the scene is loaded.
    /// </summary>
    public void SetSceneManager(SceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }
    
    /// <summary>
    /// Gets the current loading progress as a percentage (0-100).
    /// </summary>
    /// <returns>The loading progress as a percentage.</returns>
    public int GetLoadingProgressPercentage()
    {
        return (int)(_loadingProgress * 100);
    }

    /// <summary>
    /// Updates the loading progress and status with a single call
    /// </summary>
    /// <param name="progress">New progress value (0.0 to 1.0)</param>
    /// <param name="status">New status message</param>
    protected void UpdateLoadingProgress(float progress, string status)
    {
        _loadingProgress = Math.Clamp(progress, 0f, 1f);
        LoadingStatus = status;
    }

    /// <summary>
    /// Loads the scene and initializes all game systems.
    /// This method should be called when the scene is loaded by the SceneManager.
    /// </summary>
    public void Load()
    {
        if (IsLoading || IsLoaded)
            return;
            
        IsLoading = true;
        _loadingProgress = 0f;
        
        // Start the loading coroutine
        CoroutineManager.StartCoroutine(LoadCoroutine());
    }
    
    /// <summary>
    /// Coroutine that handles the loading process of the scene.
    /// </summary>
    /// <returns>An IEnumerator used by the coroutine system.</returns>
    private IEnumerator LoadCoroutine()
    {
        Debug.Console.WriteLine($"Starting to load scene: {this.GetType().Name}");
        
        // Phase 1: Load game systems (25% of progress)
        UpdateLoadingProgress(0.05f, "Loading game systems...");
        yield return null;  // Allow a frame to process to update UI
        
        GameSystem[] systems = LoadGameSystems();
        UpdateLoadingProgress(0.25f, "Registering game systems...");
        yield return null;
        
        // Phase 2: Register game systems (50% of progress)
        int totalSystems = systems.Length;
        for (int i = 0; i < systems.Length; i++)
        {
            if (_gameSystems.ContainsKey(systems[i].GetType()))
                throw new Exception("Game System already exists: " + systems[i].GetType().ToString());

            _gameSystems.Add(systems[i].GetType(), systems[i]);
            systems[i].SetScene(this);
            
            // Update progress based on registered systems
            _loadingProgress = 0.25f + (0.25f * ((float)(i + 1) / totalSystems));
            
            // Every few systems, yield to keep the game responsive
            if (i % 3 == 0)
                yield return null;
        }

        // Initialize game system arrays
        _updateSystems = systems.OfType<IUpdateGameSystem>().ToArray();
        _drawSystems = systems.OfType<IDrawGameSystem>().ToArray();
        _fixedUpdateSystems = systems.OfType<IFixedUpdateGameSystem>().ToArray();

        Debug.Console.WriteLine("Loaded Update Systems: " + _updateSystems.Length.ToString());
        Debug.Console.WriteLine("Loaded Fixed Update Systems: " + _fixedUpdateSystems.Length.ToString());
        Debug.Console.WriteLine("Loaded Draw Systems: " + _drawSystems.Length.ToString());
        
        UpdateLoadingProgress(0.5f, "Initializing scene...");
        yield return null;
        
        // Phase 3: Call onStart for additional initialization (50-100% progress)
        // Derived classes can update _loadingProgress during onStartCoroutine
        yield return OnStartCoroutine();
        
        // Ensure we reach 100% at the end
        UpdateLoadingProgress(1.0f, "Loading complete");
        IsLoaded = true;
        IsLoading = false;
        
        Debug.Console.WriteLine($"Scene loaded: {this.GetType().Name}");
    }
    
    /// <summary>
    /// Called when the scene is loaded and all game systems are registered.
    /// This coroutine allows for asynchronous initialization of the scene.
    /// </summary>
    /// <returns>An IEnumerator used by the coroutine system.</returns>
    protected abstract IEnumerator OnStartCoroutine();

    /// <summary>
    /// Gets a game system by its type.
    /// </summary>
    /// <typeparam name="T">The type of game system to retrieve.</typeparam>
    /// <returns>The requested game system.</returns>
    /// <exception cref="Exception">Thrown when the requested game system is not found.</exception>
    public T GetGameSystem<T>() where T : GameSystem
    {
        if (_gameSystems.ContainsKey(typeof(T)))
            return (T)_gameSystems[typeof(T)];
        else
            throw new Exception("Game System not found: " + typeof(T).ToString());
    }

    /// <summary>
    /// Updates all game systems that implement the IUpdateGameSystem interface.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public void Update(GameTime gameTime)
    {
        // Don't update if scene isn't loaded yet
        if (!IsLoaded) 
            return;
            
        // Update all game systems that implement IUpdateGameSystem
        for (int i = 0; i < _updateSystems.Length; i++)
        {
            _updateSystems[i].Update(gameTime);
        }
    }

    /// <summary>
    /// Updates all game systems that implement the IFixedUpdateGameSystem interface.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <remarks>This method is called at a fixed interval, typically for physics updates.</remarks>
    public void FixedUpdate(GameTime gameTime)
    {
        // Don't update if scene isn't loaded yet
        if (!IsLoaded) 
            return;
            
        // Update all game systems that implement IFixedUpdateGameSystem
        for (int i = 0; i < _fixedUpdateSystems.Length; i++)
        {
            _fixedUpdateSystems[i].FixedUpdate(gameTime);
        }
    }

    /// <summary>
    /// Draws all game systems that implement the IDrawGameSystem interface.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <remarks>This method is called to render the scene.</remarks>
    public virtual void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Don't draw if scene isn't loaded yet
        if (!IsLoaded) 
            return;
            
        // Draw all game systems that implement IDrawGameSystem
        for (int i = 0; i < _drawSystems.Length; i++)
        {
            _drawSystems[i].Draw(gameTime, spriteBatch);
        }
    }

    /// <summary>
    /// Unloads the scene and performs any necessary cleanup.
    /// This method should be called when the scene is unloaded by the SceneManager.
    /// </summary>
    /// <remarks>This method is called when the scene is unloaded by the SceneManager.</remarks>
    public virtual void Unload()
    {
        // Unload all game systems and perform any necessary cleanup
        foreach (var system in _gameSystems.Values)
        {
            if (system is IDisposable disposableSystem)
            {
                disposableSystem.Dispose();
            }
        }

        _gameSystems.Clear();
        _updateSystems = null;
        _drawSystems = null;
        _fixedUpdateSystems = null;
        
        IsLoaded = false;
        IsLoading = false;
        _loadingProgress = 0f;
    }
}
