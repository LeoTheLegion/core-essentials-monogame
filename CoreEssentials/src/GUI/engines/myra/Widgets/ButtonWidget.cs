using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
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
        // When the content is a MyraLabel (e.g. buttons created via CreateTextButton),
        // return its text rather than the label's type name.
        get => _textContent ?? (Button.Content is MyraLabel contentLabel ? contentLabel.Text : Button.Content?.ToString());
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
    public object? Font
    {
        get => _font;
        set
        {
            _font = value;
            // The button's text lives in its content label; apply the font there.
            if (Button.Content is MyraLabel contentLabel)
                contentLabel.Font = (FontStashSharp.SpriteFontBase?)value;
        }
    }

    /// <inheritdoc />
    public Color? BackgroundTint
    {
        get => _backgroundTint;
        set
        {
            _backgroundTint = value;
            ApplyBackground(value);
        }
    }

    /// <inheritdoc />
    public event System.Action<IButton>? Clicked;

    private string? _textContent;
    private object? _font;
    private Color? _backgroundTint;

    /// <summary>
    /// Applies the background tint to all of Myra's button visual states. A null tint clears every
    /// state brush so no opaque box is drawn (transparent by default); a non-null tint paints a
    /// solid background of that color in each state.
    /// </summary>
    private void ApplyBackground(Color? tint)
    {
        if (tint == null)
        {
            Button.Background = null;
            Button.OverBackground = null;
            Button.PressedBackground = null;
            Button.DisabledBackground = null;
            Button.FocusedBackground = null;
            return;
        }

        var brush = new Brushes.SolidColorBrush(tint.Value);
        var myraBrush = brush.MyraBrush;
        Button.Background = myraBrush;
        Button.OverBackground = myraBrush;
        Button.PressedBackground = myraBrush;
        Button.DisabledBackground = myraBrush;
        Button.FocusedBackground = myraBrush;
    }

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
