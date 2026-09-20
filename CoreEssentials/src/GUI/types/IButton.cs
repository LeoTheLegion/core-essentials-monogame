using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Button widget interface for clickable UI elements.
/// </summary>
public interface IButton : IWidget
{
    /// <summary>
    /// Gets or sets the display text on this button.
    /// </summary>
    string? Text { get; set; }

    /// <summary>
    /// Gets or sets the font used to render this button's text, or null to use the engine default.
    /// The actual font resource is managed by the game's content pipeline / GUI font loader.
    /// </summary>
    object? Font { get; set; }

    /// <summary>
    /// Gets or sets an optional background tint for this button. When null (the default), the
    /// button renders with no opaque background box, so a game-supplied back-plate shows through.
    /// When set, a solid background of that color is drawn in every visual state. Ignored when
    /// <see cref="BackgroundSprite"/> is set (the sprite wins).
    /// </summary>
    Color? BackgroundTint { get; set; }

    /// <summary>
    /// Gets or sets an optional sprite texture drawn as the button's background, stretched to fill
    /// the widget's background area in every visual state. When set, it takes precedence over
    /// <see cref="BackgroundTint"/>. When null (the default), the button falls back to its
    /// <see cref="BackgroundTint"/> behavior.
    /// </summary>
    Texture2D? BackgroundSprite { get; set; }

    /// <summary>
    /// Gets or sets the tint applied to <see cref="BackgroundSprite"/> (default white). Only has an
    /// effect when a background sprite is assigned.
    /// </summary>
    Color BackgroundSpriteTint { get; set; }

    /// <summary>
    /// Occurs when this button is clicked.
    /// </summary>
    event Action<IButton>? Clicked;
}
