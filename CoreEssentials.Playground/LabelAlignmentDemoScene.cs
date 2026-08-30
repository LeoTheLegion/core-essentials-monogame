using System;
using System.Collections;
using CoreEssentials.Coroutines;
using CoreEssentials.Debugging;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GUI.Types;
using CoreEssentials.Inputs;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground;

/// <summary>
/// Demonstrates AutoWidth/AutoHeight (labels report their real measured size) and container-style
/// HorizontalAlignment/VerticalAlignment on LabelComponent, in both canvas spaces:
///
///   Screen space — three labels, one per row, positioned by alignment ALONE (no anchor
///                  positions): LEFT hugs the canvas's left edge, CENTER sits in the middle,
///                  RIGHT hugs the right edge. Each label's text shows its own measured Width.
///   World space  — a floating panel that follows an orbiting target entity; its title is
///                  centered and its caption bottom-centered inside the panel by alignment alone.
///
/// A debug overlay draws each canvas (parent) bounds, each label's rendered bounds in its color,
/// and a white cross at the reference point its alignment targets.
///
/// Controls:
///   WASD — pan camera (built into CameraEntity)
///   Q/E  — zoom (built into CameraEntity)
///   R    — reset camera
///   Esc  — back to the SendMessage demo
/// </summary>
public class LabelAlignmentDemoScene : Scene
{
    private EntitySystem? _entitySystem;
    private CameraEntity? _cameraEntity;

    // Screen-space HUD labels (one row, each anchored where its name says, different alignment).
    private LabelComponent? _leftLabel;
    private LabelComponent? _centerLabel;
    private LabelComponent? _rightLabel;

    // World-space panel + its orbiting target.
    private Entity? _panelEntity;
    private LabelComponent? _panelTitle;
    private LabelComponent? _panelCaption;
    private float _orbitTime;

    // Stored as a field so Unload can unsubscribe the exact same delegate instance.
    private EventHandler<KeyboardEventArgs>? _handleKey;

    // The per-frame loop coroutine, stopped on unload so it can't touch a dead scene.
    private Guid _frameLoopId = Guid.Empty;

    // Debug overlay: the canvases (for their bounds) and shape drawing.
    private ICanvas? _hudCanvas;
    private ICanvas? _panelCanvas;
    private readonly Primitives _primitives = new();

    protected override GameSystem[] LoadGameSystems() => new GameSystem[] { new EntitySystem() };

    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.2f, "Initializing label alignment demo...");
        yield return null;

        _entitySystem = GetGameSystem<EntitySystem>();

        // Camera: WASD/QE/R panning and zoom come free from CameraEntity. The default pan
        // speed of 1 world unit/second is imperceptible, so raise it (same as the anchor demo).
        _cameraEntity = _entitySystem.CreateEntity<CameraEntity>();
        _cameraEntity.CameraSpeed = 300f;
        _cameraEntity.Position = new Vector2(640, 360);

        UpdateLoadingProgress(0.5f, "Building screen-space HUD...");
        yield return null;
        BuildScreenSpaceHud();

        UpdateLoadingProgress(0.8f, "Building world-space panel...");
        yield return null;
        BuildWorldSpacePanel();

        _handleKey = (sender, args) => HandleKeyPressed(sender, args);
        Input.Keyboard.KeyReleased += _handleKey;

        UpdateLoadingProgress(1.0f, "Label alignment demo ready!");
        Console.WriteLine("Label alignment demo loaded. WASD pans, Q/E zooms, Esc exits.");

        // Scene.Update is not virtual in this codebase, so per-frame work runs in its own
        // coroutine — it must NOT block OnStartCoroutine, which the scene's load routine awaits.
        _frameLoopId = CoroutineManager.StartCoroutine(RunFrameLoop());
        yield break;
    }

    private IEnumerator RunFrameLoop()
    {
        var lastTime = DateTime.UtcNow;
        while (true)
        {
            var now = DateTime.UtcNow;
            var dt = (float)(now - lastTime).TotalSeconds;
            lastTime = now;

            if (_entitySystem != null && _panelEntity != null)
            {
                // Orbit the panel around screen-center in world space so camera panning/zooming
                // visibly carries it along. Radius is small enough to stay on-screen at default
                // zoom (zooming out far enough will push it off — that's real world-space behavior).
                _orbitTime += dt;
                _panelEntity.Position = new Vector2(
                    640 + (float)Math.Cos(_orbitTime * 0.6f) * 150f,
                    360 + (float)Math.Sin(_orbitTime * 0.6f) * 90f);

                // Refresh the HUD labels a couple of times a second so the measured-width text
                // is visible without thrashing every frame.
                if (_leftLabel != null && _orbitTime - _lastHudRefresh >= 0.5f)
                {
                    RefreshHudLabels();
                    _lastHudRefresh = _orbitTime;
                }
            }

            yield return null;
        }
    }

    public override void Unload()
    {
        base.Unload();
        if (_frameLoopId != Guid.Empty)
        {
            CoroutineManager.StopCoroutine(_frameLoopId);
            _frameLoopId = Guid.Empty;
        }
        if (_handleKey != null)
            Input.Keyboard.KeyReleased -= _handleKey;
        _handleKey = null;
    }

    private void HandleKeyPressed(object sender, KeyboardEventArgs args)
    {
        if (args.Key == Microsoft.Xna.Framework.Input.Keys.Escape)
            SceneManager.LoadScene(new SendMessageDemoScene());
    }

    // ===== Debug overlay: make the invisible parts visible =====

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        base.Draw(gameTime, spriteBatch);

        // The scene's SpriteBatch is not started by MainGame — each game system manages its own
        // Begin/End. This overlay draws in screen space (identity transform), so it needs its own
        // Begin/End pair after the systems have finished.
        spriteBatch.Begin();

        // The screen-space canvas IS the viewport — draw its bounds so the "canvas" is visible.
        if (_hudCanvas != null)
            _primitives.DrawRectangle(spriteBatch,
                new Rectangle(0, 0, (int)_hudCanvas.Width, (int)_hudCanvas.Height),
                Color.DarkSlateGray, 2f);

        // Each label's actual rendered bounds (its color) + a white cross at the reference point
        // its alignment targets. Left/Top: top-left corner on the cross; Center: middle on it;
        // Right/Bottom: bottom-right corner on it.
        DrawLabelBounds(spriteBatch, _leftLabel, Color.CornflowerBlue, p => p);
        DrawLabelBounds(spriteBatch, _centerLabel, Color.LimeGreen, p => p);
        DrawLabelBounds(spriteBatch, _rightLabel, Color.OrangeRed, p => p);

        // World-space panel: its parent canvas bounds and labels, projected through the camera.
        var cam = CoreEssentials.Camera.Camera.MainCamera;
        if (_panelEntity != null && cam != null)
        {
            Func<Vector2, Vector2> project = p => cam.WorldToScreen(_panelEntity!.Position + p);

            if (_panelCanvas != null)
            {
                var topLeft = project(Vector2.Zero);
                var bottomRight = project(new Vector2(_panelCanvas.Width, _panelCanvas.Height));
                _primitives.DrawRectangle(spriteBatch,
                    new Rectangle((int)topLeft.X, (int)topLeft.Y,
                        (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y)),
                    Color.DarkOrange, 2f);
            }

            DrawLabelBounds(spriteBatch, _panelTitle, Color.Gold, project);
            DrawLabelBounds(spriteBatch, _panelCaption, Color.LightGray, project);
        }

        spriteBatch.End();
    }

    /// <summary>
    /// Draws a label's rendered bounds (in <paramref name="color"/>) and a white cross at the
    /// reference point its alignment targets. All geometry is computed in canvas-local
    /// coordinates and pushed through <paramref name="toScreen"/> (identity for screen-space
    /// canvases, camera projection for world-space ones).
    /// </summary>
    private void DrawLabelBounds(SpriteBatch spriteBatch, LabelComponent? label, Color color,
        Func<Vector2, Vector2> toScreen)
    {
        if (label?.Owner == null) return;

        var canvasComponent = CanvasComponent.FindCanvas(label.Owner);
        if (canvasComponent?.Owner == null || canvasComponent.Canvas == null) return;

        // The entity's position relative to the canvas entity is the margin.
        var margin = label.Owner.Position - canvasComponent.Owner.Position;
        var canvasSize = new Vector2(canvasComponent.Canvas.Width, canvasComponent.Canvas.Height);
        var size = new Vector2(label.Width * label.Scale.X, label.Height * label.Scale.Y);

        // Where the top-left corner sits inside the canvas for this alignment.
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
        _primitives.DrawRectangle(spriteBatch,
            new Rectangle((int)topLeft.X, (int)topLeft.Y,
                (int)(bottomRight.X - topLeft.X), (int)(bottomRight.Y - topLeft.Y)), color, 2f);

        // White cross = the reference point the alignment targets.
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
        _primitives.DrawLine(spriteBatch, cross + new Vector2(-6, 0), cross + new Vector2(6, 0), Color.White, 2f);
        _primitives.DrawLine(spriteBatch, cross + new Vector2(0, -6), cross + new Vector2(0, 6), Color.White, 2f);
    }

    // ===== Screen space: three labels on one position, one per alignment =====

    private void BuildScreenSpaceHud()
    {
        // HUD root sits at the origin; its canvas is screen-space, so widget positions
        // are screen coordinates regardless of camera movement.
        var hudRoot = _entitySystem!.CreateEntity<GameObjectEntity>();
        hudRoot.Position = Vector2.Zero;
        hudRoot.AddComponent(new CanvasComponent(isScreenSpace: true));

        // No anchor positions needed — the alignment alone places each label inside the canvas:
        // LEFT hugs the left edge, CENTER sits in the middle, RIGHT hugs the right edge. One
        // host entity per label (an entity can only host one LabelComponent), one row each so
        // they don't overlap; the small y values are margins from the top edge.
        _leftLabel = AddHudLabel(hudRoot, new Vector2(16, 140), "LEFT", new LabelComponent("LEFT")
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            TextColor = Color.CornflowerBlue
        });

        _centerLabel = AddHudLabel(hudRoot, new Vector2(0, 190), "CENTER", new LabelComponent("CENTER")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.LimeGreen
        });

        _rightLabel = AddHudLabel(hudRoot, new Vector2(-16, 240), "RIGHT", new LabelComponent("RIGHT")
        {
            HorizontalAlignment = HorizontalAlignment.Right,
            TextColor = Color.OrangeRed
        });

        // Instructions in the bottom-left corner (default Left/Top alignment) — the top-left
        // is taken by the playground's built-in FPS/debug overlay.
        var infoHost = _entitySystem.CreateEntity<GameObjectEntity>();
        infoHost.LocalPosition = new Vector2(16, 620);
        hudRoot.AddChild(infoHost);
        var canvas = _hudCanvas = hudRoot.GetComponent<CanvasComponent>()!.Canvas;
        infoHost.AddComponent(new LabelComponent(
            $"Container alignment: no anchor positions — LEFT hugs the canvas left edge, CENTER sits in the middle, RIGHT hugs the right edge.\n" +
            $"HUD canvas is auto-sized and reports the viewport: W={canvas.Width:0} H={canvas.Height:0}. Each label shows its own measured Width.\n" +
            "WASD pan / Q-E zoom / R reset camera. The HUD stays put while the world-space panel moves.\n" +
            "Esc returns to the SendMessage demo."));

        RefreshHudLabels();
    }

    // Creates a host entity under the HUD root and attaches the label to it. The position is a
    // margin from the aligned reference point — alignment does the actual positioning.
    private LabelComponent AddHudLabel(GameObjectEntity hudRoot, Vector2 margin, string name, LabelComponent label)
    {
        var host = _entitySystem!.CreateEntity<GameObjectEntity>();
        host.LocalPosition = margin; // HUD root is at origin, so local == world
        hudRoot.AddChild(host);
        return host.AddComponent(label);
    }

    private void RefreshHudLabels()
    {
        if (_leftLabel == null || _centerLabel == null || _rightLabel == null)
            return;

        // The text itself embeds the measured width — it changes as the text changes,
        // and the alignment keeps each label anchored correctly every frame.
        _leftLabel.Text = $"LEFT   (W={_leftLabel.Width:0} H={_leftLabel.Height:0})";
        _centerLabel.Text = $"CENTER (W={_centerLabel.Width:0} H={_centerLabel.Height:0})";
        _rightLabel.Text = $"RIGHT  (W={_rightLabel.Width:0} H={_rightLabel.Height:0})";
    }

    // ===== World space: floating panel following an orbiting target =====

    private void BuildWorldSpacePanel()
    {
        // The panel is a world-space canvas on its own entity; it tracks the target
        // every frame, so panning/zooming the camera moves it with the world.
        _panelEntity = _entitySystem!.CreateEntity<GameObjectEntity>();
        _panelEntity.Position = new Vector2(640, 360);
        _panelCanvas = _panelEntity.AddComponent(new CanvasComponent(isScreenSpace: false)
        {
            Width = 280f,   // explicit size — Canvas setters pin auto-sizing on set
            Height = 150f
        }).Canvas;

        // Title centered horizontally near the panel's top edge — alignment alone, with a small
        // y margin. No x position needed: Center means "middle of the canvas".
        var titleHost = _entitySystem.CreateEntity<GameObjectEntity>();
        titleHost.LocalPosition = new Vector2(0, 8);
        _panelEntity.AddChild(titleHost);
        _panelTitle = titleHost.AddComponent(new LabelComponent("World-space panel")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            TextColor = Color.Gold
        });

        // Caption centered horizontally on the panel's bottom edge (12px margin up), reporting
        // the explicit canvas size.
        var captionHost = _entitySystem.CreateEntity<GameObjectEntity>();
        captionHost.LocalPosition = new Vector2(0, -12);
        _panelEntity.AddChild(captionHost);
        _panelCaption = captionHost.AddComponent(new LabelComponent("")
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Bottom,
            TextColor = Color.LightGray
        });

        _panelCaption.Text = $"Canvas: W={_panelCanvas!.Width:0} H={_panelCanvas.Height:0} (pinned)";
    }

    private float _lastHudRefresh = -999f;
}
