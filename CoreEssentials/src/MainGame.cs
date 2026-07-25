using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using CoreEssentials.GUI;
using CoreEssentials.Inputs;
using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using System.Collections.Generic;
using System;
using CoreEssentials.SceneManagement;
using CoreEssentials.Coroutines;
using CoreEssentials.Audio;
using CoreEssentials.Timing;

namespace CoreEssentials
{
    /// <summary>
    /// Base abstract class for MonoGame applications that provides a structured framework with game systems management,
    /// fixed update timing, diagnostics, input handling, and GUI integration.
    /// </summary>
    public class MainGame : Game
    {
        private GraphicsDeviceManager _graphics;

        /// <summary>
        /// The <see cref="SpriteBatch"/> used for drawing 2D sprites and textures.
        /// </summary>
        protected SpriteBatch? _spriteBatch;

        /// <summary>
        /// The time interval in milliseconds between fixed update calls (set at 50 FPS).
        /// </summary>
        private const float FIXED_UPDATE_MS = 1000 / 50;

        /// <summary>
        /// Accumulated time since the last fixed update.
        /// </summary>
        private float _fixedUpdateTime;

        /// <summary>
        /// Gets the GraphicsDeviceManager for this game.
        /// </summary>
        public GraphicsDeviceManager Graphics => _graphics;

        /// <summary>
        /// Gets the <see cref="SceneManagement.SceneManager"/> responsible for managing game scenes.
        /// </summary>
        public SceneManager SceneManager { get; private set; }

        /// <summary>
        /// Initializes a new instance of the MainGame class.
        /// Sets up the graphics device manager with default settings and configures the content directory.
        /// </summary>
        public MainGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = false;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;

            _graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            SceneManager = new SceneManager(this);
        }

        /// <summary>
        /// Initializes the game. Sets up the escape key to exit the game.
        /// </summary>
        protected override void Initialize()
        {
            Input.Keyboard.KeyPressed += (sender, args) =>
            {
                if (args.Key == Keys.Escape)
                    this.Exit();
            };

            base.Initialize();
        }

        /// <summary>
        /// Loads game content and initializes systems. 
        /// This includes setting up the SpriteBatch, AssetManager, GUI, debugging tools, and game systems.
        /// </summary>
        protected override void LoadContent()
        {

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            IContentManager contentManagerWrapper = new ContentManagerWrapper(Content);

            AssetManager.Init(contentManagerWrapper);
            GUIManager.Init(this, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            Debug.StickyLog.LoadGUI();
        }

        /// <summary>
        /// Updates the game state. This method runs the update logic for all game systems and handles fixed update timing.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Time.SetDeltaTime((float)gameTime.ElapsedGameTime.TotalMilliseconds);
            Debug.baseGameDiagnostics.UpdateBegin();

            Input.Update(gameTime);

            // Update all active coroutines
            CoroutineManager.Update(gameTime);

            _fixedUpdateTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_fixedUpdateTime >= FIXED_UPDATE_MS)
            {
                Debug.baseGameDiagnostics.FixedUpdateEnd();
                Debug.baseGameDiagnostics.FixedUpdateBegin();

                SceneManager.FixedUpdate(gameTime);

                _fixedUpdateTime -= FIXED_UPDATE_MS;

            }            
            
            SceneManager.Update(gameTime);    

            AudioManager.Instance.Update(gameTime);
            
            Debug.StickyLog.Update(gameTime);

            base.Update(gameTime);

            Debug.baseGameDiagnostics.UpdateEnd();
        }

        /// <summary>
        /// Renders the game. This method runs the drawing code for all game systems and renders the GUI.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            if (_spriteBatch == null)
                return;

            Debug.baseGameDiagnostics.DrawBegin();
            GraphicsDevice.Clear(Color.Black);

            SceneManager.Draw(gameTime, _spriteBatch);

            GUIManager.Draw(gameTime);

            base.Draw(gameTime);

            Debug.baseGameDiagnostics.DrawEnd();
        }
    }
}