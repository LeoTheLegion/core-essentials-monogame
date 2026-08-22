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

    /// <summary>
    /// Gets or sets the display text of the label. Can be set before attaching; applied on attach.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Gets or sets the color of the label's text. Can be set before attaching; applied on attach.
    /// </summary>
    public Color TextColor { get; set; } = Color.White;

    /// <summary>
    /// Gets or sets the scale of the label widget. Can be set before attaching; applied on attach.
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets whether the label is visible. Can be set before attaching; applied on attach.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets the opacity of the label (0 = fully transparent, 1 = opaque).
    /// Can be set before attaching; applied on attach.
    /// </summary>
    public float Opacity { get; set; } = 1.0f;

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

        _label = WidgetFactory.CreateLabel(Text);
        _label.TextColor = TextColor;
        _label.Scale = Scale;
        _label.Visible = Visible;
        _label.Opacity = Opacity;

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
