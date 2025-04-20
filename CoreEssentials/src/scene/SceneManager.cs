using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.SceneManagement;
    /// <summary>
    /// The SceneManager is responsible for managing the current scene and transitioning between scenes.
    /// </summary>;

public static class SceneManager
{
    static MainGame _game;
    static Scene _currentScene;
    static Scene _nextScene;
    static bool _isTransitioning = false;

    public static MainGame Game => _game;

    static SceneManager()
    {
        _currentScene = null;
        _nextScene = null;
    }

    public static void SetGame(MainGame game)
    {
        _game = game;
    }

    public static void LoadScene(Scene scene)
    {
        _nextScene = scene;
        _isTransitioning = false;
    }


    public static void Update(GameTime gameTime)
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

    public static void FixedUpdate(GameTime gameTime)
    {
        _currentScene?.FixedUpdate(gameTime);
    }

    public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        _currentScene?.Draw(gameTime, spriteBatch);
    }
}
