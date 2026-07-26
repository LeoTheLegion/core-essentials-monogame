using CoreEssentials.GUI.Types;

namespace CoreEssentials.GUI.Factory;

/// <summary>
/// Interface for creating UI widgets. This allows for mocking in unit tests.
/// </summary>
public interface IWidgetFactory
{
    IPanel CreatePanel();
    ILabel CreateLabel(string text);
    IButton CreateTextButton(string text);
    IGrid CreateGrid();
}
