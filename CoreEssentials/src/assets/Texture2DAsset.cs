using System;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

public class Texture2DAsset : Asset
{
    private Texture2D _texture2D;

    public Texture2D Texture => _texture2D;

    public int Width => _texture2D?.Width ?? 0;
    public int Height => _texture2D?.Height ?? 0;

    public Texture2DAsset(string name) : base(name)
    {
    }

    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _texture2D = contentManager.Load<Texture2D>(_assetName);
    }

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
