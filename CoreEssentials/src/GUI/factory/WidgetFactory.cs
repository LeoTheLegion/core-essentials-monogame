using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Engines.Myra.Widgets;

namespace CoreEssentials.GUI.Factory;

/// <summary>
/// Static factory class that creates widgets via interfaces using the active engine backend.
/// Users call factory methods and receive interface types — concrete widget classes are never exposed.
/// </summary>
public static class WidgetFactory
{
    /// <summary>Gets or sets the singleton factory instance.</summary>
    public static IWidgetFactory Instance { get; set; } = new DefaultWidgetFactory();

    /// <summary>
    /// Creates a new panel widget.
    /// </summary>
    public static IPanel CreatePanel() => Instance.CreatePanel();

    /// <summary>
    /// Creates a new label with the specified text.
    /// </summary>
    public static ILabel CreateLabel(string text) => Instance.CreateLabel(text);

    /// <summary>
    /// Creates a new button with the specified display text.
    /// </summary>
    public static IButton CreateTextButton(string text) => Instance.CreateTextButton(text);

    /// <summary>
    /// Creates a new grid widget for tabular layouts.
    /// </summary>
    public static IGrid CreateGrid() => Instance.CreateGrid();
}

