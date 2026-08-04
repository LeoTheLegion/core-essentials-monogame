using CoreEssentials.GUI.Types;

namespace CoreEssentials.GUI.Factory;

/// <summary>
/// Interface for creating UI widgets. This allows for mocking in unit tests.
/// </summary>
public interface IWidgetFactory
{
    /// <summary>Creates a new panel widget.</summary>
    IPanel CreatePanel();
    /// <summary>Creates a new label with the specified text.</summary>
    ILabel CreateLabel(string text);
    /// <summary>Creates a new button with the specified text.</summary>
    IButton CreateTextButton(string text);
    /// <summary>Creates a new grid widget.</summary>
    IGrid CreateGrid();
}
