using System;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents an asset for a MonoGame Effect.
/// </summary>
public class EffectAsset : Asset
{
    // Initialize with null! to satisfy non-nullable requirement; will be set in Load.
    private Effect _effect = null!;

    /// <summary>
    /// Gets the loaded Effect object.
    /// </summary>
    public Effect Effect => _effect;

    /// <summary>
    /// Initializes a new instance of the <see cref="EffectAsset"/> class.
    /// </summary>
    /// <param name="name">The name of the asset.</param>
    public EffectAsset(string name) : base(name)
    {
        // No additional initialization needed; field is already initialized.
    }

    /// <summary>
    /// Loads the effect asset using the provided content manager.
    /// </summary>
    /// <param name="contentManager">The content manager to load the asset with.</param>
    /// <exception cref="ArgumentNullException">Thrown if contentManager is null.</exception>
    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _effect = contentManager.Load<Effect>(_assetName);
    }

    /// <summary>
    /// Unloads the effect asset.
    /// </summary>
    /// <param name="contentManager">The content manager to unload the asset with.</param>
    /// <exception cref="ArgumentNullException">Thrown if contentManager is null.</exception>
    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        // Effects are not disposed in the same way textures are,
        // and the ContentManager handles their lifecycle.
        // If specific cleanup for Effect is needed, it would go here.
        // For now, we'll just ensure the reference is cleared.
        // Avoid assigning null to a non-nullable field. The ContentManager
        // handles cleanup; we simply clear the reference.
        if (_effect != null)
        {
            contentManager.Unload(_assetName);
            _effect = null!;
        }
    }
}
