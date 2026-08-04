using System.Collections.Generic;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using CoreEssentials.GUI.Types;
using MyraSolidBrush = Myra.Graphics2D.Brushes.SolidBrush;
using MyraIBrush = Myra.Graphics2D.IBrush;

namespace CoreEssentials.GUI.Engines.Myra.Widgets;

/// <summary>
/// Wrapper for a Myra Panel, implementing IContainer and IPanel interfaces.
/// </summary>
public class ContainerWidget : WidgetBase, IPanel
{
    /// <summary>
    /// Gets the underlying Myra Panel instance (typed).
    /// </summary>
    protected Panel Panel => (Panel)MyraWidget;

    /// <inheritdoc />
    public IList<IWidget> Children
    {
        get
        {
            var result = new List<IWidget>();
            foreach (var w in Panel.Widgets)
            {
                if (w is global::Myra.Graphics2D.UI.Widget myra && WidgetWrapper.TryGetFromMyra(myra) is IWidget wrapper)
                    result.Add(wrapper);
            }
            return result;
        }
    }

    /// <inheritdoc />
    public IEnumerable<IWidget> Widgets => GetDescendants(this);

    /// <inheritdoc />
    public void AddChild(IWidget widget)
    {
        var myra = WidgetWrapper.Unwrap(widget);
        Panel.Widgets.Add(myra);
    }

    /// <inheritdoc />
    public void RemoveChild(IWidget widget)
    {
        var myra = WidgetWrapper.Unwrap(widget);
        Panel.Widgets.Remove(myra);
    }

    /// <inheritdoc />
    public void ClearChildren() => Panel.Widgets.Clear();

    /// <inheritdoc />
    public CoreEssentials.GUI.Types.IBrush? Background
    {
        get => Panel.Background is MyraIBrush myraBrush ? ConvertToCoreEssentialsBrush(myraBrush) : null;
        set => Panel.Background = ConvertToMyraBrush(value);
    }

    /// <inheritdoc />
    public CoreEssentials.GUI.Types.Thickness BorderThickness
    {
        get => new(Panel.BorderThickness.Left, Panel.BorderThickness.Top, Panel.BorderThickness.Right, Panel.BorderThickness.Bottom);
        set => Panel.BorderThickness = new global::Myra.Graphics2D.Thickness((int)value.Left, (int)value.Top, (int)value.Right, (int)value.Bottom);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ContainerWidget"/> class.
    /// </summary>
    /// <param name="panel">The underlying Myra Panel widget.</param>
    public ContainerWidget(Panel panel) : base(panel)
    {
    }

    private static MyraIBrush? ConvertToMyraBrush(CoreEssentials.GUI.Types.IBrush? brush)
    {
        if (brush == null) return null;
        return brush switch
        {
            Brushes.SolidColorBrush solid => solid.MyraBrush,
            _ => throw new System.ArgumentException("Unsupported brush type", nameof(brush))
        };
    }

    private static CoreEssentials.GUI.Types.IBrush? ConvertToCoreEssentialsBrush(MyraIBrush myraBrush)
    {
        return myraBrush switch
        {
            MyraSolidBrush solid => new Brushes.SolidColorBrush(solid.Color),
            _ => null
        };
    }

    private static IEnumerable<IWidget> GetDescendants(IWidget widget)
    {
        if (widget is IContainer container)
        {
            foreach (var child in container.Children)
            {
                yield return child;
                foreach (var descendant in GetDescendants(child))
                    yield return descendant;
            }
        }
    }
}
