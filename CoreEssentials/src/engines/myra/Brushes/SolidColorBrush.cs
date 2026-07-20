using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.Engines.Myra.Brushes;

/// <summary>
/// Wrapper for a Myra SolidBrush, implementing IBrush interface.
/// </summary>
public class SolidColorBrush : BrushBase
{
    /// <summary>
    /// Creates a new SolidColorBrush wrapping a Myra SolidBrush with the specified color.
    /// </summary>
    public SolidColorBrush(Color color) : base(new SolidBrush(color))
    {
    }

    /// <inheritdoc />
    public override Color Color
    {
        get => ((SolidBrush)MyraBrush).Color;
        set => ((SolidBrush)MyraBrush).Color = value;
    }
}
