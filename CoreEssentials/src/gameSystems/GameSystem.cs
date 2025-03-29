using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems
{
    public abstract class GameSystem
    {
    }

    public interface IUpdateGameSystem
    {
        void Update(GameTime gameTime);
    }

    public interface IDrawGameSystem
    {
        void Draw(GameTime gameTime, SpriteBatch spriteBatch);
    }

    public interface ILoadAssetGameSystem
    {
        void LoadAssets();
    }

    public interface IFixedUpdateGameSystem
    {
        void FixedUpdate(GameTime gameTime);
    }
}
