using System;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Component that renders a clickable text button as part of a parent <see cref="CanvasComponent"/>.
/// The owning entity (or one of its ancestors) must have a <see cref="CanvasComponent"/> —
/// this component never owns a canvas itself. The button widget is created via <see cref="WidgetFactory"/>
/// and added to the nearest canvas when the component attaches.
/// </summary>
/// <remarks>
/// Per frame, the button is positioned inside its canvas by <see cref="HorizontalAlignment"/>
/// and <see cref="VerticalAlignment"/>: e.g. <c>Center</c> puts the button's horizontal middle on
/// the canvas's horizontal middle. The entity's position relative to the canvas entity acts as a
/// margin offset from that aligned reference point, so a host with no position is positioned by
/// alignment alone, and moving the entity moves the button. Subscribe to <see cref="Clicked"/>
/// to react to user input.
/// </remarks>
public class ButtonComponent : EntityComponent
{
    private IButton? _button;
    private CanvasComponent? _canvasComponent;

    /// <summary>
    /// Occurs when the button is clicked by the user.
    /// </summary>
    public event Action? Clicked;

    /// <summary>
    /// Gets or sets the display text on the button. Can be set before attaching; applied on attach.
    /// </summary>
    public string Text { get; set; } = "";

    /// <summary>
    /// Gets or sets the scale of the button widget. Can be set before attaching; applied on attach.
    /// </summary>
    public Vector2 Scale { get; set; } = Vector2.One;

    /// <summary>
    /// Gets or sets whether the button is visible. Can be set before attaching; applied on attach.
    /// </summary>
    public bool Visible { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the button is enabled to receive input.
    /// Can be set before attaching; applied on attach.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets how the button is positioned inside its canvas, horizontally.
    /// <see cref="HorizontalAlignment.Left"/> (default) puts the button's left edge on the canvas's
    /// left edge; <see cref="HorizontalAlignment.Center"/> centers it in the canvas;
    /// <see cref="HorizontalAlignment.Right"/> puts its right edge on the canvas's right edge.
    /// The entity's position relative to the canvas entity offsets (margin) from that reference.
    /// Applied per frame during position sync; scale-aware.
    /// </summary>
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;

    /// <summary>
    /// Gets or sets how the button is positioned inside its canvas, vertically.
    /// <see cref="VerticalAlignment.Top"/> (default) puts the button's top edge on the canvas's top
    /// edge; <see cref="VerticalAlignment.Center"/> centers it in the canvas;
    /// <see cref="VerticalAlignment.Bottom"/> puts its bottom edge on the canvas's bottom edge.
    /// The entity's position relative to the canvas entity offsets (margin) from that reference.
    /// Applied per frame during position sync; scale-aware.
    /// </summary>
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonComponent"/> class (template-friendly).
    /// </summary>
    public ButtonComponent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ButtonComponent"/> class with initial text.
    /// </summary>
    /// <param name="text">The display text on the button.</param>
    public ButtonComponent(string text)
    {
        Text = text;
    }

    /// <inheritdoc />
    public override void OnAttach()
    {
        // Resolve the nearest canvas in the entity hierarchy (this entity or an ancestor).
        _canvasComponent = CanvasComponent.RequireCanvas(Owner);

        _button = WidgetFactory.CreateTextButton(Text);
        _button.Scale = Scale;
        _button.Visible = Visible;
        _button.Enabled = Enabled;
        _button.Clicked += OnWidgetClicked;

        _canvasComponent.Canvas.AddWidget(_button);
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_button != null)
            _button.Clicked -= OnWidgetClicked;

        if (_button != null && _canvasComponent != null)
            _canvasComponent.Canvas.RemoveWidget(_button);

        _button = null;
        _canvasComponent = null;
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (_button == null || _canvasComponent == null || Owner == null)
            return;

        // Position the button inside its canvas by alignment; the entity's position relative to
        // the canvas entity is a margin added on top of the aligned reference point.
        var canvasEntity = _canvasComponent.Owner;
        if (canvasEntity != null)
        {
            var margin = Owner.Position - canvasEntity.Position;
            _button.Position = margin + GetAlignmentOffset(_canvasComponent.Canvas);
        }
    }

    /// <summary>
    /// Computes where the button's top-left corner sits inside the canvas for the configured
    /// alignment: Left/Top is (0, 0); Center shifts by half the canvas minus half the rendered
    /// size; Right/Bottom shifts by the canvas size minus the rendered size. Rendered size is the
    /// measured size multiplied by scale, since Myra scales from the top-left corner.
    /// </summary>
    private Vector2 GetAlignmentOffset(ICanvas canvas)
    {
        if (_button == null)
            return Vector2.Zero;

        var renderedSize = new Vector2(_button.Width * Scale.X, _button.Height * Scale.Y);
        float x = HorizontalAlignment switch
        {
            HorizontalAlignment.Left => 0f,
            HorizontalAlignment.Center => canvas.Width * 0.5f - renderedSize.X * 0.5f,
            HorizontalAlignment.Right => canvas.Width - renderedSize.X,
            _ => 0f
        };
        float y = VerticalAlignment switch
        {
            VerticalAlignment.Top => 0f,
            VerticalAlignment.Center => canvas.Height * 0.5f - renderedSize.Y * 0.5f,
            VerticalAlignment.Bottom => canvas.Height - renderedSize.Y,
            _ => 0f
        };
        return new Vector2(x, y);
    }

    /// <summary>
    /// Bridges the widget's <c>Clicked(IButton)</c> event to the component's <see cref="Clicked"/> event.
    /// </summary>
    private void OnWidgetClicked(IButton button) => Clicked?.Invoke();
}
