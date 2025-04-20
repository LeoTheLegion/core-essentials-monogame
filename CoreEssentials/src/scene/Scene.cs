using System;
using System.Collections.Generic;
using System.Linq;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.SceneManagement;

public abstract class Scene
{
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
    /// Abstract method that must be implemented by derived classes to load and return an array of game systems.
    /// </summary>
    /// <returns>An array of GameSystem objects to be registered with the game.</returns>
    protected abstract GameSystem[] LoadGameSystems();

    public bool IsLoaded {get; private set; }

    public Scene()
    {
        // Constructor logic can be added here if needed.
        IsLoaded = false;
    }

    public void SetSceneManager(SceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public void Load()
    {
        // Load all the game systems you want to use in your game here.
        GameSystem[] systems = LoadGameSystems();

        for (int i = 0; i < systems.Length; i++)
        {
            if (_gameSystems.ContainsKey(systems[i].GetType()))
                throw new Exception("Game System already exists: " + systems[i].GetType().ToString());

            _gameSystems.Add(systems[i].GetType(), systems[i]);
            systems[i].SetScene(this);
        }

        // Initialize all game systems
        _updateSystems = systems.OfType<IUpdateGameSystem>().ToArray();
        _drawSystems = systems.OfType<IDrawGameSystem>().ToArray();
        _fixedUpdateSystems = systems.OfType<IFixedUpdateGameSystem>().ToArray();

        Debug.Console.WriteLine("Loaded Update Systems: " + _updateSystems.Length.ToString());
        Debug.Console.WriteLine("Loaded Fixed Update Systems: " + _fixedUpdateSystems.Length.ToString());
        Debug.Console.WriteLine("Loaded Draw Systems: " + _drawSystems.Length.ToString());

        // Call the onStart method to perform any additional initialization
        onStart();

        IsLoaded = true;
    }
    /// <summary>
    /// Called when the scene is loaded and all game systems are registered.
    /// This method should be overridden in derived classes to perform any additional initialization.
    /// </summary>
    protected abstract void onStart();

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

    public void Update(GameTime gameTime)
    {
        // Update all game systems that implement IUpdateGameSystem
        for (int i = 0; i < _updateSystems.Length; i++)
        {
            _updateSystems[i].Update(gameTime);
        }
    }

    public void FixedUpdate(GameTime gameTime)
    {
        // Update all game systems that implement IFixedUpdateGameSystem
        for (int i = 0; i < _fixedUpdateSystems.Length; i++)
        {
            _fixedUpdateSystems[i].FixedUpdate(gameTime);
        }
    }

    public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        // Draw all game systems that implement IDrawGameSystem
        for (int i = 0; i < _drawSystems.Length; i++)
        {
            _drawSystems[i].Draw(gameTime, spriteBatch);
        }
    }

    internal void Unload()
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
    }
}
