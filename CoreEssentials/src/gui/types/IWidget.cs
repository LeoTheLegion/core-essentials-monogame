using Microsoft.Xna.Framework;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Base abstraction for all UI widget elements.
/// </summary>
public interface IWidget
{
    /// <summary>
    /// Gets or sets the width of this widget in pixels.
    /// </summary>
    float Width { get; set; }

    /// <summary>
    /// Gets or sets the height of this widget in pixels.
    /// </summary>
    float Height { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this widget is visible.
    /// </summary>
    bool Visible { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this widget is enabled to receive input.
    /// </summary>
    bool Enabled { get; set; }

    /// <summary>
    /// Gets a value indicating whether the mouse cursor is currently inside this widget's bounds.
    /// </summary>
    bool IsMouseInside { get; }

    /// <summary>
    /// Gets a value indicating whether this widget has keyboard focus.
    /// </summary>
    bool IsKeyboardFocused { get; }

    /// <summary>
    /// Gets or sets the 2D position of this widget relative to its parent container.
    /// Shortcut for setting X and Y simultaneously using a MonoGame Vector2.
    /// </summary>
    Vector2 Position { get; set; }

    /// <summary>
    /// Gets or sets the margin around this widget as a thickness value.
    /// </summary>
    Thickness Margin { get; set; }

    /// <summary>
    /// Gets or sets the horizontal alignment of this widget within its layout container.
    /// </summary>
    HorizontalAlignment HorizontalAlignment { get; set; }

    /// <summary>
    /// Gets or sets the vertical alignment of this widget within its layout container.
    /// </summary>
    VerticalAlignment VerticalAlignment { get; set; }

    /// <summary>
    /// Gets or sets the scale factor for this widget in X and Y dimensions.
    /// A value of (1, 1) represents no scaling; values greater than 1 enlarge, less than 1 shrink.
    /// </summary>
    Vector2 Scale { get; set; }

    /// <summary>
    /// Gets or sets the opacity of this widget. 
    /// 0.0 is fully transparent, 1.0 is fully opaque.
    /// </summary>
    float Opacity { get; set; }
}

/// <summary>
/// Defines thickness values for uniform spacing around a widget (e.g., margins, borders).
/// </summary>
public struct Thickness
{
    /// <summary>The left margin/border thickness.</summary>
    public float Left { get; set; }

    /// <summary>The top margin/border thickness.</summary>
    public float Top { get; set; }

    /// <summary>The right margin/border thickness.</summary>
    public float Right { get; set; }

    /// <summary>The bottom margin/border thickness.</summary>
    public float Bottom { get; set; }

    /// <summary>
    /// Creates a uniform thickness with the same value for all sides.
    /// </summary>
    public Thickness(float uniformValue) : this(uniformValue, uniformValue, uniformValue, uniformValue) { }

    /// <summary>
    /// Creates a thickness with specific values for each side.
    /// </summary>
    public Thickness(float left, float top, float right, float bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    /// <summary>
    /// Creates a uniform thickness with the same value for all sides.
    /// </summary>
    public static Thickness Zero => new(0f);

    /// <summary>
    /// Creates a uniform thickness with the specified value.
    /// </summary>
    public static Thickness Uniform(float value) => new(value);
}

/// <summary>
/// Defines horizontal alignment options for widgets within their layout containers.
/// </summary>
public enum HorizontalAlignment
{
    /// <summary>
    /// Align the widget to the left edge of its container.
    /// </summary>
    Left,

    /// <summary>
    /// Center the widget horizontally within its container.
    /// </summary>
    Center,

    /// <summary>
    /// Align the widget to the right edge of its container.
    /// </summary>
    Right,

    /// <summary>
    /// Stretch the widget to fill the available horizontal space in its container.
    /// </summary>
    Stretch
}

/// <summary>
/// Defines vertical alignment options for widgets within their layout containers.
/// </summary>
public enum VerticalAlignment
{
    /// <summary>
    /// Align the widget to the top edge of its container.
    /// </summary>
    Top,

    /// <summary>
    /// Center the widget vertically within its container.
    /// </summary>
    Center,

    /// <summary>
    /// Align the widget to the bottom edge of its container.
    /// </summary>
    Bottom,

    /// <summary>
    /// Stretch the widget to fill the available vertical space in its container.
    /// </summary>
    Stretch
}
