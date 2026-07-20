using Myra.Graphics2D.UI;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.Engines.Myra.Widgets;

/// <summary>
/// Wrapper for a Myra Button (or TextButton), implementing IButton with event support and static factory.
/// </summary>
public class ButtonWidget : WidgetBase, IButton
{
    /// <summary>
    /// Gets the underlying Myra Button instance (typed).
    /// </summary>
    protected Button Button => (Button)MyraWidget;

    /// <inheritdoc />
    public string? Text
    {
        get => _textContent ?? Button.Content?.ToString();
        set
        {
            _textContent = value;
            if (Button.Content == null && value != null)
                Button.Content = new Label { Text = value };
            else if (Button.Content is Label label)
                label.Text = value;
        }
    }

    /// <inheritdoc />
    public event System.Action<IButton>? Clicked;

    private string? _textContent;

    protected ButtonWidget(Button button) : base(button)
    {
        // Wire up Myra's click delegate to our C# event
        Button.Click += (sender, e) => Clicked?.Invoke(this);
    }

    /// <summary>
    /// Creates a TextButton wrapped in a ButtonWidget with the given text.
    /// Replaces Myra's Button.CreateTextButton().
    /// </summary>
    public static IButton CreateTextButton(string text)
    {
        var button = new TextButton();
        var wrapper = new ButtonWidget(button);
        wrapper.Text = text;
        return wrapper;
    }
}
