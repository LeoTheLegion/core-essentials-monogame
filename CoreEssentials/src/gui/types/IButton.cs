using System;

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
    /// Occurs when this button is clicked.
    /// </summary>
    event Action<IButton>? Clicked;
}
