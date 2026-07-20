using System.Collections.Generic;
using Myra.Graphics2D.UI;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.Engines.Myra.Widgets;

/// <summary>
/// Wrapper for a Myra Panel, implementing IContainer and IPanel interfaces.
/// </summary>
public class ContainerWidget : WidgetBase, IContainer, IPanel
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
                if (w is WidgetBase wrapper)
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
        var myra = Unwrap(widget);
        Panel.Widgets.Add(myra);
    }

    /// <inheritdoc />
    public void RemoveChild(IWidget widget)
    {
        var myra = Unwrap(widget);
        Panel.Widgets.Remove(myra);
    }

    /// <inheritdoc />
    public void ClearChildren() => Panel.Widgets.Clear();

    /// <inheritdoc />
    public IBrush? Background
    {
        get => Panel.Background;
        set => Panel.Background = value;
    }

    /// <inheritdoc />
    public Thickness BorderThickness
    {
        get => new(Panel.BorderThickness.Left, Panel.BorderThickness.Top, Panel.BorderThickness.Right, Panel.BorderThickness.Bottom);
        set => Panel.BorderThickness = value;
    }

    protected ContainerWidget(Panel panel) : base(panel)
    {
    }

    /// <summary>
    /// Unwraps a user-facing IWidget to its underlying Myra Widget.
    /// </summary>
    internal static Widget Unwrap(IWidget widget)
    {
        return widget switch
        {
            WidgetBase wrapper => wrapper.MyraWidget,
            _ => throw new System.ArgumentException("Widget is not a CoreEssentials wrapper", nameof(widget))
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
