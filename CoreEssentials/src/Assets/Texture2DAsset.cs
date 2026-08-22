using System;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;
/// <summary>
/// Represents a texture asset that loads and manages Texture2D resources.
/// </summary>
public class Texture2DAsset : Asset
{
    private Texture2D? _texture2D;
    /// <summary>
    /// Gets the loaded Texture2D.
    /// </summary>
    public Texture2D? Texture => _texture2D;

    /// <summary>
    /// Gets the width of the loaded Texture2D.
    /// </summary>
    public int Width => _texture2D?.Width ?? 0;
    /// <summary>
    /// Gets the height of the loaded Texture2D.
    /// </summary>
    public int Height => _texture2D?.Height ?? 0;
    /// <summary>
    /// Initializes a new instance of the Texture2DAsset class.
    /// </summary>
    /// <param name="name">The name of the texture asset.</param>
    public Texture2DAsset(string name) : base(name)
    {
    }
    /// <summary>
    /// Loads the Texture2D from content.
    /// </summary>
    /// <param name="contentManager">The content manager to use for loading.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content manager is null.</exception>
    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _texture2D = contentManager.Load<Texture2D>(_assetName);
    }
    /// <summary>
    /// Unloads the Texture2D resource.
    /// </summary>
    /// <param name="contentManager">The content manager to use for unloading.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content manager is null.</exception>
    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        if (_texture2D != null)
        {
            contentManager.Unload(_assetName);
            _texture2D.Dispose();
            _texture2D = null;
        }
    }
}
