using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Component that renders a text label as part of a parent <see cref="CanvasComponent"/>.
/// The owning entity (or one of its ancestors) must have a <see cref="CanvasComponent"/> —
/// this component never owns a canvas itself. The label widget is created via <see cref="WidgetFactory"/>
/// and added to the nearest canvas when the component attaches.
/// </summary>
/// <remarks>
/// Per frame, the label is positioned inside its canvas by <see cref="HorizontalAlignment"/>
/// and <see cref="VerticalAlignment"/>: e.g. <c>Center</c> puts the label's horizontal middle on
/// the canvas's horizontal middle. The entity's position relative to the canvas entity acts as a
/// margin offset from that aligned reference point, so a host with no position is positioned by
/// alignment alone, and moving the entity moves the label.
/// </remarks>
public class LabelComponent : EntityComponent
{
    private ILabel? _label;
    private CanvasComponent? _canvasComponent;

    private string _text = "";
    private Color _textColor = Color.White;
    private Vector2 _scale = Vector2.One;
    private bool _visible = true;
    private float _opacity = 1.0f;
    private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;
    private VerticalAlignment _verticalAlignment = VerticalAlignment.Top;

    /// <summary>
    /// Gets or sets the display text of the label. Live pass-through: setting it before attaching
    /// is applied on attach; setting it after attaching immediately updates the rendered widget.
    /// </summary>
    public string Text
    {
        get => _text;
        set { _text = value; if (_label != null) _label.Text = value; }
    }

    /// <summary>
    /// Gets or sets the color of the label's text. Live pass-through: setting it after attaching
    /// immediately updates the rendered widget.
    /// </summary>
    public Color TextColor
    {
        get => _textColor;
        set { _textColor = value; if (_label != null) _label.TextColor = value; }
    }

    /// <summary>
    /// Gets or sets the scale of the label widget. Live pass-through: setting it after attaching
    /// immediately updates the rendered widget.
    /// </summary>
    public Vector2 Scale
    {
        get => _scale;
        set { _scale = value; if (_label != null) _label.Scale = value; }
    }

    /// <summary>
    /// Gets or sets whether the label is visible. Live pass-through: setting it after attaching
    /// immediately updates the rendered widget.
    /// </summary>
    public bool Visible
    {
        get => _visible;
        set { _visible = value; if (_label != null) _label.Visible = value; }
    }

    /// <summary>
    /// Gets or sets the opacity of the label (0 = fully transparent, 1 = opaque).
    /// Live pass-through: setting it after attaching immediately updates the rendered widget.
    /// </summary>
    public float Opacity
    {
        get => _opacity;
        set { _opacity = value; if (_label != null) _label.Opacity = value; }
    }

    /// <summary>
    /// Gets the label's current width in layout units. While auto-sized (the default), this is the
    /// measured content size, so it reflects the actual text length instead of 0. Returns 0 when
    /// not attached to a canvas.
    /// </summary>
    public float Width => _label?.Width ?? 0f;

    /// <summary>
    /// Gets the label's current height in layout units. While auto-sized (the default), this is the
    /// measured content size. Returns 0 when not attached to a canvas.
    /// </summary>
    public float Height => _label?.Height ?? 0f;

    /// <summary>
    /// Gets or sets how the label is positioned inside its canvas, horizontally.
    /// <see cref="HorizontalAlignment.Left"/> (default) puts the label's left edge on the canvas's
    /// left edge; <see cref="HorizontalAlignment.Center"/> centers it in the canvas;
    /// <see cref="HorizontalAlignment.Right"/> puts its right edge on the canvas's right edge.
    /// The entity's position relative to the canvas entity offsets (margin) from that reference.
    /// Applied per frame during position sync; scale-aware.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        set => _horizontalAlignment = value;
    }

    /// <summary>
    /// Gets or sets how the label is positioned inside its canvas, vertically.
    /// <see cref="VerticalAlignment.Top"/> (default) puts the label's top edge on the canvas's top
    /// edge; <see cref="VerticalAlignment.Center"/> centers it in the canvas;
    /// <see cref="VerticalAlignment.Bottom"/> puts its bottom edge on the canvas's bottom edge.
    /// The entity's position relative to the canvas entity offsets (margin) from that reference.
    /// Applied per frame during position sync; scale-aware.
    /// </summary>
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        set => _verticalAlignment = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelComponent"/> class (template-friendly).
    /// </summary>
    public LabelComponent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LabelComponent"/> class with initial text.
    /// </summary>
    /// <param name="text">The display text.</param>
    public LabelComponent(string text)
    {
        Text = text;
    }

    /// <inheritdoc />
    public override void OnAttach()
    {
        // Resolve the nearest canvas in the entity hierarchy (this entity or an ancestor).
        _canvasComponent = CanvasComponent.RequireCanvas(Owner);

        // Apply the current property values to the freshly created widget.
        _label = WidgetFactory.CreateLabel(_text);
        _label.TextColor = _textColor;
        // Pin the transform origin to the top-left so scaling matches the alignment math,
        // which assumes top-left scaling. Myra's default origin is the widget center.
        _label.TransformOrigin = Vector2.Zero;
        _label.Scale = _scale;
        _label.Visible = _visible;
        _label.Opacity = _opacity;

        _canvasComponent.Canvas.AddWidget(_label);
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_label != null && _canvasComponent != null)
            _canvasComponent.Canvas.RemoveWidget(_label);

        _label = null;
        _canvasComponent = null;
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (_label == null || _canvasComponent == null || Owner == null)
            return;

        // Position the label inside its canvas by alignment; the entity's position relative to
        // the canvas entity is a margin added on top of the aligned reference point.
        var canvasEntity = _canvasComponent.Owner;
        if (canvasEntity != null)
        {
            var margin = Owner.Position - canvasEntity.Position;
            _label.Position = margin + GetAlignmentOffset(_canvasComponent.Canvas);
        }
    }

    /// <summary>
    /// Computes where the label's top-left corner sits inside the canvas for the configured
    /// alignment: Left/Top is (0, 0); Center shifts by half the canvas minus half the rendered
    /// size; Right/Bottom shifts by the canvas size minus the rendered size. Rendered size is the
    /// measured size multiplied by scale, since Myra scales from the top-left corner.
    /// </summary>
    private Vector2 GetAlignmentOffset(ICanvas canvas)
    {
        if (_label == null)
            return Vector2.Zero;

        var renderedSize = new Vector2(_label.Width * _scale.X, _label.Height * _scale.Y);
        float x = _horizontalAlignment switch
        {
            HorizontalAlignment.Left => 0f,
            HorizontalAlignment.Center => canvas.Width * 0.5f - renderedSize.X * 0.5f,
            HorizontalAlignment.Right => canvas.Width - renderedSize.X,
            _ => 0f
        };
        float y = _verticalAlignment switch
        {
            VerticalAlignment.Top => 0f,
            VerticalAlignment.Center => canvas.Height * 0.5f - renderedSize.Y * 0.5f,
            VerticalAlignment.Bottom => canvas.Height - renderedSize.Y,
            _ => 0f
        };
        return new Vector2(x, y);
    }
}
