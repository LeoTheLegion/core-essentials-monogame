using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems
{
    public abstract class GameSystem
    {
        private MainGame _game;

        protected GameSystem(MainGame game)
        {
            _game = game;
        }

        public T GetGameSystem<T>() where T : GameSystem
        {
            return _game.GetGameSystem<T>();
        }
    }

    public interface IUpdateGameSystem
    {
        void Update(GameTime gameTime);
    }

    public interface IDrawGameSystem
    {
        void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }

    public interface IFixedUpdateGameSystem
    {
        void FixedUpdate(GameTime gameTime);
    }
}
