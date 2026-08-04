using CoreEssentials.GUI.Types;
using MyraButton = Myra.Graphics2D.UI.Button;
using MyraLabel = Myra.Graphics2D.UI.Label;

namespace CoreEssentials.GUI.Engines.Myra.Widgets;

/// <summary>
/// Wrapper for a Myra Button, implementing IButton with event support and static factory.
/// </summary>
public class ButtonWidget : WidgetBase, IButton
{
    /// <summary>
    /// Gets the underlying Myra Button instance (typed).
    /// </summary>
    /// <summary>
    /// Gets the underlying Myra Button instance (typed).
    /// </summary>
    protected MyraButton Button => (MyraButton)base.MyraWidget;

    /// <inheritdoc />
    public string? Text
    {
        get => _textContent ?? Button.Content?.ToString();
        set
        {
            _textContent = value;
            if (Button.Content == null && value != null)
                Button.Content = new MyraLabel { Text = value };
            else if (Button.Content is MyraLabel label)
                label.Text = value;
        }
    }

    /// <inheritdoc />
    public event System.Action<IButton>? Clicked;

    private string? _textContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonWidget"/> class.
    /// </summary>
    /// <param name="button">The underlying Myra Button widget.</param>
    protected ButtonWidget(MyraButton button) : base(button)
    {
        // Wire up Myra's click delegate to our C# event
        Button.Click += (sender, e) => Clicked?.Invoke(this);
    }

    /// <summary>
    /// Creates a Button with a text label as its content, wrapped in a ButtonWidget.
    /// Replaces Myra's Button.CreateTextButton().
    /// </summary>
    public static IButton CreateTextButton(string text)
    {
        var button = new MyraButton();
        button.Content = new MyraLabel { Text = text };
        return new ButtonWidget(button);
    }
}
