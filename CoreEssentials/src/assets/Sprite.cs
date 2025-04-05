using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

public class Sprite : Asset
{
    private Texture2D _texture;

    public Sprite(string name) : base(name)
    {
        _texture = AssetManager.LoadAsset<Texture2D>(this._assetName);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        spriteBatch.Draw(_texture, position, color);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        spriteBatch.Draw(_texture, position, null, color, rotation, origin, scale, effects, layerDepth);
    }

    public Vector2 GetSize()
    {
        return new Vector2(_texture.Width, _texture.Height);
    }
}
