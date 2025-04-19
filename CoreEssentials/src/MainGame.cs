using CoreEssentials.Assets;
using CoreEssentials.Debugging;
using CoreEssentials.GUI;
using CoreEssentials.Inputs;
using CoreEssentials.GameSystems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace CoreEssentials
{
    public abstract class MainGame : Game
    {
        private GraphicsDeviceManager _graphics;
        protected SpriteBatch _spriteBatch;

        private const float FIXED_UPDATE_MS = 1000 / 50;
        private float _fixedUpdateTime;
        private IUpdateGameSystem[] _updateSystems;
        private IDrawGameSystem[] _drawSystems;
        private IFixedUpdateGameSystem[] _fixedUpdateSystems;

        protected abstract GameSystem[] LoadSystems();

        protected GraphicsDeviceManager Graphics => _graphics;

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

        protected override void Initialize()
        {
            Input.Keyboard.KeyPressed += (sender, args) => {
                if (args.Key == Keys.Escape)
                    this.Exit();
            };

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            AssetManager.Init(Content);
            GUIManager.Init(this, _graphics.PreferredBackBufferWidth, _graphics.PreferredBackBufferHeight);
            Debug.StickyLog.LoadGUI();
            Debug.Console.LoadGUI();
            onStart();
        }

        private void onStart(){
            GameSystem[] _systems = LoadSystems();
            _updateSystems = _systems.OfType<IUpdateGameSystem>().ToArray();
            _drawSystems = _systems.OfType<IDrawGameSystem>().ToArray();
            _fixedUpdateSystems = _systems.OfType<IFixedUpdateGameSystem>().ToArray();

            Debug.Console.WriteLine("Game Systems Loaded: " + _systems.Length.ToString());
            Debug.Console.WriteLine("Update Systems Loaded: " + _updateSystems.Length.ToString());
            Debug.Console.WriteLine("Fixed Update Systems Loaded: " + _fixedUpdateSystems.Length.ToString());
            Debug.Console.WriteLine("Draw Systems Loaded: " + _drawSystems.Length.ToString());
        }

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