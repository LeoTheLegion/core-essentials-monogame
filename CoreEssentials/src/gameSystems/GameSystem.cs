using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.SceneManagement;

namespace CoreEssentials.GameSystems
{
    /// <summary>
    /// Base abstract class for all game systems in the CoreEssentials framework.
    /// Game systems encapsulate distinct functional areas of the game, such as physics,
    /// rendering, input handling, or AI. This modularity allows for better organization
    /// and separation of concerns.
    /// </summary>
    public abstract class GameSystem
    {
        private Scene _scene;

        /// <summary>
        /// Gets the MainGame instance associated with this system.
        /// </summary>
        public MainGame Game => _scene?.SceneManager?.Game;

        protected GameSystem()
        {
        }

        /// <summary>
        /// Sets the Scene instance for this system.
        /// This is called automatically during system registration.
        /// </summary>
        /// <param name="scene">The scene instance.</param>
        public void SetScene(Scene scene)
        {
            _scene = scene;
        }

        /// <summary>
        /// Retrieves a game system of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the game system to retrieve.</typeparam>
        /// <returns>The game system instance of the specified type.</returns>
        public T GetGameSystem<T>() where T : GameSystem
        {
            return _scene.GetGameSystem<T>();
        }

        /// <summary>
        /// Called after all game systems have been loaded and initialized.
        /// Override this method to perform any setup that depends on other game systems.
        /// </summary>
        public virtual void OnStart()
        {
            // Default implementation does nothing.
        }
    }

    /// <summary>
    /// Interface for game systems that need to run on every frame update.
    /// Implement this interface in any GameSystem that needs to perform regular processing
    /// during the game's update loop.
    /// </summary>
    public interface IUpdateGameSystem
    {
        /// <summary>
        /// Updates the game system's state.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void Update(GameTime gameTime);
    }

    /// <summary>
    /// Interface for game systems that need to render to the screen.
    /// Implement this interface in any GameSystem that needs to draw visual elements.
    /// </summary>
    public interface IDrawGameSystem
    {
        /// <summary>
        /// Draws the game system's visual elements.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        /// <param name="spriteBatch">The SpriteBatch used for drawing sprites and textures.</param>
        void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }

    /// <summary>
    /// Interface for game systems that need to run on fixed-interval updates.
    /// This is useful for physics or other simulation systems that should run at a consistent rate
    /// regardless of frame rate.
    /// </summary>
    public interface IFixedUpdateGameSystem
    {
        /// <summary>
        /// Updates the game system's state at a fixed time interval.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        void FixedUpdate(GameTime gameTime);
    }
}
