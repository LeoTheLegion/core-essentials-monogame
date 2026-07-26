using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Engines.Myra.Widgets;

namespace CoreEssentials.GUI.Factory;

/// <summary>
/// The default implementation of the widget factory using the Myra engine.
/// </summary>
public class DefaultWidgetFactory : IWidgetFactory
{
    public IPanel CreatePanel() => new ContainerWidget(new global::Myra.Graphics2D.UI.Panel());
    public ILabel CreateLabel(string text) => new LabelWidget(text);
    public IButton CreateTextButton(string text) => ButtonWidget.CreateTextButton(text);
    public IGrid CreateGrid() => new GridWidget();
}
