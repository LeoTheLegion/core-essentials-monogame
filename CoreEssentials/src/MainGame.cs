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
using CoreEssentials.Scenes;
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
        private readonly GraphicsDeviceManager _graphics;

        /// <summary>
        /// The <see cref="SpriteBatch"/> used for drawing 2D sprites and textures.
        /// </summary>
        protected SpriteBatch? _spriteBatch;

        /// <summary>
        /// The time interval in milliseconds between fixed update calls (set at 50 FPS).
        /// </summary>
        private const float FIXED_UPDATE_MS = 1000f / 50;

        /// <summary>
        /// Accumulated time since the last fixed update.
        /// </summary>
        private float _fixedUpdateTime;

        /// <summary>
        /// Optional auto-exit timer. When set, the game closes itself once the deadline is reached,
        /// enabling unattended smoke-runs of a scene. Null (the default) means run indefinitely.
        /// </summary>
        private AutoExitTimer? _autoExitTimer;

        /// <summary>
        /// When true, window focus changes do not pause/resume the game's systems — so audio keeps
        /// playing even when the window is unfocused. Set for unattended smoke-runs where the window
        /// may never hold foreground. False (the default) preserves normal focus-pause behavior.
        /// </summary>
        private bool _ignoreFocusForPause;

        /// <summary>
        /// Gets the GraphicsDeviceManager for this game.
        /// </summary>
        public GraphicsDeviceManager Graphics => _graphics;

        /// <summary>
        /// Gets the <see cref="SceneManager"/> responsible for managing game scenes.
        /// </summary>
        public SceneManager SceneManager { get; private set; }

        /// <summary>
        /// Enables an opt-in auto-exit: once the given number of seconds of elapsed game time has
        /// passed, the game calls <see cref="Game.Exit"/> and the process terminates. This is intended for
        /// unattended smoke-runs (e.g., launching a scene from the command line and letting it run for a
        /// few seconds). When never called, the game runs indefinitely exactly as before.
        /// </summary>
        /// <param name="seconds">How long (in seconds) to keep running before auto-exiting. Must be positive.</param>
        public void EnableAutoExit(double seconds)
        {
            _autoExitTimer = new AutoExitTimer(seconds);
        }

        /// <summary>
        /// Enables ignoring window focus changes for pausing purposes: when the window loses or gains
        /// focus, the game will NOT pause or resume its systems. This keeps audio (and other
        /// focus-paused systems) running during unattended smoke-runs where the window may never hold
        /// foreground. No-op to call multiple times; the default behavior is preserved until called.
        /// </summary>
        public void EnableIgnoreFocusForPause()
        {
            _ignoreFocusForPause = true;
        }

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
        /// Called when the game window loses focus.
        /// Fires app-wide pause so all <see cref="IPausableGameSystem"/> instances can suspend work.
        /// </summary>
        /// <param name="sender">The game instance.</param>
        /// <param name="args">The event arguments.</param>
        protected override void OnDeactivated(object sender, EventArgs args)
        {
            base.OnDeactivated(sender, args);

            // Focus-pause is suppressed for unattended runs (e.g. --no-focus-pause), so audio keeps
            // playing even when the window loses foreground.
            if (_ignoreFocusForPause)
                return;

            SceneManager.OnApplicationPause(true);
        }

        /// <summary>
        /// Called when the game window regains focus.
        /// Fires app-wide resume so all <see cref="IPausableGameSystem"/> instances can resume work.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="args">The event args.</param>
        protected override void OnActivated(object sender, EventArgs args)
        {
            base.OnActivated(sender, args);

            // Mirror OnDeactivated: when focus-pause is suppressed we never paused, so there is
            // nothing to resume.
            if (_ignoreFocusForPause)
                return;

            SceneManager.OnApplicationPause(false);
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

            // Opt-in auto-exit (smoke-run): only ticks when a deadline was set, so the default
            // game loop is untouched. When the duration elapses, close the game cleanly.
            if (_autoExitTimer != null)
            {
                _autoExitTimer.Tick((float)gameTime.ElapsedGameTime.TotalMilliseconds);
                if (_autoExitTimer.IsExpired)
                    this.Exit();
            }

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