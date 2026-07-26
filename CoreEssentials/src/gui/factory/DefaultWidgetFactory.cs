using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Engines.Myra.Widgets;

namespace CoreEssentials.GUI.Factory;

/// <summary>
/// The default implementation of the widget factory using the Myra engine.
/// </summary>
public class DefaultWidgetFactory : IWidgetFactory
{
    /// <summary>Creates a new panel widget.</summary>
    public IPanel CreatePanel() => new ContainerWidget(new global::Myra.Graphics2D.UI.Panel());
    /// <summary>Creates a new label with the specified text.</summary>
    public ILabel CreateLabel(string text) => new LabelWidget(text);
    /// <summary>Creates a new button with the specified text.</summary>
    public IButton CreateTextButton(string text) => ButtonWidget.CreateTextButton(text);
    /// <summary>Creates a new grid widget.</summary>
    public IGrid CreateGrid() => new GridWidget();
}
