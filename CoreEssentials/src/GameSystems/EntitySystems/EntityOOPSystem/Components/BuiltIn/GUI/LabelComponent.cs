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
/// Per frame, the label's position is synced so it sits at the owning entity's position relative
/// to the canvas entity: <c>widget.Position = Owner.Position - CanvasEntity.Position</c>.
/// Move the entity and the label follows.
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

        // Keep the label at the entity's position relative to the canvas entity.
        var canvasEntity = _canvasComponent.Owner;
        if (canvasEntity != null)
            _label.Position = Owner.Position - canvasEntity.Position;
    }
}
