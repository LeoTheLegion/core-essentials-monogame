using System;
using System.Xml.Serialization;
using System.IO;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents a drawable sprite asset with metadata for rendering.
/// The Sprite class loads and manages sprite data including source texture, size, and origin.
/// </summary>
public class Sprite : Asset
{
    private Texture2D _texture;
    private SpriteMeta _metaData;

    /// <summary>
    /// Initializes a new instance of the Sprite class.
    /// Loads sprite metadata from an XML file and the associated texture.
    /// </summary>
    /// <param name="name">The name of the sprite asset to load.</param>
    /// <exception cref="ArgumentNullException">Thrown when the asset name or data is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when metadata deserialization fails or the source type is unknown.</exception>
    public Sprite(string name) : base(name)
    {
        string extension = Path.GetExtension(name);
        if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            LoadFromXml(name);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported sprite data format: {extension}. Use .xml format");
        }

        if (_metaData.SourceType == null)
        {
            throw new InvalidOperationException("Sprite metadata source type cannot be null.");
        }
        
        switch (_metaData.SourceType)
        {
            case "texture2d":
                _texture = AssetManager.LoadAsset<Texture2D>(_metaData.Source);
                break;
            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }
    }

    private void LoadFromXml(string name)
    {
        var xml = AssetManager.LoadAsset<string>(name);
        if (xml == null)
        {
            throw new ArgumentNullException("xml", "XML data cannot be null.");
        }

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SpriteDataXml), "http://schemas.coreessentials.monogame/2025/sprite");
            using (StringReader reader = new StringReader(xml))
            {
                var xmlData = (SpriteDataXml)serializer.Deserialize(reader);
                
                // Convert XML data to SpriteMeta format
                _metaData = new SpriteMeta
                {
                    SourceType = xmlData.SourceType,
                    Source = xmlData.Source,
                    Size = new Size
                    {
                        Width = xmlData.Size.Width,
                        Height = xmlData.Size.Height
                    },
                    Origin = new Origin
                    {
                        X = xmlData.Origin.X,
                        Y = xmlData.Origin.Y
                    }
                };
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize XML sprite metadata: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Draws the sprite at the specified position with the given parameters.
    /// Also draws a debug outline around the sprite bounds.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="color">The color to tint the sprite with.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="effects">Sprite effects like flipping horizontally or vertically.</param>
    /// <param name="layerDepth">The layer depth to draw the sprite at (0 to 1).</param>
    /// <exception cref="InvalidOperationException">Thrown when the source type is unknown.</exception>
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

    /// <summary>
    /// Gets the size of the sprite.
    /// </summary>
    /// <returns>A Vector2 containing the width and height of the sprite in pixels.</returns>
    public Vector2 GetSize()
    {
        Vector2 size = new Vector2(_metaData.Size.Width, _metaData.Size.Height);
        return size;
    }

    /// <summary>
    /// Represents the size dimensions of a sprite.
    /// </summary>
    private class Size
    {
        /// <summary>
        /// Gets or sets the width of the sprite in pixels.
        /// </summary>
        public float Width { get; set; }
        
        /// <summary>
        /// Gets or sets the height of the sprite in pixels.
        /// </summary>
        public float Height { get; set; }
    }

    /// <summary>
    /// Represents the origin point of a sprite (the pivot point for rotation and positioning).
    /// </summary>
    private class Origin
    {
        /// <summary>
        /// Gets or sets the X coordinate of the origin point.
        /// </summary>
        public float X { get; set; }
        
        /// <summary>
        /// Gets or sets the Y coordinate of the origin point.
        /// </summary>
        public float Y { get; set; }
    }

    /// <summary>
    /// Contains metadata about a sprite, loaded from XML.
    /// </summary>
    private class SpriteMeta
    {
        /// <summary>
        /// Gets or sets the type of source for the sprite (e.g., "texture2d").
        /// </summary>
        public string SourceType { get; set; }
        
        /// <summary>
        /// Gets or sets the source asset name.
        /// </summary>
        public string Source { get; set; }
        
        /// <summary>
        /// Gets or sets the size of the sprite.
        /// </summary>
        public Size Size { get; set; }
        
        /// <summary>
        /// Gets or sets the origin point of the sprite.
        /// </summary>
        public Origin Origin { get; set; }
    }

    /// <summary>
    /// XML serializable class for sprite data
    /// </summary>
    [XmlRoot("SpriteData", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
    public class SpriteDataXml
    {
        public string SourceType { get; set; }
        public string Source { get; set; }
        
        public SizeXml Size { get; set; }
        public OriginXml Origin { get; set; }
        
        public class SizeXml
        {
            public float Width { get; set; }
            public float Height { get; set; }
        }
        
        public class OriginXml
        {
            public float X { get; set; }
            public float Y { get; set; }
        }
    }
}
