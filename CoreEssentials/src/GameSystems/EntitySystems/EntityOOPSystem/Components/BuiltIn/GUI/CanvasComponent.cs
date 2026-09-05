using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GUI;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Component that attaches a single <see cref="Canvas"/> to an entity, making it the owner and
/// source of truth for UI rendering in that part of the entity hierarchy. This mirrors Unity's
/// Canvas: you put one on an entity to declare "this subtree renders UI", and widget components
/// (<c>LabelComponent</c>, <c>ButtonComponent</c>) attach their widgets into it rather than
/// owning a canvas themselves.
/// </summary>
/// <remarks>
/// The component drives the canvas lifecycle end to end:
/// <list type="bullet">
/// <item><see cref="OnAttach"/> — nothing required; the canvas is created eagerly in the constructor.</item>
/// <item><see cref="Update"/> — syncs the canvas position from <see cref="EntityComponent.Owner"/> and pumps the canvas.</item>
/// <item><see cref="OnDetach"/> — calls <see cref="Canvas.CleanUp"/>, releasing all child widgets.</item>
/// </list>
/// Because <c>Entity</c> destroys its children recursively, a single root <see cref="CanvasComponent"/>
/// is enough to cover an entire HUD subtree.
/// </remarks>
public class CanvasComponent : EntityComponent
{
    private readonly Canvas _canvas;

    /// <summary>
    /// Gets the canvas owned by this component. This is the single source of truth for UI in the
    /// entity subtree rooted at the owning entity.
    /// </summary>
    public Canvas Canvas => _canvas;

    /// <summary>
    /// Gets or sets a value indicating whether the canvas renders in screen space (true) or
    /// world space (false). Setting it at runtime flips the space on the next update; XML scene
    /// files can use this to declare world-space canvases via a <c>Property</c> element.
    /// </summary>
    public bool IsScreenSpace
    {
        get => _canvas.IsScreenSpace;
        set => _canvas.IsScreenSpace = value;
    }

    /// <summary>
    /// Gets or sets the canvas width. For world-space canvases this is the size of the anchored
    /// rectangle in world units (see <c>AnchorComponent</c>).
    /// </summary>
    public float Width
    {
        get => _canvas.Width;
        set => _canvas.Width = value;
    }

    /// <summary>
    /// Gets or sets the canvas height. For world-space canvases this is the size of the anchored
    /// rectangle in world units (see <c>AnchorComponent</c>).
    /// </summary>
    public float Height
    {
        get => _canvas.Height;
        set => _canvas.Height = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasComponent"/> class with a screen-space canvas.
    /// Required so prefab/scene instantiation — which creates components via a parameterless constructor —
    /// can build this component. (An optional-parameter-only constructor does not count as parameterless.)
    /// </summary>
    public CanvasComponent() : this(true) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CanvasComponent"/> class.
    /// </summary>
    /// <param name="isScreenSpace">If true (default) the canvas renders in screen space; if false it renders
    /// in world space and follows the owning entity's position relative to the main camera.</param>
    public CanvasComponent(bool isScreenSpace = true)
    {
        _canvas = new Canvas(isScreenSpace);
    }

    /// <summary>
    /// Adds a widget directly to this component's canvas. Convenience for games that want to add
    /// arbitrary widgets (panels, grids, custom controls) without going through a dedicated component.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    public void AddWidget(IWidget widget) => _canvas.AddWidget(widget);

    /// <summary>
    /// Removes a widget from this component's canvas.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    public void RemoveWidget(IWidget widget) => _canvas.RemoveWidget(widget);

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (Owner == null)
            return;

        // Keep the canvas anchored to its owning entity, then pump it so input/animation stays current.
        _canvas.SetPosition(Owner.Position);
        _canvas.Update(gameTime);
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        // Release the canvas and everything inside it. Children are destroyed recursively by Entity,
        // so a single root CanvasComponent is enough to cover the whole subtree.
        _canvas.CleanUp();
    }

    /// <summary>
    /// Finds the nearest <see cref="CanvasComponent"/> for the given entity by walking up its parent
    /// chain (the entity itself first, then each ancestor). Returns null when no canvas is found.
    /// </summary>
    /// <param name="entity">The entity to resolve a canvas for.</param>
    /// <returns>The nearest <see cref="CanvasComponent"/>, or null if none exists in the hierarchy.</returns>
    public static CanvasComponent? FindCanvas(Entity? entity)
    {
        var current = entity;
        while (current != null)
        {
            if (current.TryGetComponent<CanvasComponent>(out var canvasComponent))
                return canvasComponent;

            current = current.Parent;
        }

        return null;
    }

    /// <summary>
    /// Finds the nearest <see cref="CanvasComponent"/> for the given entity, throwing a descriptive
    /// exception when none is found. This is the version widget components use, so a missing canvas
    /// surfaces as a clear error instead of silently doing nothing.
    /// </summary>
    /// <param name="entity">The entity to resolve a canvas for.</param>
    /// <returns>The nearest <see cref="CanvasComponent"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no <see cref="CanvasComponent"/> exists
    /// anywhere in the entity's hierarchy.</exception>
    public static CanvasComponent RequireCanvas(Entity? entity)
    {
        var canvas = FindCanvas(entity);
        if (canvas == null)
            throw new InvalidOperationException(
                $"No CanvasComponent found for the given entity or any of its ancestors. " +
                "Add a CanvasComponent to this entity or one of its parents before attaching GUI widget components.");

        return canvas;
    }
}
