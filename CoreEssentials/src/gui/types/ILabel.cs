using Microsoft.Xna.Framework;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Label widget interface for displaying text.
/// </summary>
public interface ILabel : IWidget
{
    /// <summary>
    /// Gets or sets the display text of this label.
    /// </summary>
    string? Text { get; set; }

    /// <summary>
    /// Gets or sets the font used to render this label's text.
    /// The actual font resource is managed by the game's content pipeline.
    /// </summary>
    object? Font { get; set; }

    /// <summary>
    /// Gets or sets the color of this label's text.
    /// </summary>
    Color TextColor { get; set; }
}
