using System;
using System.Xml.Serialization;
using System.IO;
using CoreEssentials.Debugging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents a sprite sheet asset that can be divided into a grid of frames.
/// Provides frame data that can be used with the Sprite class for drawing.
/// </summary>
public class SpriteSheet : Asset
{
    private Texture2DAsset? _texture;
    private SpriteSheetMetadata? _metaData;
    private Rectangle[]? _frames;
    
    /// <summary>
    /// Gets the texture used by this sprite sheet.
    /// </summary>
    public Texture2D? Texture => _texture?.Texture;
    
    /// <summary>
    /// Gets the origin point for all frames in this sprite sheet.
    /// </summary>
    public virtual Vector2 FrameOrigin
    {
        get
        {
            if (_metaData == null)
            {
                throw new InvalidOperationException("Sprite sheet metadata is not loaded.");
            }
            if (_metaData.Origin == null)
            {
                throw new InvalidOperationException("Sprite sheet origin metadata is not loaded.");
            }
            return new Vector2(_metaData.Origin.X, _metaData.Origin.Y);
        }
    }

    /// <summary>
    /// Initializes a new instance of the SpriteSheet class.
    /// Loads sprite sheet metadata from an XML file and the associated texture.
    /// </summary>
    /// <param name="name">The name of the sprite sheet asset to load.</param>
    /// <exception cref="ArgumentNullException">Thrown when the asset name or data is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when metadata deserialization fails or the source type is unknown.</exception>
    public SpriteSheet(string name) : base(name)
    {
        
    }
    
    private void InitializeFrames()
    {
        if (_texture == null || _metaData == null)
        {
            throw new InvalidOperationException("Cannot initialize frames without loaded texture and metadata.");
        }
        if (_metaData.Grid == null)
        {
            throw new InvalidOperationException("Sprite sheet grid metadata is not loaded.");
        }
        // Calculate frame dimensions based on grid
        int frameWidth = (int)(_texture.Width / _metaData.Grid.Columns);
        int frameHeight = (int)(_texture.Height / _metaData.Grid.Rows);
        
        // Create frame rectangles for each cell in the grid
        _frames = new Rectangle[_metaData.Grid.Rows * _metaData.Grid.Columns];
        
        for (int row = 0; row < _metaData.Grid.Rows; row++)
        {
            for (int col = 0; col < _metaData.Grid.Columns; col++)
            {
                int index = row * _metaData.Grid.Columns + col;
                _frames[index] = new Rectangle(
                    col * frameWidth,
                    row * frameHeight,
                    frameWidth,
                    frameHeight
                );
            }
        }
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
            XmlSerializer serializer = new XmlSerializer(typeof(SpriteSheetDataXml), "http://schemas.coreessentials.monogame/2025/spritesheet");
            if (xml == null || xml.XMLContent == null)
                throw new InvalidOperationException("XML content is missing.");
            using (StringReader reader = new StringReader(xml.XMLContent))
            {
                var xmlData = (SpriteSheetDataXml?)serializer.Deserialize(reader);
                
                if (xmlData == null)
                {
                    throw new InvalidOperationException("Failed to deserialize XML sprite sheet metadata.");
                }
                if (xmlData.SourceType == null)
                {
                    throw new InvalidOperationException("Sprite sheet metadata source type cannot be null.");
                }
                if (xmlData.Grid == null)
                {
                    throw new InvalidOperationException("Sprite sheet metadata grid cannot be null.");
                }
                if (xmlData.Origin == null)
                {
                    throw new InvalidOperationException("Sprite sheet metadata origin cannot be null.");
                }
                // Convert XML data to SpriteMeta format
                _metaData = new SpriteSheetMetadata
                {
                    SourceType = xmlData.SourceType,
                    Source = xmlData.Source,
                    Grid = new Grid
                    {
                        Rows = xmlData.Grid.Rows,
                        Columns = xmlData.Grid.Columns
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
            throw new InvalidOperationException($"Failed to deserialize XML sprite sheet metadata: {ex.Message}", ex);
        }
    }
    
    /// <summary>
    /// Gets the number of rows in the sprite sheet grid.
    /// </summary>
    public int Rows
    {
        get
        {
            if (_metaData == null)
            {
                throw new InvalidOperationException("Sprite sheet metadata is not loaded.");
            }
            if (_metaData.Grid == null)
            {
                throw new InvalidOperationException("Sprite sheet grid metadata is not loaded.");
            }
            return _metaData.Grid.Rows;
        }
    }
    
    /// <summary>
    /// Gets the number of columns in the sprite sheet grid.
    /// </summary>
    public int Columns
    {
        get
        {
            if (_metaData == null)
            {
                throw new InvalidOperationException("Sprite sheet metadata is not loaded.");
            }
            if (_metaData.Grid == null)
            {
                throw new InvalidOperationException("Sprite sheet grid metadata is not loaded.");
            }
            return _metaData.Grid.Columns;
        }
    }
    
    /// <summary>
    /// Gets the total number of frames in the sprite sheet.
    /// </summary>
    /// <returns>The total number of frames.</returns>
    public int GetFrameCount()
    {
        if (_frames == null)
        {
            throw new InvalidOperationException("Sprite sheet frames are not initialized.");
        }
        return _frames.Length;
    }
    
    /// <summary>
    /// Gets the dimensions of a single frame.
    /// </summary>
    /// <returns>A Vector2 containing the width and height of a single frame.</returns>
    public virtual Vector2 GetFrameSize()
    {
        if (_frames == null)
        {
            throw new InvalidOperationException("Sprite sheet frames are not initialized.");
        }
        if (_frames.Length > 0)
        {
            return new Vector2(_frames[0].Width, _frames[0].Height);
        }
        return Vector2.Zero;
    }
    
    /// <summary>
    /// Gets a specific frame from the sprite sheet.
    /// </summary>
    /// <param name="index">The index of the frame to get.</param>
    /// <returns>The rectangle defining the frame's position in the texture.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is out of range.</exception>
    public virtual Rectangle GetFrame(int index)
    {
        if (_frames == null)
        {
            throw new InvalidOperationException("Sprite sheet frames are not initialized.");
        }
        if (index < 0 || index >= _frames.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(index), "Frame index is out of range");
        }
        return _frames[index];
    }
    
    /// <summary>
    /// Gets a specific frame from the sprite sheet using row and column coordinates.
    /// </summary>
    /// <param name="row">The row of the frame (0-based).</param>
    /// <param name="column">The column of the frame (0-based).</param>
    /// <returns>The rectangle defining the frame's position in the texture.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when coordinates are out of range.</exception>
    public Rectangle GetFrameAt(int row, int column)
    {
        if (_metaData == null)
        {
            throw new InvalidOperationException("Sprite sheet metadata is not loaded.");
        }
        if (_frames == null)
        {
            throw new InvalidOperationException("Sprite sheet frames are not initialized.");
        }
        if (_metaData.Grid == null)
        {
            throw new InvalidOperationException("Sprite sheet grid metadata is not loaded.");
        }
        if (row < 0 || row >= _metaData.Grid.Rows || column < 0 || column >= _metaData.Grid.Columns)
        {
            throw new ArgumentOutOfRangeException(
                $"Frame coordinates ({row}, {column}) are out of range. Grid is {_metaData.Grid.Rows}x{_metaData.Grid.Columns}");
        }
        
        int index = row * _metaData.Grid.Columns + column;
        return _frames[index];
    }
    /// <summary>
    /// Loads the sprite sheet metadata and texture from content.
    /// </summary>
    /// <param name="contentManager">The content manager to use for loading.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content manager is null.</exception>
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
            throw new InvalidOperationException($"Unsupported sprite sheet data format: {extension}. Use .xml format");
        }

        if (_metaData == null)
        {
            throw new InvalidOperationException("Sprite sheet metadata is not loaded.");
        }

        if (_metaData.SourceType == null)
        {
            throw new InvalidOperationException("Sprite sheet metadata source type cannot be null.");
        }
        
        switch (_metaData.SourceType)
        {
            case "texture2d":
                if (_metaData.Source == null)
                {
                    throw new InvalidOperationException("Sprite sheet metadata source cannot be null for texture2d.");
                }
                _texture = (Texture2DAsset)AssetManager.LoadAsset<Texture2DAsset>(_metaData.Source);
                break;
            default:
                throw new InvalidOperationException($"Unknown source type: {_metaData.SourceType}");
        }
        
        // Initialize frames based on the grid
        InitializeFrames();
    }
    /// <summary>
    /// Unloads the sprite sheet and its associated texture from content.
    /// </summary>
    /// <param name="contentManager">The content manager to use for unloading.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content manager is null.</exception>
    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager));
        }
        if (_texture != null)
        {
            AssetManager.UnloadAsset<Texture2DAsset>(_texture.Name);
            _texture = null;
        }
        
        _frames = null;
        _metaData = null;
    }

    /// <summary>
    /// Contains metadata about a sprite sheet, loaded from XML.
    /// </summary>
    private class SpriteSheetMetadata
    {
        /// <summary>
        /// Gets or sets the type of source for the sprite sheet (e.g., "texture2d").
        /// </summary>
        public string? SourceType { get; set; }
        
        /// <summary>
        /// Gets or sets the source asset name.
        /// </summary>
        public string? Source { get; set; }
        
        /// <summary>
        /// Gets or sets the grid information for dividing the texture.
        /// </summary>
        public Grid? Grid { get; set; }
        
        /// <summary>
        /// Gets or sets the origin point of the sprite.
        /// </summary>
        public Origin? Origin { get; set; }
    }
    
    /// <summary>
    /// Represents the grid dimensions of a sprite sheet.
    /// </summary>
    private class Grid
    {
        /// <summary>
        /// Gets or sets the number of rows in the grid.
        /// </summary>
        public int Rows { get; set; }
        
        /// <summary>
        /// Gets or sets the number of columns in the grid.
        /// </summary>
        public int Columns { get; set; }
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
    /// XML serializable class for sprite sheet data
    /// </summary>
    [XmlRoot("SpriteSheetData", Namespace = "http://schemas.coreessentials.monogame/2025/spritesheet")]
    public class SpriteSheetDataXml
    {
        /// <summary>
        /// Gets or sets the source type of the sprite sheet (e.g., "texture2
        /// d" or "spritesheet").
        /// </summary>
        public string? SourceType { get; set; }
        /// <summary>
        /// Gets or sets the source asset name of the sprite sheet.
        /// </summary>
        public string? Source { get; set; }
        
        /// <summary>
        /// Gets or sets the grid information for dividing the texture into frames.
        /// </summary>
        public GridXml? Grid { get; set; }
        /// <summary>
        /// Gets or sets the origin point of the sprite sheet.
        /// </summary>
        public OriginXml? Origin { get; set; }
        /// <summary>
        /// XML serializable class for grid data
        /// </summary>
        public class GridXml
        {
            /// <summary>
            /// Gets or sets the number of rows in the grid.
            /// </summary>
            public int Rows { get; set; }
            /// <summary>
            /// Gets or sets the number of columns in the grid.
            /// </summary>
            public int Columns { get; set; }
        }
        /// <summary>
        /// XML serializable class for origin data
        /// </summary>
        public class OriginXml
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
    }
}