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
    protected new MyraWidget MyraWidget => (MyraWidget)base.MyraWidget;

    protected WidgetBase(MyraWidget myraWidget) : base(myraWidget)
    {
    }
}
