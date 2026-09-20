using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyraGraphics2D = Myra.Graphics2D;

namespace CoreEssentials.GUI.Engines.Myra.Brushes;

/// <summary>
/// A Myra brush that fills a destination rectangle with a texture, stretched to fit, tinted by a
/// color. Used as a widget background (e.g. a button's sprite back-plate). Unlike the built-in
/// <see cref="SolidColorBrush"/>, which wraps a Myra <c>SolidBrush</c>, this type implements Myra's
/// <c>IBrush</c> directly because Myra ships no textured brush of its own.
/// </summary>
public class TextureBrush : MyraGraphics2D.IBrush
{
    /// <summary>
    /// Gets the texture drawn by this brush.
    /// </summary>
    public Texture2D Texture { get; }

    /// <summary>
    /// Gets or sets the tint applied to the texture (multiplied against the per-pixel color).
    /// Defaults to white (no tint).
    /// </summary>
    public Color Tint { get; set; }

    /// <summary>
    /// Gets the optional source region within <see cref="Texture"/> to draw. When null, the entire
    /// texture is used.
    /// </summary>
    public Rectangle? Source { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TextureBrush"/> class.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    /// <param name="tint">The tint applied to the texture (default white).</param>
    /// <param name="source">An optional source region within the texture (default: whole texture).</param>
    public TextureBrush(Texture2D texture, Color? tint = null, Rectangle? source = null)
    {
        Texture = texture ?? throw new ArgumentNullException(nameof(texture));
        Tint = tint ?? Color.White;
        Source = source;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Stretches the (optionally region-clipped) texture into <paramref name="dest"/> and multiplies
    /// the stored tint against <paramref name="color"/> — matching how Myra's own brushes blend their
    /// color with the caller-supplied one.
    /// </remarks>
    public void Draw(MyraGraphics2D.RenderContext context, Rectangle dest, Color color)
    {
        context.Draw(Texture, dest, Source, MultiplyTint(Tint, color));
    }

    /// <summary>
    /// Multiplies two colors component-wise in 0–255 space (premultiplied-style tinting), matching
    /// Myra's <c>SolidBrush</c> blend.
    /// </summary>
    private static Color MultiplyTint(Color a, Color b) => new(
        (byte)(a.R * b.R / 255),
        (byte)(a.G * b.G / 255),
        (byte)(a.B * b.B / 255),
        (byte)(a.A * b.A / 255));
}
