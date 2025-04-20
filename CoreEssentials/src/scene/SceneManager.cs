using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.SceneManagement;
/// <summary>
/// The SceneManager is responsible for managing the current scene and transitioning between scenes.
/// </summary>;

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
    /// Flag indicating whether a scene transition is in progress.
    /// </summary>
    bool _isTransitioning = false;

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
    public SceneManager(MainGame game) : this()
    {
        _game = game;
    }

    public SceneManager()
    {
        _currentScene = null;
        _nextScene = null;
    }

    /// <summary>
    /// Loads the specified scene and sets it as the next scene to be loaded.
    /// This method should be called when transitioning to a new scene.
    /// </summary>
    /// <param name="scene">The scene to be loaded.</param>
    public void LoadScene(Scene scene)
    {
        _nextScene = scene;
        _nextScene.SetSceneManager(this);
        _isTransitioning = false;
    }

    /// <summary>
    /// Updates the current scene and handles scene transitions.
    /// This method should be called in the game's update loop.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public void Update(GameTime gameTime)
    {
        if (_nextScene != null && !_isTransitioning)
        {
            Debug.Console.WriteLine($"Loading scene in background: {_nextScene.GetType().Name}");
            _isTransitioning = true;
            // this could be a good place to add a loading screen
            // and run this in a separate thread
            _nextScene.Load();
        }

        if (_isTransitioning)
        {
            _currentScene?.Unload();
            Debug.Console.WriteLine($"Unloading old scene: {_currentScene?.GetType().Name}");
            _currentScene = _nextScene;
            Debug.Console.WriteLine($"Loaded scene: {_currentScene?.GetType().Name}");
            _nextScene = null;
            _isTransitioning = false;
        }

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
