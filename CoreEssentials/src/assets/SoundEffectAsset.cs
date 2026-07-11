using System;
using Microsoft.Xna.Framework.Audio;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents a sound effect asset that loads and manages SoundEffect resources.
/// </summary>
public class SoundEffectAsset : Asset
{
    /// <summary>
    /// The loaded SoundEffect.
    /// </summary>
    private SoundEffect? _soundEffect;

    /// <summary>
    /// Gets the loaded SoundEffect.
    /// </summary>
    public SoundEffect? SoundEffect => _soundEffect;

    /// <summary>
    /// Initializes a new instance of the SoundEffectAsset class.
    /// </summary>
    /// <param name="name">The name of the sound effect asset.</param>
    /// <exception cref="ArgumentNullException">Thrown when the name is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the name is empty.</exception>
    public SoundEffectAsset(string name) : base(name)
    {
    }

    /// <summary>
    /// Loads the SoundEffect from content.
    /// </summary>
    /// <param name="contentManager">The content manager to use for loading.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content manager is null.</exception>
    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _soundEffect = contentManager.Load<SoundEffect>(_assetName);
    }

    /// <summary>
    /// Unloads the SoundEffect resource.
    /// </summary>
    /// <param name="contentManager">The content manager to use for unloading.</param>
    /// <exception cref="ArgumentNullException">Thrown when the content manager is null.</exception>
    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _soundEffect = null;
    }
}
