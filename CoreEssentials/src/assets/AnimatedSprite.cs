using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents an animated sprite that defines a series of animation frames from a sprite sheet.
/// The AnimatedSprite class provides frame definitions but doesn't track animation state.
/// </summary>
public class AnimatedSprite : Asset
{
    private SpriteSheet? _spriteSheet;
    private AnimatedSpriteMetaData? _metaData;
    private int[]? _frames;
    private float _frameRate = 1f / 10f; // Default: 10 FPS
    
    /// <summary>
    /// Gets the total number of frames in the animation sequence.
    /// </summary>
    public int FrameCount => _frames?.Length ?? 0;
    
    /// <summary>
    /// Gets the size of the animated sprite.
    /// </summary>
    public Vector2 SpriteSize => _spriteSheet?.GetFrameSize() ?? Vector2.Zero;
    
    /// <summary>
    /// Gets the underlying SpriteSheet used by this animated sprite.
    /// </summary>
    public SpriteSheet? SpriteSheet => _spriteSheet;

    
    /// <summary>
    /// Gets the array of frame indices used in this animation.
    /// </summary>
    public int[] Frames => _frames;

    /// <summary>
    /// Gets the frame rate (in seconds per frame) defined for this animation.
    /// </summary>
    public float FrameRate => _frameRate;

    /// <summary>
    /// Initializes a new instance of the AnimatedSprite class.
    /// Loads animation metadata from an XML file and the associated sprite sheet.
    /// </summary>
    /// <param name="name">The name of the animated sprite asset to load.</param>
    /// <exception cref="ArgumentNullException">Thrown when the asset name or data is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when metadata deserialization fails or the source type is unknown.</exception>
    public AnimatedSprite(string name) : base(name)
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
            XmlSerializer serializer = new XmlSerializer(typeof(AnimatedSpriteDataXml), "http://schemas.coreessentials.monogame/2025/sprite");
            using (StringReader reader = new StringReader(xml.XMLContent))
            {
                var xmlData = (AnimatedSpriteDataXml)serializer.Deserialize(reader);
                
                // Parse frame indices from comma-separated list
                List<int> framesList = new List<int>();
                if (!string.IsNullOrEmpty(xmlData.Frames))
                {
                    string[] frameStrings = xmlData.Frames.Split(',');
                    foreach (var frameString in frameStrings)
                    {
                        if (int.TryParse(frameString.Trim(), out int frameIndex))
                        {
                            framesList.Add(frameIndex);
                        }
                    }
                }
                
                // Parse frame rate (default to 10 if not specified)
                float frameRate = 10f; // Default: 10 frames per second
                if (xmlData.FrameRate != null)
                {
                    float.TryParse(xmlData.FrameRate, out frameRate);
                    
                    // Make sure the frame rate is positive
                    if (frameRate <= 0)
                        frameRate = 10f; // Default to 10 frames per second
                }
                
                // Convert XML data to metadata format
                _metaData = new AnimatedSpriteMetaData
                {
                    SourceType = xmlData.SourceType,
                    Source = xmlData.Source,
                    Size = new Size
                    {
                        Width = xmlData.Size?.Width ?? 0,
                        Height = xmlData.Size?.Height ?? 0
                    }
                };
                
                // Set the frames array
                _frames = framesList.ToArray();
                
                // If no frames were specified, create a default sequence
                if (_frames.Length == 0)
                {
                    _frames = new int[] { 0 };
                }
                
                // Set the frame rate - properly convert from frames per second to seconds per frame
                _frameRate = 1f / frameRate; // Correct calculation: seconds per frame = 1 / frames per second
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize XML animated sprite metadata: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Draws a specific frame of the animation
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="frameIndex">The index of the frame in the animation sequence to draw.</param>
    /// <param name="color">The color to tint the sprite with.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="effects">Sprite effects like flipping horizontally or vertically.</param>
    /// <param name="layerDepth">The layer depth to draw the sprite at (0 to 1).</param>
    /// <exception cref="InvalidOperationException">Thrown when the sprite sheet is null or frames array is empty.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown when the frame index is out of range.</exception>
    public void DrawFrame(SpriteBatch spriteBatch, Vector2 position, int frameIndex, Color color, 
                        float rotation = 0f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
    {
        if (_spriteSheet == null)
        {
            throw new InvalidOperationException("SpriteSheet is null");
        }
        
        if (_frames == null || _frames.Length == 0)
        {
            throw new InvalidOperationException("Animation frames array is empty");
        }
        
        if (frameIndex < 0 || frameIndex >= _frames.Length)
        {
            throw new IndexOutOfRangeException($"Frame index {frameIndex} is out of range (0-{_frames.Length - 1})");
        }
        
        // Get the frame from the sequence
        int spriteSheetFrameIndex = _frames[frameIndex]; 
        
        // Get the source rectangle for the frame
        Rectangle sourceRect = _spriteSheet.GetFrame(spriteSheetFrameIndex);
        
        // Use origin from sprite sheet
        Vector2 origin = _spriteSheet.FrameOrigin;
        
        // Draw the frame
        spriteBatch.Draw(
            _spriteSheet.Texture,
            position,
            sourceRect,
            color,
            rotation,
            origin,
            1.0f,
            effects,
            layerDepth
        );
        
        // Draw debug outline if needed
        Rectangle targetRectangle = new Rectangle(
            (int)(position.X - origin.X), 
            (int)(position.Y - origin.Y),
            sourceRect.Width, 
            sourceRect.Height
        );
        
        Debug.Primitives.DrawRectangle(spriteBatch, targetRectangle, Color.Red, 1f);
    }

    /// <summary>
    /// Loads the animated sprite asset from its XML configuration file.
    /// Deserializes animation metadata including frame sequence and timing information,
    /// then loads the associated sprite sheet referenced in the metadata.
    /// </summary>
    /// <param name="contentManager">The content manager used to load assets.</param>
    /// <exception cref="ArgumentNullException">Thrown when XML asset data is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when deserialization fails, source type is unsupported, or metadata is incomplete.</exception>

    public override void Load(IContentManager contentManager)
    {
        string extension = Path.GetExtension(Name);
        if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            LoadFromXml(Name);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported animated sprite data format: {extension}. Use .xml format");
        }

        if (_metaData.SourceType == null)
        {
            throw new InvalidOperationException("Animated sprite metadata source type cannot be null.");
        }
        
        // Only support spritesheet source type
        if (_metaData.SourceType != "spritesheet")
        {
            throw new InvalidOperationException($"Unsupported source type for animated sprite: {_metaData.SourceType}. Only spritesheet is supported.");
        }

        // Load the sprite sheet
        _spriteSheet = (SpriteSheet)AssetManager.LoadAsset<SpriteSheet>(_metaData.Source);
    }

    /// <summary>
    /// Unloads the animated sprite asset and releases associated resources.
    /// Frees the sprite sheet reference and clears metadata and frames arrays.
    /// </summary>
    /// <param name="contentManager">The content manager used to unload assets.</param>

    public override void Unload(IContentManager contentManager)
    {
        if (_spriteSheet != null)
        {
            AssetManager.UnloadAsset<SpriteSheet>(_spriteSheet.Name);
            _spriteSheet = null;
        }
        
        // Clear the metadata and frames
        _metaData = null;
        _frames = null;
    }

    /// <summary>
    /// Represents the size dimensions of an animated sprite.
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
    /// Contains metadata about an animated sprite, loaded from XML.
    /// </summary>
    private class AnimatedSpriteMetaData
    {
        /// <summary>
        /// Gets or sets the type of source for the sprite (always "spritesheet" for animated sprites).
        /// </summary>
        public string SourceType { get; set; } = "";

        /// <summary>
        /// Gets or sets the source sprite sheet asset name.
        /// </summary>
        public string Source { get; set; } = "";

        /// <summary>
        /// Gets or sets the size of the sprite.
        /// </summary>
        public Size Size { get; set; } = new();
    }
    
    /// <summary>
    /// XML serializable class for animated sprite data.
    /// Contains all information needed to configure an animated sprite from a spritesheet source.
    /// </summary>
    [XmlRoot("AnimatedSpriteData", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
    public class AnimatedSpriteDataXml
    {
        /// <summary>
        /// Gets or sets the type of source for the sprite (always "spritesheet" for animated sprites).
        /// </summary>
        [XmlElement("SourceType", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
        public string SourceType { get; set; } = "";

        /// <summary>
        /// Gets or sets the name of the source sprite sheet asset to use.
        /// </summary>
        [XmlElement("Source", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
        public string Source { get; set; } = "";

        /// <summary>
        /// Gets or sets the optional size dimensions of individual frames in pixels.
        /// </summary>
        [XmlElement("Size", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
        public SizeXml? Size { get; set; }

        /// <summary>
        /// Gets or sets a comma-separated list of frame indices to include in the animation sequence.
        /// If not specified, defaults to using only frame 0.
        /// </summary>
        [XmlElement("Frames", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
        public string? Frames { get; set; }

        /// <summary>
        /// Gets or sets the animation speed in frames per second. If not specified, defaults to 10 FPS.
        /// </summary>
        [XmlElement("FrameRate", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
        public string? FrameRate { get; set; }

        /// <summary>
        /// Represents the size dimensions of individual frames in pixels.
        /// </summary>
        [XmlRoot("Size", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
        public class SizeXml
        {
            /// <summary>
            /// Gets or sets the width of each frame in pixels.
            /// </summary>
            [XmlElement("Width", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
            public float Width { get; set; }

            /// <summary>
            /// Gets or sets the height of each frame in pixels.
            /// </summary>
            [XmlElement("Height", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
            public float Height { get; set; }
        }
    }
}