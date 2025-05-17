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
    private Texture2DAsset _texture;
    private SpriteSheet _spriteSheet;
    private SpriteMeta _metaData;
    private int _defaultFrame; // Default frame for sprite sheet rendering

    /// <summary>
    /// Initializes a new instance of the Sprite class.
    /// Loads sprite metadata from an XML file and the associated texture.
    /// </summary>
    /// <param name="name">The name of the sprite asset to load.</param>
    /// <exception cref="ArgumentNullException">Thrown when the asset name or data is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when metadata deserialization fails or the source type is unknown.</exception>
    public Sprite(string name) : base(name)
    {
        
    }

    private void LoadFromXml(string name)
    {
        var xml = (XMLAsset)AssetManager.LoadAsset<XMLAsset>(name);
        if (xml == null)
        {
            throw new ArgumentNullException("xml", "XML data cannot be null.");
        }

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SpriteDataXml), "http://schemas.coreessentials.monogame/2025/sprite");
            using (StringReader reader = new StringReader(xml.XMLContent))
            {
                var xmlData = (SpriteDataXml)serializer.Deserialize(reader);
                
                // Convert XML data to SpriteMeta format
                _metaData = new SpriteMeta
                {
                    SourceType = xmlData.SourceType,
                    Source = xmlData.Source,
                    Size = new Size
                    {
                        Width = xmlData.Size?.Width ?? 0,
                        Height = xmlData.Size?.Height ?? 0
                    },
                    Origin = xmlData.Origin != null ? new Origin
                    {
                        X = xmlData.Origin.X,
                        Y = xmlData.Origin.Y
                    } : new Origin
                    {
                        X = 0,
                        Y = 0
                    },
                    Frame = xmlData.Frame
                };
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize XML sprite metadata: {ex.Message}", ex);
        }
    }    /// <summary>
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
        // Use the default scale of 1.0f (no scaling)
        Draw(spriteBatch, position, color, rotation, Vector2.One, effects, layerDepth);
    }

    /// <summary>
    /// Draws the sprite at the specified position with the given parameters including scale.
    /// Also draws a debug outline around the sprite bounds.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="color">The color to tint the sprite with.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="scale">The scale to apply to the sprite (Vector2.One for no scaling).</param>
    /// <param name="effects">Sprite effects like flipping horizontally or vertically.</param>
    /// <param name="layerDepth">The layer depth to draw the sprite at (0 to 1).</param>
    /// <exception cref="InvalidOperationException">Thrown when the source type is unknown.</exception>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        // Handle drawing based on source type
        Rectangle targetRectangle;
        
        switch (_metaData.SourceType)
        {
            case "texture2d":
                Vector2 TextureScale = new Vector2(_metaData.Size.Width / _texture.Width,
                                      _metaData.Size.Height / _texture.Height);

                // Apply the additional scale factor
                TextureScale *= scale;

                float xFactor = _metaData.Origin.X / _metaData.Size.Width;
                float yFactor = _metaData.Origin.Y / _metaData.Size.Height;

                Vector2 TextureOrigin = new Vector2(
                    _texture.Width * xFactor,
                    _texture.Height * yFactor
                );

                spriteBatch.Draw(_texture.Texture,
                position,
                null,
                color,
                rotation,
                TextureOrigin,
                TextureScale,
                effects,
                layerDepth);

                targetRectangle = new Rectangle(
                    (int)(position.X - _metaData.Origin.X * scale.X), 
                    (int)(position.Y - _metaData.Origin.Y * scale.Y),
                    (int)(GetSize().X * scale.X), 
                    (int)(GetSize().Y * scale.Y)
                );
                break;
                
            case "spritesheet":
                // Use the default frame for spritesheet
                if (_spriteSheet == null)
                {
                    throw new InvalidOperationException("SpriteSheet is null");
                }
                
                // Get the default frame rectangle
                Rectangle sourceRect = _spriteSheet.GetFrame(_defaultFrame);
                
                // Use origin from metadata
                Vector2 origin = _spriteSheet.FrameOrigin;
                
                spriteBatch.Draw(
                    _spriteSheet.Texture,
                    position,
                    sourceRect,
                    color,
                    rotation,
                    origin,
                    scale,
                    effects,
                    layerDepth
                );
                
                targetRectangle = new Rectangle(
                    (int)(position.X - origin.X * scale.X), 
                    (int)(position.Y - origin.Y * scale.Y),
                    (int)(sourceRect.Width * scale.X), 
                    (int)(sourceRect.Height * scale.Y)
                );
                break;
                
            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }

        Debug.Primitives.DrawRectangle(spriteBatch, targetRectangle, Color.Red, 1f);
    }    /// <summary>
    /// Draws the sprite at the specified position with the given parameters including a uniform scale.
    /// Also draws a debug outline around the sprite bounds.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="color">The color to tint the sprite with.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="scale">The uniform scale to apply to both width and height (1.0f for no scaling).</param>
    /// <param name="effects">Sprite effects like flipping horizontally or vertically.</param>
    /// <param name="layerDepth">The layer depth to draw the sprite at (0 to 1).</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, float scale, SpriteEffects effects, float layerDepth)
    {
        Draw(spriteBatch, position, color, rotation, new Vector2(scale, scale), effects, layerDepth);
    }

    /// <summary>
    /// Gets the size of the sprite.
    /// </summary>
    /// <returns>A Vector2 containing the width and height of the sprite in pixels.</returns>
    public Vector2 GetSize()
    {
        if (_metaData.SourceType == "spritesheet" && _spriteSheet != null)
        {
            Vector2 frameSize = _spriteSheet.GetFrameSize();
            return frameSize;
        }
        
        Vector2 size = new Vector2(_metaData.Size.Width, _metaData.Size.Height);
        return size;
    }

    public override void Load(IContentManager contentManager)
    {
        string extension = Path.GetExtension(Name);
        if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            LoadFromXml(Name);
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
                _texture = (Texture2DAsset)AssetManager.LoadAsset<Texture2DAsset>(_metaData.Source);
                break;
            case "spritesheet":
                _spriteSheet = (SpriteSheet)AssetManager.LoadAsset<SpriteSheet>(_metaData.Source);
                _defaultFrame = _metaData.Frame ?? 0; // Use specified frame or default to 0
                break;
            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }
    }

    public override void Unload(IContentManager contentManager)
    {
        if (_texture != null)
        {
            AssetManager.UnloadAsset<Texture2DAsset>(_texture.Name);
            _texture = null;
        }
        
        if (_spriteSheet != null)
        {
            AssetManager.UnloadAsset<SpriteSheet>(_texture.Name);
            _spriteSheet = null;
        }
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
        public Origin? Origin { get; set; }
        
        /// <summary>
        /// Gets or sets the initial frame for sprite sheet animations.
        /// </summary>
        public int? Frame { get; set; }
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
        [XmlElement(IsNullable = true)]
        public OriginXml? Origin { get; set; }
        
        [XmlElement(IsNullable = true)]
        public int? Frame { get; set; }
        
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
