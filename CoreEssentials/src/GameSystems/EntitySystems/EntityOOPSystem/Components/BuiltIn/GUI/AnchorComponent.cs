using CoreEssentials.GUI;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Common UI anchor presets, mirroring Unity's RectTransform anchor points.
/// Each preset maps to a normalized (0..1) point in screen space: (0, 0) is the top-left
/// corner and (1, 1) is the bottom-right corner.
/// </summary>
public enum AnchorPreset
{
    /// <summary>Top-left corner (0, 0).</summary>
    TopLeft,
    /// <summary>Top edge center (0.5, 0).</summary>
    TopCenter,
    /// <summary>Top-right corner (1, 0).</summary>
    TopRight,
    /// <summary>Left edge center (0, 0.5).</summary>
    MiddleLeft,
    /// <summary>Screen center (0.5, 0.5).</summary>
    MiddleCenter,
    /// <summary>Right edge center (1, 0.5).</summary>
    MiddleRight,
    /// <summary>Bottom-left corner (0, 1).</summary>
    BottomLeft,
    /// <summary>Bottom edge center (0.5, 1).</summary>
    BottomCenter,
    /// <summary>Bottom-right corner (1, 1).</summary>
    BottomRight
}

/// <summary>
/// Anchors the owning entity to a point on its canvas, mirroring Unity's RectTransform
/// anchor + offset model. Each frame the entity's position is resolved as
/// <c>(Anchor * canvasRect) + Offset</c>, so layouts stay correct when the window resizes.
/// </summary>
/// <remarks>
/// This component only drives the position of entities that belong to a canvas hierarchy
/// (i.e. they have a <see cref="CanvasComponent"/> on themselves or an ancestor). Entities
/// without a canvas are left untouched, so plain world-space gameplay entities can carry
/// this component harmlessly.
///
/// The anchor rectangle depends on the space of the nearest canvas:
/// <list type="bullet">
/// <item>Screen-space canvas — the GUI viewport (<c>GUIManager.Width/Height</c>), so HUD
/// elements survive window resizes.</item>
/// <item>World-space canvas — the canvas's own <c>Width/Height</c> (falling back to the GUI
/// viewport when unset), positioned relative to the canvas entity in world coordinates, so
/// anchored widgets can live inside a panel that moves around the world.</item>
/// </list>
///
/// Typical usage: put one <see cref="CanvasComponent"/> on a root entity and give each HUD
/// element (label/button child) an <see cref="AnchorComponent"/> with a preset such as
/// <see cref="AnchorPreset.TopCenter"/> plus a small pixel offset. All properties are
/// XML-serializable via <c>SerializationUtils</c>, so entire anchored layouts can be defined
/// in scene files.
/// </remarks>
public class AnchorComponent : EntityComponent
{
    private Vector2 _anchor = new(0.5f, 0.5f);
    private Vector2 _offset = Vector2.Zero;
    private bool _active = true;
    private AnchorPreset _preset = AnchorPreset.MiddleCenter;

    /// <summary>
    /// Gets or sets a common anchor preset. Setting it updates <see cref="Anchor"/> with the
    /// preset's normalized point. Assigning <see cref="Anchor"/> directly does not change this
    /// value — the last property written in code (or XML document order) wins.
    /// </summary>
    public AnchorPreset Preset
    {
        get => _preset;
        set
        {
            _preset = value;
            _anchor = ToVector2(value);
        }
    }

    /// <summary>
    /// Gets or sets the normalized anchor point on the canvas rect: (0, 0) = top-left,
    /// (1, 1) = bottom-right. The rect is the GUI viewport for screen-space canvases and the
    /// canvas's own size for world-space canvases. Live pass-through: changing it re-resolves
    /// the position next update.
    /// </summary>
    public Vector2 Anchor
    {
        get => _anchor;
        set => _anchor = value;
    }

    /// <summary>
    /// Gets or sets the pixel offset applied on top of the anchor point (screen pixels for
    /// screen-space canvases, world units for world-space canvases).
    /// </summary>
    public Vector2 Offset
    {
        get => _offset;
        set => _offset = value;
    }

    /// <summary>
    /// Gets or sets whether this component actively drives the entity's position each frame.
    /// Set to false to freeze the current position while keeping the anchor configuration.
    /// </summary>
    public bool Active
    {
        get => _active;
        set => _active = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnchorComponent"/> class (template-friendly).
    /// Defaults to screen center with no offset.
    /// </summary>
    public AnchorComponent()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AnchorComponent"/> class with a preset anchor
    /// and pixel offset.
    /// </summary>
    /// <param name="preset">The anchor preset to start with.</param>
    /// <param name="offset">The pixel offset from the anchor point.</param>
    public AnchorComponent(AnchorPreset preset, Vector2 offset)
    {
        Preset = preset;
        _anchor = ToVector2(preset);
        _offset = offset;
    }

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (!_active || Owner == null)
            return;

        // Only anchor entities that belong to a canvas hierarchy (screen or world space).
        var canvasComponent = CanvasComponent.FindCanvas(Owner);
        if (canvasComponent?.Owner == null)
            return;

        Vector2 resolved;
        if (canvasComponent.IsScreenSpace)
        {
            // Screen space: resolve against the GUI viewport. The canvas entity itself is
            // expected to sit at the origin, so no extra offset is needed.
            var rect = new Rectangle(0, 0, GUIManager.Width, GUIManager.Height);
            resolved = ResolvePosition(_anchor, _offset, rect);
        }
        else
        {
            // World space: resolve against the canvas's own size in world units. For a child of
            // the canvas entity the canvas-relative position is exactly its LocalPosition, so
            // the widget stays pinned inside the panel while it moves around the world — the
            // same convention widget components use (Owner.Position - canvasEntity.Position).
            var width = canvasComponent.Canvas.Width > 0 ? canvasComponent.Canvas.Width : GUIManager.Width;
            var height = canvasComponent.Canvas.Height > 0 ? canvasComponent.Canvas.Height : GUIManager.Height;
            var rect = new Rectangle(0, 0, (int)width, (int)height);
            resolved = ResolvePosition(_anchor, _offset, rect);
        }

        // Parented entities expose their position through LocalPosition (see Entity.Position),
        // so drive that one; root entities use the plain position field.
        if (Owner.Parent != null)
            Owner.LocalPosition = resolved;
        else
            Owner.Position = resolved;
    }

    /// <summary>
    /// Resolves an anchor point plus offset into a concrete position within the given rectangle.
    /// </summary>
    /// <param name="anchor">Normalized anchor point (0..1 on each axis).</param>
    /// <param name="offset">Pixel offset from the anchor point.</param>
    /// <param name="rect">The screen/canvas rectangle to resolve against.</param>
    /// <returns>The resolved position in pixels.</returns>
    public static Vector2 ResolvePosition(Vector2 anchor, Vector2 offset, Rectangle rect)
        => new Vector2(anchor.X * rect.Width + offset.X, anchor.Y * rect.Height + offset.Y);

    /// <summary>
    /// Converts an anchor preset to its normalized (0..1) screen-space point.
    /// </summary>
    /// <param name="preset">The preset to convert.</param>
    /// <returns>The normalized anchor point.</returns>
    public static Vector2 ToVector2(AnchorPreset preset) => preset switch
    {
        AnchorPreset.TopLeft => new Vector2(0f, 0f),
        AnchorPreset.TopCenter => new Vector2(0.5f, 0f),
        AnchorPreset.TopRight => new Vector2(1f, 0f),
        AnchorPreset.MiddleLeft => new Vector2(0f, 0.5f),
        AnchorPreset.MiddleCenter => new Vector2(0.5f, 0.5f),
        AnchorPreset.MiddleRight => new Vector2(1f, 0.5f),
        AnchorPreset.BottomLeft => new Vector2(0f, 1f),
        AnchorPreset.BottomCenter => new Vector2(0.5f, 1f),
        AnchorPreset.BottomRight => new Vector2(1f, 1f),
        _ => new Vector2(0.5f, 0.5f)
    };
}
