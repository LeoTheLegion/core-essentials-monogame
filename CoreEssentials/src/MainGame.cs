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

namespace CoreEssentials
{
    /// <summary>
    /// Base abstract class for MonoGame applications that provides a structured framework with game systems management,
    /// fixed update timing, diagnostics, input handling, and GUI integration.
    /// </summary>
    public abstract class MainGame : Game
    {
        private GraphicsDeviceManager _graphics;
        protected SpriteBatch _spriteBatch;

        /// <summary>
        /// The time interval in milliseconds between fixed update calls (set at 50 FPS).
        /// </summary>
        private const float FIXED_UPDATE_MS = 1000 / 50;
        
        /// <summary>
        /// Accumulated time since the last fixed update.
        /// </summary>
        private float _fixedUpdateTime;

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

        /// <summary>
        /// Gets the GraphicsDeviceManager for this game.
        /// </summary>
        protected GraphicsDeviceManager Graphics => _graphics;

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
        }

        /// <summary>
        /// Initializes the game. Sets up the escape key to exit the game.
        /// </summary>
        protected override void Initialize()
        {
            Input.Keyboard.KeyPressed += (sender, args) => {
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

            AssetManager.Init(Content);
            GUIManager.Init(this, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            Debug.StickyLog.LoadGUI();
            Debug.Console.LoadGUI();
            
            GameSystem[] _systems = LoadGameSystems();

            for (int i = 0; i < _systems.Length; i++)
            {
                if (_gameSystems.ContainsKey(_systems[i].GetType()))
                    throw new Exception("Game System already exists: " + _systems[i].GetType().ToString());

                _gameSystems.Add(_systems[i].GetType(), _systems[i]);
                _systems[i].SetGame(this);
            }
            // Initialize all game systems

            _updateSystems = _systems.OfType<IUpdateGameSystem>().ToArray();
            _drawSystems = _systems.OfType<IDrawGameSystem>().ToArray();
            _fixedUpdateSystems = _systems.OfType<IFixedUpdateGameSystem>().ToArray();

            Debug.Console.WriteLine("Game Systems Loaded: " + _systems.Length.ToString());
            Debug.Console.WriteLine("Update Systems Loaded: " + _updateSystems.Length.ToString());
            Debug.Console.WriteLine("Fixed Update Systems Loaded: " + _fixedUpdateSystems.Length.ToString());
            Debug.Console.WriteLine("Draw Systems Loaded: " + _drawSystems.Length.ToString());

            onStart();
        }

        /// <summary>
        /// Abstract method called at the end of LoadContent.
        /// Derived classes should implement this to perform any initialization that needs to happen after systems are loaded.
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

        /// <summary>
        /// Updates the game state. This method runs the update logic for all game systems and handles fixed update timing.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            Debug.baseGameDiagnostics.UpdateBegin();

            Input.Update(gameTime);

            _fixedUpdateTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;

            if(_fixedUpdateTime >= FIXED_UPDATE_MS)
            {
                Debug.baseGameDiagnostics.FixedUpdateEnd();
                Debug.baseGameDiagnostics.FixedUpdateBegin();

                for (int i = 0; i < _fixedUpdateSystems.Length; i++)
                {
                    _fixedUpdateSystems[i].FixedUpdate(gameTime);
                }
                _fixedUpdateTime -= FIXED_UPDATE_MS;
                
            }

            for (int i = 0; i < _updateSystems.Length; i++)
                _updateSystems[i].Update(gameTime);
            
            base.Update(gameTime);

            Debug.baseGameDiagnostics.UpdateEnd();
        }

        /// <summary>
        /// Renders the game. This method runs the drawing code for all game systems and renders the GUI.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            Debug.baseGameDiagnostics.DrawBegin();
            GraphicsDevice.Clear(Color.Black);

            for (int i = 0; i < _drawSystems.Length; i++)
                _drawSystems[i].Draw(gameTime, _spriteBatch);
           
            GUIManager.Draw(gameTime);

            base.Draw(gameTime);

            Debug.baseGameDiagnostics.DrawEnd();
        }
    }
}