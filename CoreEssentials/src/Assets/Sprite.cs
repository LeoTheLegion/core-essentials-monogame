using System;
using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents a unified drawable sprite asset backed by either a single texture
/// (<c>texture2d</c> source, one frame) or a sprite sheet (<c>spritesheet</c> source, N frames).
/// A static sprite is simply a one-frame sprite; an animated sprite is an N-frame sprite with a
/// frame sequence and frame rate. Per-entity playback is handled by
/// <see cref="AnimationState"/> / <c>AnimationComponent</c>.
/// </summary>
public class Sprite : Asset
{
    private Texture2DAsset? _texture;
    private SpriteSheet? _spriteSheet;
    private SpriteMeta? _metaData;
    private int[]? _frames;
    private float _frameRate = 1f / 10f; // Default: 10 FPS (seconds per frame)

    private const string SourceTypeSpriteSheet = "spritesheet";
    private const string MetadataNotLoadedMessage = "Sprite metadata is not loaded.";

    /// <summary>
    /// Gets the underlying texture asset for this sprite.
    /// Returns null when this sprite uses a SpriteSheet (sheet textures are not batched directly).
    /// Used for instanced rendering/batching optimization.
    /// </summary>
    public Texture2DAsset? Texture => _texture;

    /// <summary>
    /// Gets the underlying SpriteSheet used by this sprite, or null for a <c>texture2d</c> sprite.
    /// </summary>
    public SpriteSheet? SpriteSheet => _spriteSheet;

    /// <summary>
    /// Gets the total number of frames in this sprite's sequence.
    /// A <c>texture2d</c> sprite has a single frame.
    /// </summary>
    public int FrameCount => _frames?.Length ?? 0;

    /// <summary>
    /// Gets the frame rate in seconds per frame (i.e. 1 / frames-per-second).
    /// </summary>
    public float FrameRate => _frameRate;

    /// <summary>
    /// Gets the array of sprite-sheet frame indices used by this sprite's sequence.
    /// </summary>
    public int[]? Frames => _frames;

    /// <summary>
    /// Gets the size of a single frame of this sprite.
    /// </summary>
    public Vector2 SpriteSize => GetSize();

    /// <summary>
    /// Initializes a new instance of the Sprite class.
    /// </summary>
    /// <param name="name">The name of the sprite asset to load.</param>
    public Sprite(string name) : base(name)
    {
    }

    private void LoadFromXml(string name)
    {
        var xml = AssetManager.LoadAsset<XMLAsset>(name);
        if (xml == null)
        {
            throw new ArgumentNullException(nameof(name), "XML data cannot be null.");
        }

        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SpriteDataXml), "http://schemas.coreessentials.monogame/2025/sprite");
            if (xml.XMLContent == null)
                throw new InvalidOperationException("XML content is missing.");

            using (StringReader reader = new StringReader(xml.XMLContent))
            {
                var xmlData = serializer.Deserialize(reader) as SpriteDataXml
                    ?? throw new InvalidOperationException("Failed to deserialize sprite metadata.");

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
                    Frames = xmlData.Frames,
                    FrameRate = xmlData.FrameRate,
                    Frame = xmlData.Frame
                };
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to deserialize XML sprite metadata: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Draws the first frame of the sprite at the specified position.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, SpriteEffects effects, float layerDepth)
    {
        Draw(spriteBatch, position, color, rotation, Vector2.One, effects, layerDepth);
    }

    /// <summary>
    /// Draws the first frame of the sprite at the specified position with the given scale.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        DrawFrame(spriteBatch, position, 0, color, rotation, scale, effects, layerDepth);
    }

    /// <summary>
    /// Draws the first frame of the sprite at the specified position with a uniform scale.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, float scale, SpriteEffects effects, float layerDepth)
    {
        Draw(spriteBatch, position, color, rotation, new Vector2(scale, scale), effects, layerDepth);
    }

    /// <summary>
    /// Draws a specific frame of the sprite at the specified position.
    /// For a <c>texture2d</c> sprite the frame index is ignored (single frame).
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="frameIndex">The index of the frame in the sprite's sequence to draw.</param>
    /// <param name="color">The color to tint the sprite with.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="effects">Sprite effects like flipping horizontally or vertically.</param>
    /// <param name="layerDepth">The layer depth to draw the sprite at (0 to 1).</param>
    public void DrawFrame(SpriteBatch spriteBatch, Vector2 position, int frameIndex, Color color,
                        float rotation = 0f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
    {
        DrawFrame(spriteBatch, position, frameIndex, color, rotation, Vector2.One, effects, layerDepth);
    }

    /// <summary>
    /// Draws a specific frame of the sprite at the specified position with the given scale.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="frameIndex">The index of the frame in the sprite's sequence to draw.</param>
    /// <param name="color">The color to tint the sprite with.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="scale">The scale to apply to the sprite.</param>
    /// <param name="effects">Sprite effects like flipping horizontally or vertically.</param>
    /// <param name="layerDepth">The layer depth to draw the sprite at (0 to 1).</param>
    /// <exception cref="InvalidOperationException">Thrown when metadata is not loaded or the source is unknown.</exception>
    /// <exception cref="IndexOutOfRangeException">Thrown when the frame index is out of range.</exception>
    public void DrawFrame(SpriteBatch spriteBatch, Vector2 position, int frameIndex, Color color,
                        float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if (_metaData == null)
        {
            throw new InvalidOperationException(MetadataNotLoadedMessage);
        }

        switch (_metaData.SourceType)
        {
            case "texture2d":
                DrawTexture2DFrame(spriteBatch, position, color, rotation, scale, effects, layerDepth);
                break;

            case SourceTypeSpriteSheet:
                DrawSpriteSheetFrame(spriteBatch, position, frameIndex, color, rotation, scale, effects, layerDepth);
                break;

            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }
    }

    /// <summary>
    /// Draws the single texture2d frame.
    /// </summary>
    private void DrawTexture2DFrame(SpriteBatch spriteBatch, Vector2 position, Color color,
        float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if (_texture == null)
            throw new InvalidOperationException("Texture2D is null");
        if (_metaData!.Size == null)
            throw new InvalidOperationException("Sprite metadata size is null");

        Vector2 textureScale = new Vector2(_metaData.Size.Width / _texture.Width,
                              _metaData.Size.Height / _texture.Height);
        textureScale *= scale;

        if (_metaData.Origin == null)
            throw new InvalidOperationException("Sprite metadata origin is null");
        float xFactor = _metaData.Origin.X / _metaData.Size.Width;
        float yFactor = _metaData.Origin.Y / _metaData.Size.Height;

        Vector2 textureOrigin = new Vector2(
            _texture.Width * xFactor,
            _texture.Height * yFactor
        );

        spriteBatch.Draw(_texture.Texture,
            position,
            null,
            color,
            rotation,
            textureOrigin,
            textureScale,
            effects,
            layerDepth);
    }

    /// <summary>
    /// Draws a specific frame from the sprite sheet.
    /// </summary>
    private void DrawSpriteSheetFrame(SpriteBatch spriteBatch, Vector2 position, int frameIndex, Color color,
        float rotation, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        if (_spriteSheet == null)
            throw new InvalidOperationException("SpriteSheet is null");
        if (_frames == null || _frames.Length == 0)
            throw new InvalidOperationException("Sprite frames array is empty");
        if (frameIndex < 0 || frameIndex >= _frames.Length)
            throw new IndexOutOfRangeException($"Frame index {frameIndex} is out of range (0-{_frames.Length - 1})");

        Rectangle sourceRect = _spriteSheet.GetFrame(_frames[frameIndex]);
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
            layerDepth);
    }

    /// <summary>
    /// Gets the size of a single frame of the sprite.
    /// </summary>
    /// <returns>A Vector2 containing the width and height of the sprite in pixels.</returns>
    public virtual Vector2 GetSize()
    {
        if (_metaData == null)
        {
            throw new InvalidOperationException(MetadataNotLoadedMessage);
        }
        if (_metaData.SourceType == SourceTypeSpriteSheet && _spriteSheet != null)
        {
            return _spriteSheet.GetFrameSize();
        }
        if (_metaData.Size == null)
        {
            throw new InvalidOperationException("Sprite metadata size is null");
        }
        return new Vector2(_metaData.Size.Width, _metaData.Size.Height);
    }

    /// <summary>
    /// Gets the pixel origin (pivot) of a single frame of this sprite.
    /// This is the point that is placed at the draw position, so the top-left corner of the
    /// rendered sprite sits at <c>position - origin * scale</c>.
    /// </summary>
    /// <returns>A Vector2 containing the origin in pixels, or <see cref="Vector2.Zero"/> when no origin is defined.</returns>
    public virtual Vector2 GetOrigin()
    {
        if (_metaData == null)
        {
            throw new InvalidOperationException(MetadataNotLoadedMessage);
        }
        if (_metaData.SourceType == SourceTypeSpriteSheet && _spriteSheet != null)
        {
            return _spriteSheet.FrameOrigin;
        }
        if (_metaData.Origin == null)
        {
            return Vector2.Zero;
        }
        return new Vector2(_metaData.Origin.X, _metaData.Origin.Y);
    }

    /// <summary>
    /// Loads the sprite asset, including its metadata and associated texture or sprite sheet.
    /// </summary>
    /// <param name="contentManager">The content manager to use for loading.</param>
    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager));
        }

        string extension = Path.GetExtension(Name);
        if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
        {
            LoadFromXml(Name);
        }
        else
        {
            throw new InvalidOperationException($"Unsupported sprite data format: {extension}. Use .xml format");
        }

        if (_metaData == null)
        {
            throw new InvalidOperationException(MetadataNotLoadedMessage);
        }
        if (_metaData.SourceType == null)
        {
            throw new InvalidOperationException("Sprite metadata source type cannot be null.");
        }

        // Build the frame sequence and frame rate before loading the source.
        BuildFrameSequence();

        switch (_metaData.SourceType)
        {
            case "texture2d":
                if (_metaData.Source == null)
                    throw new InvalidOperationException("Sprite metadata source cannot be null for texture2d.");
                _texture = AssetManager.LoadAsset<Texture2DAsset>(_metaData.Source);
                break;
            case SourceTypeSpriteSheet:
                if (_metaData.Source == null)
                    throw new InvalidOperationException("Sprite metadata source cannot be null for spritesheet.");
                _spriteSheet = AssetManager.LoadAsset<SpriteSheet>(_metaData.Source);
                break;
            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }
    }

    /// <summary>
    /// Builds the frame sequence and frame rate from the loaded metadata.
    /// <list type="bullet">
    /// <item><c>texture2d</c> → single frame (index 0).</item>
    /// <item><c>spritesheet</c> → explicit <c>Frames</c> list, else a single <c>Frame</c>, else frame 0.</item>
    /// </list>
    /// </summary>
    private void BuildFrameSequence()
    {
        // Frame rate: XML stores frames-per-second; store seconds-per-frame.
        float fps = 10f;
        if (_metaData!.FrameRate != null && float.TryParse(_metaData.FrameRate, out float parsed) && parsed > 0)
        {
            fps = parsed;
        }
        _frameRate = 1f / fps;

        if (_metaData.SourceType == "texture2d")
        {
            _frames = new int[] { 0 };
            return;
        }

        // spritesheet
        List<int> framesList = new List<int>();
        if (!string.IsNullOrEmpty(_metaData.Frames))
        {
            foreach (var frameString in _metaData.Frames.Split(','))
            {
                if (int.TryParse(frameString.Trim(), out int frameIndex))
                    framesList.Add(frameIndex);
            }
        }

        if (framesList.Count == 0)
        {
            // Fall back to a single explicit frame, or frame 0.
            framesList.Add(_metaData.Frame ?? 0);
        }

        _frames = framesList.ToArray();
    }

    /// <summary>
    /// Unloads the sprite asset, including its associated texture or sprite sheet.
    /// </summary>
    public override void Unload(IContentManager contentManager)
    {
        if (_texture != null)
        {
            AssetManager.UnloadAsset<Texture2DAsset>(_texture.Name);
            _texture = null;
        }

        if (_spriteSheet != null)
        {
            AssetManager.UnloadAsset<SpriteSheet>(_spriteSheet.Name);
            _spriteSheet = null;
        }

        _metaData = null;
        _frames = null;
    }

    // Test seams: internal accessors so the test project (via InternalsVisibleTo) can drive a
    // Sprite without a GraphicsDevice or the AssetManager. Not part of the public API.
    internal SpriteMeta? TestMetaData { get => _metaData; set => _metaData = value; }
    internal int[]? TestFrames { get => _frames; set => _frames = value; }
    internal Texture2DAsset? TestTexture { get => _texture; set => _texture = value; }
    internal SpriteSheet? TestSpriteSheet { get => _spriteSheet; set => _spriteSheet = value; }
    internal float TestFrameRate { get => _frameRate; set => _frameRate = value; }
    internal void TestBuildFrameSequence() => BuildFrameSequence();

    /// <summary>
    /// Represents the size dimensions of a sprite.
    /// </summary>
    internal class Size
    {
        public float Width { get; set; }
        public float Height { get; set; }
    }

    /// <summary>
    /// Represents the origin point of a sprite (the pivot point for rotation and positioning).
    /// </summary>
    internal class Origin
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    /// <summary>
    /// Contains metadata about a sprite, loaded from XML.
    /// </summary>
    internal class SpriteMeta
    {
        public string? SourceType { get; set; }
        public string? Source { get; set; }
        public Size? Size { get; set; }
        public Origin? Origin { get; set; }
        public string? Frames { get; set; }
        public string? FrameRate { get; set; }
        public int? Frame { get; set; }
    }

    /// <summary>
    /// XML serializable class for the unified sprite data.
    /// Supports both <c>texture2d</c> (single frame) and <c>spritesheet</c> (N frame) sources.
    /// </summary>
    [XmlRoot("SpriteData", Namespace = "http://schemas.coreessentials.monogame/2025/sprite")]
    public class SpriteDataXml
    {
        /// <summary>The source type of the sprite ("texture2d" or "spritesheet").</summary>
        public string? SourceType { get; set; }

        /// <summary>The source asset name (a texture for texture2d, a sprite sheet for spritesheet).</summary>
        public string? Source { get; set; }

        /// <summary>The size of the sprite (required for texture2d; informational for spritesheet).</summary>
        public SizeXml? Size { get; set; }

        /// <summary>The origin of the sprite (required for texture2d).</summary>
        [XmlElement(IsNullable = true)]
        public OriginXml? Origin { get; set; }

        /// <summary>A comma-separated list of sprite-sheet frame indices for the animation sequence.</summary>
        public string? Frames { get; set; }

        /// <summary>The animation speed in frames per second (default 10).</summary>
        public string? FrameRate { get; set; }

        /// <summary>The single frame to use when no <c>Frames</c> list is provided.</summary>
        [XmlElement(IsNullable = true)]
        public int? Frame { get; set; }

        /// <summary>XML serializable class for size data.</summary>
        public class SizeXml
        {
            /// <summary>Gets or sets the width in pixels.</summary>
            public float Width { get; set; }

            /// <summary>Gets or sets the height in pixels.</summary>
            public float Height { get; set; }
        }

        /// <summary>XML serializable class for origin data.</summary>
        public class OriginXml
        {
            /// <summary>Gets or sets the X coordinate of the origin.</summary>
            public float X { get; set; }

            /// <summary>Gets or sets the Y coordinate of the origin.</summary>
            public float Y { get; set; }
        }
    }
}
