using System;
using Microsoft.Xna.Framework;

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
    /// When set, a solid background of that color is drawn in every visual state.
    /// </summary>
    Color? BackgroundTint { get; set; }

    /// <summary>
    /// Occurs when this button is clicked.
    /// </summary>
    event Action<IButton>? Clicked;
}
