using CoreEssentials.GUI.Types;
using MyraWidget = Myra.Graphics2D.UI.Widget;

namespace CoreEssentials.GUI.Engines.Myra.Widgets;

/// <summary>
/// Abstract base class for all widget wrappers.
/// Inherits from WidgetWrapper which handles IWidget delegation and the Myra↔CoreEssentials registry.
/// </summary>
public abstract class WidgetBase : WidgetWrapper
{
    /// <summary>
    /// Gets the underlying Myra widget instance (typed).
    /// </summary>
    protected new MyraWidget MyraWidget => base.MyraWidget;

    /// <summary>
    /// Initializes a new instance of the <see cref="WidgetBase"/> class.
    /// </summary>
    /// <param name="myraWidget">The underlying Myra widget.</param>
    protected WidgetBase(MyraWidget myraWidget) : base(myraWidget)
    {
    }
}
