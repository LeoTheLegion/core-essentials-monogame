using Microsoft.Xna.Framework;
using MyraGraphics2D = Myra.Graphics2D;
using MyraSolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.GUI.Engines.Myra.Brushes;

/// <summary>
/// Wrapper for a Myra SolidBrush, implementing IBrush interface directly.
/// Stores opacity as a field since Myra's SolidBrush does not have an Opacity property.
/// </summary>
public class SolidColorBrush : IBrush
{
    /// <summary>
    /// Gets the underlying Myra SolidBrush instance.
    /// </summary>
    public MyraSolidBrush MyraBrush { get; }

    /// <summary>
    /// Stores the opacity value since Myra's SolidBrush does not have an Opacity property.
    /// </summary>
    private float _opacity = 1.0f;

    /// <inheritdoc />
    public Color Color
    {
        get => ApplyOpacity(MyraBrush.Color);
        set => MyraBrush.Color = value;
    }

    /// <inheritdoc />
    public bool IsSolid => true;

    /// <inheritdoc />
    public float Opacity
    {
        get => _opacity;
        set => _opacity = Math.Clamp(value, 0f, 1f);
    }

    /// <summary>
    /// Creates a new SolidColorBrush wrapping a Myra SolidBrush with the specified color.
    /// </summary>
    public SolidColorBrush(Color color)
    {
        MyraBrush = new MyraSolidBrush(color);
    }

    private Color ApplyOpacity(Color color) => new((int)(color.R), (int)(color.G), (int)(color.B), (byte)(color.A * _opacity));
}
