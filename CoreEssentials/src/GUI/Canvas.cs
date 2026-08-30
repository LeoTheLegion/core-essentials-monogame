using Microsoft.Xna.Framework;
using CoreEssentials.GUI.Engines.Myra;
using CoreEssentials.GUI.Types;
using CoreEssentials.Camera;

namespace CoreEssentials.GUI;

/// <summary>
/// Represents a UI canvas that can contain and manage widgets in either screen space or world space.
/// This class wraps <see cref="CanvasImpl"/> to provide backward-compatible API while delegating all operations to the new implementation.
/// </summary>
public class Canvas : ICanvas
{
    private readonly CanvasImpl _impl;

    /// <summary>
    /// Gets or sets whether the canvas renders in screen space (true) or world space (false).
    /// The value is re-evaluated on every update, so it can be switched at runtime.
    /// </summary>
    public bool IsScreenSpace
    {
        get => _impl.IsScreenSpace;
        set => _impl.IsScreenSpace = value;
    }

    /// <inheritdoc />
    public IBrush? Background
    {
        get => _impl.Background;
        set => _impl.Background = value;
    }

    /// <inheritdoc />
    public Thickness BorderThickness
    {
        get => _impl.BorderThickness;
        set => _impl.BorderThickness = value;
    }

    /// <inheritdoc />
    public IList<IWidget> Children => _impl.Children;

    /// <inheritdoc />
    public IEnumerable<IWidget> Widgets => _impl.Widgets;

    /// <summary>
    /// Initializes a new instance of the Canvas class with specified space type.
    /// </summary>
    /// <param name="isScreenSpace">If true, canvas will be in screen space; if false, in world space.</param>
    public Canvas(bool isScreenSpace) => _impl = new CanvasImpl(isScreenSpace);

    /// <summary>
    /// Initializes a new instance of the Canvas class in screen space by default.
    /// </summary>
    public Canvas() : this(true) { }

    /// <inheritdoc />
    public void SetPosition(Vector2 position) => _impl.SetPosition(position);

    /// <inheritdoc />
    public void AddWidget(IWidget widget) => _impl.AddWidget(widget);

    /// <inheritdoc />
    public void RemoveWidget(IWidget widget) => _impl.RemoveWidget(widget);

    /// <inheritdoc />
    public void CleanUp() => _impl.CleanUp();

    /// <inheritdoc />
    public void Update(GameTime gameTime) => _impl.Update(gameTime);

    /// <inheritdoc />
    public void AddChild(IWidget widget) => _impl.AddChild(widget);

    /// <inheritdoc />
    public void RemoveChild(IWidget widget) => _impl.RemoveChild(widget);

    /// <inheritdoc />
    public void ClearChildren() => _impl.ClearChildren();

    /// <inheritdoc />
    public bool AutoWidth { get => _impl.AutoWidth; set => _impl.AutoWidth = value; }

    /// <inheritdoc />
    public bool AutoHeight { get => _impl.AutoHeight; set => _impl.AutoHeight = value; }

    /// <summary>
    /// Gets or sets the canvas width. Setting it pins auto-sizing first so an explicit
    /// size is always applied (world-space canvases rely on this).
    /// </summary>
    public float Width
    {
        get => _impl.Width;
        set { _impl.AutoWidth = false; _impl.Width = value; }
    }

    /// <summary>
    /// Gets or sets the canvas height. Setting it pins auto-sizing first so an explicit
    /// size is always applied (world-space canvases rely on this).
    /// </summary>
    public float Height
    {
        get => _impl.Height;
        set { _impl.AutoHeight = false; _impl.Height = value; }
    }

    /// <inheritdoc />
    public bool Visible { get => _impl.Visible; set => _impl.Visible = value; }

    /// <inheritdoc />
    public bool Enabled { get => _impl.Enabled; set => _impl.Enabled = value; }

    /// <inheritdoc />
    public bool IsMouseInside => _impl.IsMouseInside;

    /// <inheritdoc />
    public bool IsKeyboardFocused => _impl.IsKeyboardFocused;

    /// <inheritdoc />
    public Vector2 Position { get => _impl.Position; set => _impl.Position = value; }

    /// <inheritdoc />
    public Thickness Margin { get => _impl.Margin; set => _impl.Margin = value; }

    /// <inheritdoc />
    public HorizontalAlignment HorizontalAlignment { get => _impl.HorizontalAlignment; set => _impl.HorizontalAlignment = value; }

    /// <inheritdoc />
    public VerticalAlignment VerticalAlignment { get => _impl.VerticalAlignment; set => _impl.VerticalAlignment = value; }

    /// <inheritdoc />
    public Vector2 Scale { get => _impl.Scale; set => _impl.Scale = value; }

    /// <inheritdoc />
    public float Opacity { get => _impl.Opacity; set => _impl.Opacity = value; }
}
