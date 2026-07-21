using Microsoft.Xna.Framework;
using MyraLabel = Myra.Graphics2D.UI.Label;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.GUI.Engines.Myra.Widgets;

/// <summary>
/// Wrapper for a Myra Label, implementing ILabel interface.
/// </summary>
public class LabelWidget : WidgetBase, ILabel
{
    /// <summary>
    /// Gets the underlying Myra Label instance (typed).
    /// </summary>
    protected new MyraLabel Label => (MyraLabel)base.MyraWidget;

    /// <inheritdoc />
    public string? Text
    {
        get => Label.Text;
        set => Label.Text = value;
    }

    /// <inheritdoc />
    public object? Font
    {
        get => Label.Font;
        set => Label.Font = (FontStashSharp.SpriteFontBase?)value;
    }

    /// <inheritdoc />
    public Color TextColor
    {
        get => Label.TextColor;
        set => Label.TextColor = value;
    }

    /// <summary>
    /// Creates a new LabelWidget with the specified text. Convenient factory-style constructor.
    /// </summary>
    public LabelWidget(string text) : base(new MyraLabel { Text = text })
    {
    }
}
