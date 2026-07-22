using Microsoft.Xna.Framework;
using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Engines.Myra.Brushes;
using MyraIBrush = Myra.Graphics2D.IBrush;
using MyraSolidBrush = Myra.Graphics2D.Brushes.SolidBrush;

namespace CoreEssentials.GUI.Internal;

/// <summary>
/// Static helper class that converts between MonoGame Color and Myra brush types.
/// Users pass Color values — conversion to Myra brushes happens internally.
/// </summary>
public static class ColorAdapter
{
    /// <summary>
    /// Converts a MonoGame Color to a Myra SolidBrush.
    /// </summary>
    public static IBrush ToMyraBrush(Color color) => new SolidColorBrush(color);

    /// <summary>
    /// Creates a SolidColorBrush from this color.
    /// </summary>
    public static IBrush AsBrush(this Color color) => new SolidColorBrush(color);

    /// <summary>
    /// Returns a new Color with the specified alpha value applied.
    /// </summary>
    public static Color WithAlpha(this Color color, byte alpha) => new Color(color.R, color.G, color.B, alpha);
}
