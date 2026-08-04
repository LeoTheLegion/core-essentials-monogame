using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;

namespace CoreEssentials.Assets;

/// <summary>
/// A wrapper for the MonoGame Extended ContentManager to provide a simplified interface for loading and unloading
/// assets. This class implements the IContentManager interface, allowing it to be used interchangeably with other content managers.
/// </summary>
public class ContentManagerWrapper : IContentManager
{
    private ContentManager contentManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="ContentManagerWrapper"/> class.
    /// </summary>
    /// <param name="contentManager">The content manager to wrap.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content manager is null.</exception>
    public ContentManagerWrapper(ContentManager contentManager)
    {
        this.contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
    }

    /// <summary>
    /// Unloads the specified asset.
    /// </summary>
    /// <param name="assetName">The name of the asset to unload.</param>
    /// <exception cref="ArgumentException">Thrown when the asset name is null or empty.</exception>
    public void Unload(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            throw new ArgumentException("Asset name cannot be null or empty.", nameof(assetName));
        }

        contentManager.UnloadAsset(assetName);
    }

    /// <summary>
    /// Loads the specified asset of the given type.
    /// </summary>
    /// <typeparam name="T">The type of the asset to load.</typeparam>
    /// <param name="assetName">The name of the asset to load.</param>
    /// <returns>The loaded asset.</returns>
    public T Load<T>(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            throw new ArgumentException("Asset name cannot be null or empty.", nameof(assetName));
        }

        return contentManager.Load<T>(assetName);
    }
}
