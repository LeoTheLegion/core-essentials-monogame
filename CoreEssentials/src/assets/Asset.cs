using System;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

public abstract class Asset
{
    protected string _assetName;

    public string Name {get => _assetName; }

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

    public abstract void Load(IContentManager contentManager);

    public abstract void Unload(IContentManager contentManager);
}
