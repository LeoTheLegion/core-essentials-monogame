using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.Engines.Myra.Brushes;

/// <summary>
/// Abstract base class for all brush wrappers. Holds the underlying Myra Brush reference
/// and delegates IBrush properties to it.
/// </summary>
public abstract class BrushBase : IBrush
{
    /// <summary>
    /// Gets the underlying Myra Brush instance.
    /// </summary>
    protected Brush MyraBrush { get; }

    /// <inheritdoc />
    public Color Color => MyraBrush.Color;

    /// <inheritdoc />
    public bool IsSolid => this is SolidColorBrush;

    /// <inheritdoc />
    public float Opacity
    {
        get => MyraBrush.Opacity;
        set => MyraBrush.Opacity = value;
    }

    protected BrushBase(Brush myraBrush)
    {
        MyraBrush = myraBrush;
    }
}
