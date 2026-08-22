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
/// Per frame, the button's position is synced so it sits at the owning entity's position relative
/// to the canvas entity: <c>widget.Position = Owner.Position - CanvasEntity.Position</c>.
/// Move the entity and the button follows. Subscribe to <see cref="Clicked"/> to react to user input.
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

        // Keep the button at the entity's position relative to the canvas entity.
        var canvasEntity = _canvasComponent.Owner;
        if (canvasEntity != null)
            _button.Position = Owner.Position - canvasEntity.Position;
    }

    /// <summary>
    /// Bridges the widget's <c>Clicked(IButton)</c> event to the component's <see cref="Clicked"/> event.
    /// </summary>
    private void OnWidgetClicked(IButton button) => Clicked?.Invoke();
}
