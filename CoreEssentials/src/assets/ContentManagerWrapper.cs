using System;
using CoreEssentials.Assets;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Content;

namespace CoreEssentials.Assets;

public class ContentManagerWrapper : IContentManager
{
    private ContentManager contentManager;

    public ContentManagerWrapper(ContentManager contentManager)
    {
        this.contentManager = contentManager ?? throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
    }

    public void Unload(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            throw new ArgumentException("Asset name cannot be null or empty.", nameof(assetName));
        }

        contentManager.UnloadAsset(assetName);
    }

    public T Load<T>(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            throw new ArgumentException("Asset name cannot be null or empty.", nameof(assetName));
        }

        return contentManager.Load<T>(assetName);
    }
}
