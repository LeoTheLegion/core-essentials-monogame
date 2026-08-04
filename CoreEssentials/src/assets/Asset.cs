using System;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Base class for all assets in the game.
/// </summary>
public abstract class Asset
{
    /// <summary>
    /// The name of the asset. Cannot be null or empty.
    /// </summary>
    protected string _assetName;

    /// <summary>
    /// Gets the name of the asset.
    /// </summary>
    public string Name {get => _assetName; }

    /// <summary>
    /// Initializes a new instance of the <see cref="Asset"/> class.
    /// </summary>
    /// <param name="name">The name of the asset. Cannot be null or empty.</param>
    /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the name is empty.</exception>
    public Asset(string name)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name), "Asset name cannot be null.");
        }

        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Asset name cannot be empty.", nameof(name));
        }
        
        _assetName = name;
    }

    /// <summary>
    /// Loads the asset using the provided content manager.
    /// </summary>
    /// <param name="contentManager">The content manager to use for loading.</param>
    public abstract void Load(IContentManager contentManager);

    /// <summary>
    /// Unloads the asset using the provided content manager.
    /// </summary>
    /// <param name="contentManager">The content manager to use for unloading.</param>
    public abstract void Unload(IContentManager contentManager);
}
