using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Assets;

public class SoundEffectAsset : Asset
{
    private SoundEffect _soundEffect;

    public SoundEffect SoundEffect => _soundEffect;

    public SoundEffectAsset(string name) : base(name)
    {
    }

    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _soundEffect = contentManager.Load<SoundEffect>(_assetName);
    }

    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _soundEffect = null;
    }
}
