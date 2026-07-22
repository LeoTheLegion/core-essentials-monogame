using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Engines.Myra.Widgets;

namespace CoreEssentials.GUI.Factory;

/// <summary>
/// Static factory class that creates widgets via interfaces using the active engine backend.
/// Users call factory methods and receive interface types — concrete widget classes are never exposed.
/// </summary>
public static class WidgetFactory
{
    /// <summary>
    /// Creates a new panel widget.
    /// </summary>
    public static IPanel CreatePanel() => new ContainerWidget(new global::Myra.Graphics2D.UI.Panel());

    /// <summary>
    /// Creates a new label with the specified text.
    /// </summary>
    public static ILabel CreateLabel(string text) => new LabelWidget(text);

    /// <summary>
    /// Creates a new button with the specified display text.
    /// </summary>
    public static IButton CreateTextButton(string text) => ButtonWidget.CreateTextButton(text);

    /// <summary>
    /// Creates a new grid widget for tabular layouts.
    /// </summary>
    public static IGrid CreateGrid() => new GridWidget();
}
