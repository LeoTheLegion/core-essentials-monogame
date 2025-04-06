using System;
using System.Text.Json;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

public class Sprite : Asset
{
    private Texture2D _texture;
    private SpriteMeta _metaData;

    public Sprite(string name) : base(name)
    {
        var json = AssetManager.LoadAsset<string>(name);
        if (json == null)
        {
            throw new ArgumentNullException("json", "Asset name cannot be null or empty.");
        }
        _metaData = JsonSerializer.Deserialize<SpriteMeta>(json);
        if (_metaData == null)
        {
            throw new InvalidOperationException("Failed to deserialize sprite metadata.");
        }
        switch (_metaData.SourceType)
        {
            case "texture2d":
                _texture = AssetManager.LoadAsset<Texture2D>(this._metaData.Source);
                break;
            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }

    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, SpriteEffects effects, float layerDepth)
    {
        switch (_metaData.SourceType)
        {
            case "texture2d":
                Vector2 TextureScale = new Vector2(_metaData.Size.Width / _texture.Width,
                                         _metaData.Size.Height / _texture.Height);

                float xFactor = _metaData.Origin.X / _metaData.Size.Width;
                float yFactor = _metaData.Origin.Y / _metaData.Size.Height;

                Vector2 TextureOrigin = new Vector2(
                    _texture.Width * xFactor,
                    _texture.Height * yFactor
                );

                spriteBatch.Draw(_texture,
                position,
                null,
                color,
                rotation,
                TextureOrigin,
                TextureScale,
                effects,
                layerDepth);

                break;
            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }

        Rectangle targetRectangle = new Rectangle(
            (int)(position.X - _metaData.Origin.X), (int)(position.Y - _metaData.Origin.X),
             (int)(GetSize().X), (int)(GetSize().Y)
        );

        Debug.Primitives.DrawRectangle(spriteBatch, targetRectangle, Color.Red, 1f);
    }

    public Vector2 GetSize()
    {
        Vector2 size = new Vector2(_metaData.Size.Width, _metaData.Size.Height);
        return size;
    }

    private class Size
    {
        public float Width { get; set; }
        public float Height { get; set; }
    }

    private class Origin
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    private class SpriteMeta
    {
        public string SourceType { get; set; }
        public string Source { get; set; }
        public Size Size { get; set; }
        public Origin Origin { get; set; }
    }
}
