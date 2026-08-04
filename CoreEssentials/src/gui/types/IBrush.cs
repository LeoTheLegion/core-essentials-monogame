using Microsoft.Xna.Framework;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Abstraction for background and styling rendering.
/// </summary>
public interface IBrush
{
    /// <summary>
    /// Gets the color of this brush.
    /// </summary>
    Color Color { get; }

    /// <summary>
    /// Gets a value indicating whether this is a solid color brush.
    /// </summary>
    bool IsSolid { get; }

    /// <summary>
    /// Gets or sets the opacity of this brush (0.0 to 1.0).
    /// </summary>
    float Opacity { get; set; }
}
