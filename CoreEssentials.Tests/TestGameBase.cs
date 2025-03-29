using System;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;

namespace CoreEssentials.Tests
{
    public class TestGameBase : Game, IDisposable
    {
        private readonly GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private AutoResetEvent _updateEvent;
        private Thread _gameThread;
        private bool _isRunning;

        public TestGameBase()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            
            // We need to run minimal window settings for tests
            _graphics.PreferredBackBufferWidth = 100;
            _graphics.PreferredBackBufferHeight = 100;
            _graphics.IsFullScreen = false;
            _graphics.ApplyChanges();
            
            
            _updateEvent = new AutoResetEvent(false);
        }

        protected override void Initialize()
        {
            base.Initialize();

            // Initialize the AssetManager
            AssetManager.Init(Content);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // Signal that an update has occurred
            _updateEvent.Set();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            base.Draw(gameTime);
        }

        public void RunHeadless()
        {
            _isRunning = true;
            _gameThread = new Thread(() =>
            {
                try
                {
                    Run();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Game thread exception: {ex}");
                }
                finally
                {
                    _isRunning = false;
                }
            });
            
            _gameThread.Start();
            
            // Wait for the first update to ensure initialization is complete
            _updateEvent.WaitOne(5000);
        }

        public new void Dispose()
        {
            if (_isRunning)
            {
                Exit();
                // Give it a moment to shutdown properly
                Thread.Sleep(500);
                
                // Force kill if needed
                if (_gameThread.IsAlive)
                {
                    _gameThread.Abort();
                }
            }
            
            _updateEvent.Dispose();
            base.Dispose();
        }
    }
}
