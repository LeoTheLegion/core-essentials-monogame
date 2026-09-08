using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Renders a line of text at the owning entity's position using the shared base font. This ports the
/// world-space, canvas-free text rendering that used to live in the (now-deleted) <c>TextEntity</c>, so
/// a plain game object can show aligned text purely from data:
/// <code>
/// &lt;Component Type="TextComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="Text" Value="Hello" /&gt;
///     &lt;Property Name="Color" Value="White" /&gt;
///     &lt;Property Name="Alignment" Value="Center" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// The base <c>Entity.Render</c> auto-draws every attached <see cref="IDrawableComponent"/>, so the host
/// entity needs no <c>Render</c> override.
/// </summary>
public class TextComponent : EntityComponent, IDrawableComponent
{
    /// <summary>Horizontal alignment of the text relative to the entity's position.</summary>
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    private FontAsset? _font;

    /// <summary>The text to render.</summary>
    public string Text { get; set; } = "";

    /// <summary>The color to render the text with.</summary>
    public Color Color { get; set; } = Color.White;

    /// <summary>The horizontal alignment of the text.</summary>
    public TextAlignment Alignment { get; set; } = TextAlignment.Left;

    /// <summary>An additional offset added to the entity's position when drawing. Defaults to zero.</summary>
    public Vector2 Offset { get; set; } = Vector2.Zero;

    /// <inheritdoc />
    public override void OnAttach()
    {
        base.OnAttach();
        _font = AssetManager.LoadAsset<FontAsset>("Fonts/base");
    }

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch)
    {
        if (Owner == null || _font?.Font == null)
            return;

        Vector2 textSize = _font.MeasureStringVector(Text);
        spriteBatch.DrawString(_font.Font, Text, ComputeDrawPosition(textSize), Color);
    }

    /// <summary>
    /// Computes the top-left draw position for the text given its measured size and the current
    /// alignment/offset. Exposed so unit tests can assert the alignment math without a live
    /// <see cref="SpriteBatch"/>.
    /// </summary>
    public Vector2 ComputeDrawPosition(Vector2 textSize)
    {
        Vector2 drawPosition = (Owner?.Position ?? Vector2.Zero) + Offset;

        // Apply alignment.
        switch (Alignment)
        {
            case TextAlignment.Center:
                drawPosition.X -= textSize.X / 2;
                break;
            case TextAlignment.Right:
                drawPosition.X -= textSize.X;
                break;
        }

        return drawPosition;
    }
}
