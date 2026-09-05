using System;
using System.Collections.Generic;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI.Types;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Draws a debug overlay that makes the invisible parts of a label-alignment demo visible: each
/// canvas's bounds, each label's rendered bounds (in the label's own text color), and a white cross
/// at the reference point its alignment targets. This ports the custom <c>Draw</c> override that used
/// to live in a scene subclass (the LabelAlignment demo), so the overlay can be declared purely from
/// data by attaching this component to any entity:
/// <code>
/// &lt;EntityDefinition Type="...GameObjectEntity" Id="debugOverlay"&gt;
///   &lt;Components&gt;&lt;Component Type="LabelAlignmentDebugOverlayComponent" /&gt;&lt;/Components&gt;
/// &lt;/EntityDefinition&gt;
/// </code>
/// The overlay auto-discovers every <see cref="CanvasComponent"/> and <see cref="LabelComponent"/> in
/// the scene. Screen-space canvases are drawn with an identity transform (DarkSlateGray bounds);
/// world-space canvases are projected through the main camera (DarkOrange bounds). Because it
/// implements <see cref="IDrawableComponent"/>, the owning entity's render pass draws it each frame —
/// no scene <c>Draw</c> override is required.
/// </summary>
public class LabelAlignmentDebugOverlayComponent : EntityComponent, IDrawableComponent
{
    private readonly Primitives _primitives = new();

    // A separate SpriteBatch for the overlay: entity rendering runs inside an already-begun batch
    // (with the camera view matrix), and this overlay draws screen-space geometry, so it needs its
    // own Begin/End scope — exactly like Aether's DebugView uses its own internal batch.
    private SpriteBatch? _overlayBatch;

    /// <inheritdoc />
    public void Draw(SpriteBatch spriteBatch)
    {
        var system = EntitySystem;
        if (system == null) return;

        var device = spriteBatch.GraphicsDevice;
        if (device == null) return; // No live graphics device (e.g. unit tests).

        EnsureOverlayBatch(device);
        _overlayBatch!.Begin();
        DrawOverlay(system, _overlayBatch);
        _overlayBatch.End();
    }

    private void EnsureOverlayBatch(GraphicsDevice device)
    {
        if (_overlayBatch == null || _overlayBatch.GraphicsDevice != device)
            _overlayBatch = new SpriteBatch(device);
    }

    // ── Testability seams ────────────────────────────────────────────────────────

    /// <summary>
    /// Draws the full overlay for a system onto the given batch. Virtual so unit tests can drive the
    /// geometry without a live graphics device (the discovery + bounds math are exercised separately).
    /// </summary>
    protected virtual void DrawOverlay(EntitySystem system, SpriteBatch batch)
    {
        var canvases = DiscoverCanvases(system);
        var labels = DiscoverLabels(system);

        foreach (var canvas in canvases)
            DrawCanvasBounds(batch, canvas);

        foreach (var label in labels)
        {
            var ownerCanvas = CanvasComponent.FindCanvas(label.Owner);
            if (ownerCanvas == null || ownerCanvas.Canvas == null) continue;
            DrawLabelBounds(batch, label, ownerCanvas);
        }
    }

    /// <summary>
    /// Collects every <see cref="CanvasComponent"/> in the system. Exposed so unit tests can assert
    /// discovery without a graphics device. The system's entity list is already flat (nested children
    /// are registered as first-class entities, not just linked via <c>Children</c>), so we filter it
    /// directly rather than recursing — recursing would double-count every nested component.
    /// </summary>
    public List<CanvasComponent> DiscoverCanvases(EntitySystem system)
    {
        var result = new List<CanvasComponent>();
        foreach (var entity in system.GetEntities())
        {
            var canvas = entity.GetComponent<CanvasComponent>();
            if (canvas != null) result.Add(canvas);
        }
        return result;
    }

    /// <summary>
    /// Collects every <see cref="LabelComponent"/> in the system. Exposed so unit tests can assert
    /// discovery without a graphics device. See <see cref="DiscoverCanvases"/> on why this filters the
    /// flat entity list instead of recursing into <c>Children</c>.
    /// </summary>
    public List<LabelComponent> DiscoverLabels(EntitySystem system)
    {
        var result = new List<LabelComponent>();
        foreach (var entity in system.GetEntities())
        {
            var label = entity.GetComponent<LabelComponent>();
            if (label != null) result.Add(label);
        }
        return result;
    }

    /// <summary>Draws a canvas's bounds: identity for screen space, camera projection for world space.</summary>
    protected virtual void DrawCanvasBounds(SpriteBatch batch, CanvasComponent canvas)
    {
        if (canvas.Canvas == null) return;

        var toScreen = ResolveProjection(canvas);
        var size = new Vector2(canvas.Canvas.Width, canvas.Canvas.Height);
        var topLeft = toScreen(Vector2.Zero);
        var bottomRight = toScreen(size);

        _primitives.DrawRectangle(batch,
            new Rectangle((int)topLeft.X, (int)topLeft.Y,
                (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y)),
            canvas.IsScreenSpace ? Color.DarkSlateGray : Color.DarkOrange, 2f);
    }

    /// <summary>
    /// Draws a label's rendered bounds (in the label's own text color) and a white cross at the
    /// reference point its alignment targets. Geometry is computed in canvas-local coordinates and
    /// pushed through the canvas's projection.
    /// </summary>
    protected virtual void DrawLabelBounds(SpriteBatch batch, LabelComponent label, CanvasComponent canvas)
    {
        if (label.Owner == null || canvas.Owner == null || canvas.Canvas == null) return;

        var toScreen = ResolveProjection(canvas);
        var rect = ComputeLabelBounds(label, canvas, toScreen);
        _primitives.DrawRectangle(batch, rect, label.TextColor, 2f);

        // White cross = the reference point the alignment targets.
        var margin = label.Owner.Position - canvas.Owner.Position;
        var canvasSize = new Vector2(canvas.Canvas.Width, canvas.Canvas.Height);
        float refX = label.HorizontalAlignment switch
        {
            HorizontalAlignment.Center => canvasSize.X * 0.5f,
            HorizontalAlignment.Right => canvasSize.X,
            _ => 0f
        };
        float refY = label.VerticalAlignment switch
        {
            VerticalAlignment.Center => canvasSize.Y * 0.5f,
            VerticalAlignment.Bottom => canvasSize.Y,
            _ => 0f
        };
        var cross = toScreen(margin + new Vector2(refX, refY));
        _primitives.DrawLine(batch, cross + new Vector2(-6, 0), cross + new Vector2(6, 0), Color.White, 2f);
        _primitives.DrawLine(batch, cross + new Vector2(0, -6), cross + new Vector2(0, 6), Color.White, 2f);
    }

    /// <summary>
    /// Computes a label's rendered bounds rectangle in screen space. Pure geometry (no graphics
    /// device required) so unit tests can assert the alignment math directly.
    /// </summary>
    public static Rectangle ComputeLabelBounds(LabelComponent label, CanvasComponent canvas, Func<Vector2, Vector2> toScreen)
    {
        var margin = label.Owner.Position - canvas.Owner.Position;
        var canvasSize = new Vector2(canvas.Canvas!.Width, canvas.Canvas.Height);
        var size = new Vector2(label.Width * label.Scale.X, label.Height * label.Scale.Y);

        float offX = label.HorizontalAlignment switch
        {
            HorizontalAlignment.Center => canvasSize.X * 0.5f - size.X * 0.5f,
            HorizontalAlignment.Right => canvasSize.X - size.X,
            _ => 0f
        };
        float offY = label.VerticalAlignment switch
        {
            VerticalAlignment.Center => canvasSize.Y * 0.5f - size.Y * 0.5f,
            VerticalAlignment.Bottom => canvasSize.Y - size.Y,
            _ => 0f
        };

        var topLeft = toScreen(margin + new Vector2(offX, offY));
        var bottomRight = toScreen(margin + new Vector2(offX + size.X, offY + size.Y));
        return new Rectangle((int)topLeft.X, (int)topLeft.Y,
            (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y));
    }

    /// <summary>
    /// Resolves the canvas-local → screen projection: identity for screen-space canvases, main-camera
    /// world-to-screen for world-space ones. Returns identity when no main camera exists (tests).
    /// </summary>
    protected virtual Func<Vector2, Vector2> ResolveProjection(CanvasComponent canvas)
    {
        if (canvas.IsScreenSpace)
            return p => p;

        var cam = CoreEssentials.Camera.Camera.MainCamera;
        if (cam == null)
            return p => p;

        var origin = canvas.Owner.Position;
        return p => cam.WorldToScreen(origin + p);
    }
}
