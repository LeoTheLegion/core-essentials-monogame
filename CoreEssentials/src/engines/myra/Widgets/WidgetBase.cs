using Microsoft.Xna.Framework;
using Myra.Graphics2D.UI;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.Engines.Myra.Widgets;

/// <summary>
/// Abstract base class for all widget wrappers. Holds the underlying Myra widget reference
/// and delegates IWidget properties to it.
/// </summary>
public abstract class WidgetBase : IWidget
{
    /// <summary>
    /// Gets the underlying Myra widget instance.
    /// </summary>
    protected Widget MyraWidget { get; }

    /// <inheritdoc />
    public float Width
    {
        get => MyraWidget.Width ?? 0;
        set => MyraWidget.Width = value;
    }

    /// <inheritdoc />
    public float Height
    {
        get => MyraWidget.Height ?? 0;
        set => MyraWidget.Height = value;
    }

    /// <inheritdoc />
    public bool Visible
    {
        get => MyraWidget.Visible.Value;
        set => MyraWidget.Visible = value;
    }

    /// <inheritdoc />
    public bool Enabled
    {
        get => MyraWidget.Enabled.Value;
        set => MyraWidget.Enabled = value;
    }

    /// <inheritdoc />
    public bool IsMouseInside => MyraWidget.IsMouseInside;

    /// <inheritdoc />
    public bool IsKeyboardFocused => MyraWidget.IsKeyboardFocused;

    /// <inheritdoc />
    public Vector2 Position
    {
        get => new(MyraWidget.X, MyraWidget.Y);
        set
        {
            MyraWidget.X = (int)value.X;
            MyraWidget.Y = (int)value.Y;
        }
    }

    /// <inheritdoc />
    public Thickness Margin
    {
        get => new(MyraWidget.Margin.Left, MyraWidget.Margin.Top, MyraWidget.Margin.Right, MyraWidget.Margin.Bottom);
        set => MyraWidget.Margin = value;
    }

    /// <inheritdoc />
    public HorizontalAlignment HorizontalAlignment
    {
        get => MapHorizontalAlignment(MyraWidget.HorizontalAlignment);
        set => MyraWidget.HorizontalAlignment = MapHorizontalAlignment(value);
    }

    /// <inheritdoc />
    public VerticalAlignment VerticalAlignment
    {
        get => MapVerticalAlignment(MyraWidget.VerticalAlignment);
        set => MyraWidget.VerticalAlignment = MapVerticalAlignment(value);
    }

    protected WidgetBase(Widget myraWidget)
    {
        MyraWidget = myraWidget;
    }

    private static HorizontalAlignment MapHorizontalAlignment(Myra.Graphics2D.HorizontalAlignment myraAlignment)
    {
        return myraAlignment switch
        {
            Myra.Graphics2D.HorizontalAlignment.Left => HorizontalAlignment.Left,
            Myra.Graphics2D.HorizontalAlignment.Center => HorizontalAlignment.Center,
            Myra.Graphics2D.HorizontalAlignment.Right => HorizontalAlignment.Right,
            Myra.Graphics2D.HorizontalAlignment.Stretch => HorizontalAlignment.Stretch,
            _ => HorizontalAlignment.Left
        };
    }

    private static Myra.Graphics2D.HorizontalAlignment MapHorizontalAlignment(HorizontalAlignment alignment)
    {
        return alignment switch
        {
            HorizontalAlignment.Left => Myra.Graphics2D.HorizontalAlignment.Left,
            HorizontalAlignment.Center => Myra.Graphics2D.HorizontalAlignment.Center,
            HorizontalAlignment.Right => Myra.Graphics2D.HorizontalAlignment.Right,
            HorizontalAlignment.Stretch => Myra.Graphics2D.HorizontalAlignment.Stretch,
            _ => Myra.Graphics2D.HorizontalAlignment.Left
        };
    }

    private static VerticalAlignment MapVerticalAlignment(Myra.Graphics2D.VerticalAlignment myraAlignment)
    {
        return myraAlignment switch
        {
            Myra.Graphics2D.VerticalAlignment.Top => VerticalAlignment.Top,
            Myra.Graphics2D.VerticalAlignment.Center => VerticalAlignment.Center,
            Myra.Graphics2D.VerticalAlignment.Bottom => VerticalAlignment.Bottom,
            Myra.Graphics2D.VerticalAlignment.Stretch => VerticalAlignment.Stretch,
            _ => VerticalAlignment.Top
        };
    }

    private static Myra.Graphics2D.VerticalAlignment MapVerticalAlignment(VerticalAlignment alignment)
    {
        return alignment switch
        {
            VerticalAlignment.Top => Myra.Graphics2D.VerticalAlignment.Top,
            VerticalAlignment.Center => Myra.Graphics2D.VerticalAlignment.Center,
            VerticalAlignment.Bottom => Myra.Graphics2D.VerticalAlignment.Bottom,
            VerticalAlignment.Stretch => Myra.Graphics2D.VerticalAlignment.Stretch,
            _ => Myra.Graphics2D.VerticalAlignment.Top
        };
    }
}
