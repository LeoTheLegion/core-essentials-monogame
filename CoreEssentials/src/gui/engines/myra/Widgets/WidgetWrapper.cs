using System.Collections.Concurrent;
using Myra.Graphics2D.UI;
using CoreEssentials.GUI.Types;
using MyraHorizontalAlignment = Myra.Graphics2D.UI.HorizontalAlignment;
using MyraVerticalAlignment = Myra.Graphics2D.UI.VerticalAlignment;

namespace CoreEssentials.GUI.Engines.Myra.Widgets;

/// <summary>
/// Base class that tracks the relationship between a Myra Widget and its CoreEssentials wrapper.
/// Uses a registry to look up wrappers from either direction.
/// </summary>
public abstract class WidgetWrapper : IWidget
{
    private static readonly ConcurrentDictionary<Widget, WidgetWrapper> _registry = new();

    /// <summary>
    /// Gets the underlying Myra widget instance.
    /// </summary>
    protected Widget MyraWidget { get; }

    static WidgetWrapper()
    {
        // Clean up registry entries when widgets are removed from containers (best-effort)
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WidgetWrapper"/> class.
    /// </summary>
    /// <param name="myraWidget">The underlying Myra widget.</param>
    protected WidgetWrapper(Widget myraWidget)
    {
        MyraWidget = myraWidget;
        _registry[myraWidget] = this;
    }

    /// <summary>
    /// Tries to get the CoreEssentials wrapper for a given Myra widget.
    /// </summary>
    internal static WidgetWrapper? TryGetFromMyra(Widget myraWidget)
    {
        return _registry.GetValueOrDefault(myraWidget);
    }

    /// <summary>
    /// Unwraps a user-facing IWidget to its underlying Myra Widget.
    /// </summary>
    internal static Widget Unwrap(IWidget widget)
    {
        return widget switch
        {
            WidgetWrapper wrapper => wrapper.MyraWidget,
            _ => throw new System.ArgumentException("Widget is not a CoreEssentials wrapper", nameof(widget))
        };
    }

    /// <inheritdoc />
    public float Width
    {
        get => (MyraWidget.Width ?? 0) * 1.0f;
        set => MyraWidget.Width = (int)value;
    }

    /// <inheritdoc />
    public float Height
    {
        get => (MyraWidget.Height ?? 0) * 1.0f;
        set => MyraWidget.Height = (int)value;
    }

    /// <inheritdoc />
    public bool Visible
    {
        get => MyraWidget.Visible;
        set => MyraWidget.Visible = value;
    }

    /// <inheritdoc />
    public bool Enabled
    {
        get => MyraWidget.Enabled;
        set => MyraWidget.Enabled = value;
    }

    /// <inheritdoc />
    public bool IsMouseInside => MyraWidget.IsMouseInside;

    /// <inheritdoc />
    public bool IsKeyboardFocused => MyraWidget.IsKeyboardFocused;

    /// <inheritdoc />
    public Microsoft.Xna.Framework.Vector2 Position
    {
        get => new(MyraWidget.Left, MyraWidget.Top);
        set
        {
            MyraWidget.Left = (int)value.X;
            MyraWidget.Top = (int)value.Y;
        }
    }

    /// <inheritdoc />
    public CoreEssentials.GUI.Types.Thickness Margin
    {
        get => new(MyraWidget.Margin.Left, MyraWidget.Margin.Top, MyraWidget.Margin.Right, MyraWidget.Margin.Bottom);
        set => MyraWidget.Margin = new global::Myra.Graphics2D.Thickness((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
    }

    /// <inheritdoc />
    public CoreEssentials.GUI.Types.HorizontalAlignment HorizontalAlignment
    {
        get => MapHorizontalAlignment(MyraWidget.HorizontalAlignment);
        set => MyraWidget.HorizontalAlignment = MapHorizontalAlignment(value);
    }

    /// <inheritdoc />
    public CoreEssentials.GUI.Types.VerticalAlignment VerticalAlignment
    {
        get => MapVerticalAlignment(MyraWidget.VerticalAlignment);
        set => MyraWidget.VerticalAlignment = MapVerticalAlignment(value);
    }

    private static CoreEssentials.GUI.Types.HorizontalAlignment MapHorizontalAlignment(MyraHorizontalAlignment myraAlignment)
    {
        return myraAlignment switch
        {
            MyraHorizontalAlignment.Left => CoreEssentials.GUI.Types.HorizontalAlignment.Left,
            MyraHorizontalAlignment.Center => CoreEssentials.GUI.Types.HorizontalAlignment.Center,
            MyraHorizontalAlignment.Right => CoreEssentials.GUI.Types.HorizontalAlignment.Right,
            MyraHorizontalAlignment.Stretch => CoreEssentials.GUI.Types.HorizontalAlignment.Stretch,
            _ => CoreEssentials.GUI.Types.HorizontalAlignment.Left
        };
    }

    private static MyraHorizontalAlignment MapHorizontalAlignment(CoreEssentials.GUI.Types.HorizontalAlignment alignment)
    {
        return alignment switch
        {
            CoreEssentials.GUI.Types.HorizontalAlignment.Left => MyraHorizontalAlignment.Left,
            CoreEssentials.GUI.Types.HorizontalAlignment.Center => MyraHorizontalAlignment.Center,
            CoreEssentials.GUI.Types.HorizontalAlignment.Right => MyraHorizontalAlignment.Right,
            CoreEssentials.GUI.Types.HorizontalAlignment.Stretch => MyraHorizontalAlignment.Stretch,
            _ => MyraHorizontalAlignment.Left
        };
    }

    private static CoreEssentials.GUI.Types.VerticalAlignment MapVerticalAlignment(MyraVerticalAlignment myraAlignment)
    {
        return myraAlignment switch
        {
            MyraVerticalAlignment.Top => CoreEssentials.GUI.Types.VerticalAlignment.Top,
            MyraVerticalAlignment.Center => CoreEssentials.GUI.Types.VerticalAlignment.Center,
            MyraVerticalAlignment.Bottom => CoreEssentials.GUI.Types.VerticalAlignment.Bottom,
            MyraVerticalAlignment.Stretch => CoreEssentials.GUI.Types.VerticalAlignment.Stretch,
            _ => CoreEssentials.GUI.Types.VerticalAlignment.Top
        };
    }

    private static MyraVerticalAlignment MapVerticalAlignment(CoreEssentials.GUI.Types.VerticalAlignment alignment)
    {
        return alignment switch
        {
            CoreEssentials.GUI.Types.VerticalAlignment.Top => MyraVerticalAlignment.Top,
            CoreEssentials.GUI.Types.VerticalAlignment.Center => MyraVerticalAlignment.Center,
            CoreEssentials.GUI.Types.VerticalAlignment.Bottom => MyraVerticalAlignment.Bottom,
            CoreEssentials.GUI.Types.VerticalAlignment.Stretch => MyraVerticalAlignment.Stretch,
            _ => MyraVerticalAlignment.Top
        };
    }

    /// <summary>
    /// Converts an IWidget to its underlying Myra Widget. Returns null if not a wrapper.
    /// </summary>
    internal static Widget? TryUnwrap(IWidget widget)
    {
        return widget switch
        {
            WidgetWrapper wrapper => wrapper.MyraWidget,
            _ => null
        };
    }

    /// <summary>
    /// Converts a Myra Widget to a CoreEssentials IWidget if it has a wrapper registered.
    /// </summary>
    internal static IWidget? TryWrap(Widget myraWidget)
    {
        return TryGetFromMyra(myraWidget);
    }
}
