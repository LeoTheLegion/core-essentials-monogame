using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

public class Ball : Entity
{
    private Texture2D _texture;

    public Ball(Vector2 position)
    {
        _position = position;
        sort = 0;
    }

    public override void LoadAssets()
    {
        this._texture = AssetManager.LoadAsset<Texture2D>("Ball");
    }

    public override void Update(ref GameTime gameTime)
    {

    }

    public override void Render(ref SpriteBatch _spriteBatch)
    {
        _spriteBatch.Begin();
        _spriteBatch.Draw(_texture, _position, Color.White);
        _spriteBatch.End();
    }
}
