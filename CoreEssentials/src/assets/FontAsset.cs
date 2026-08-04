using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Represents a font asset that loads and manages SpriteFont resources.
/// </summary>
public class FontAsset : Asset
{
    private SpriteFont? _spriteFont;

    /// <summary>
    /// Gets the loaded SpriteFont.
    /// </summary>
    public SpriteFont? Font => _spriteFont;

    /// <summary>
    /// Initializes a new instance of the FontAsset class.
    /// </summary>
    /// <param name="name">The name of the font asset.</param>
    public FontAsset(string name) : base(name)
    {
    }

    /// <summary>
    /// Loads the SpriteFont from content.
    /// </summary>
    /// <param name="contentManager">The content manager to use for loading.</param>
    public override void Load(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        _spriteFont = contentManager.Load<SpriteFont>(_assetName);
    }

    /// <summary>
    /// Unloads the SpriteFont resource.
    /// </summary>
    /// <param name="contentManager">The content manager to use for unloading.</param>
    public override void Unload(IContentManager contentManager)
    {
        if (contentManager == null)
        {
            throw new ArgumentNullException(nameof(contentManager), "Content manager cannot be null.");
        }

        if (_spriteFont != null)
        {
            contentManager.Unload(_assetName);
            _spriteFont = null;
        }
    }

    /// <summary>
    /// Measures the width of a string when rendered with this font.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The width of the text in pixels.</returns>
    public virtual float MeasureString(string text)
    {
        if (_spriteFont == null)
        {
            throw new InvalidOperationException("Font not loaded. Call Load() first.");
        }

        return _spriteFont.MeasureString(text).X;
    }

    /// <summary>
    /// Measures the size of a string when rendered with this font.
    /// </summary>
    /// <param name="text">The text to measure.</param>
    /// <returns>The size of the text in pixels.</returns>
    public virtual Vector2 MeasureStringVector(string text)
    {
        if (_spriteFont == null)
        {
            throw new InvalidOperationException("Font not loaded. Call Load() first.");
        }

        return _spriteFont.MeasureString(text);
    }
}