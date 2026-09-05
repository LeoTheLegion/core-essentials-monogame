using CoreEssentials.GUI.Types;
using CoreEssentials.GUI.Internal;
using CoreEssentials.GUI.Engines.Myra.Widgets;
using Microsoft.Xna.Framework;
using Panel = global::Myra.Graphics2D.UI.Panel;

namespace CoreEssentials.GUI.Engines.Myra;

/// <summary>
/// Myra-based implementation of ICanvas. Wraps a Panel as the root container and handles screen/world space positioning.
/// </summary>
public class CanvasImpl : ContainerWidget, ICanvas
{
    private readonly IGuiManager _manager;
    private Vector2 _position;
    private bool _isScreenSpace;

    /// <summary>
    /// Tracks whether this canvas is currently registered in the global GUI root. Registration is
    /// deferred until the first pump (<see cref="Update"/>), so a canvas only renders once its owning
    /// scene actually starts updating — not while it is still loading or after it has been unloaded.
    /// </summary>
    private bool _isRegistered;

    /// <inheritdoc />
    public bool IsScreenSpace
    {
        get => _isScreenSpace;
        set => _isScreenSpace = value;
    }

    /// <summary>
    /// Gets the underlying Myra Panel instance.
    /// </summary>
    internal Panel MyraPanel => Panel;

    /// <inheritdoc />
    public CanvasImpl(bool isScreenSpace = true) : base(new Panel())
    {
        _isScreenSpace = isScreenSpace;
        _position = Vector2.Zero;
        _manager = EngineResolver.GetEngine();
        // Registration into the global GUI is deferred to the first Update() — see EnsureRegistered.
    }

    /// <summary>
    /// Registers this canvas in the global GUI root on first use. A canvas belongs to a scene and is
    /// pumped only while that scene is current, so its first pump is the moment it should start
    /// rendering. Deferring registration (rather than doing it in the constructor) keeps canvases of
    /// a still-loading or already-unloaded scene out of the global render list. Safe to call repeatedly.
    /// </summary>
    private void EnsureRegistered()
    {
        if (_isRegistered)
            return;

        _manager.AddWidget(this);
        _isRegistered = true;
    }

    /// <summary>
    /// Gets the canvas width. A screen-space canvas IS the screen, so while auto-sized (the
    /// default) it reports the GUI viewport size instead of a content measurement. World-space
    /// canvases keep the base behavior: auto means measured content size.
    /// </summary>
    public override float Width
    {
        get => _isScreenSpace && AutoWidth ? _manager.Width : base.Width;
        set => base.Width = value;
    }

    /// <summary>
    /// Gets the canvas height. A screen-space canvas IS the screen, so while auto-sized (the
    /// default) it reports the GUI viewport size instead of a content measurement. World-space
    /// canvases keep the base behavior: auto means measured content size.
    /// </summary>
    public override float Height
    {
        get => _isScreenSpace && AutoHeight ? _manager.Height : base.Height;
        set => base.Height = value;
    }

    /// <inheritdoc />
    public void SetPosition(Vector2 position)
    {
        _position = position;
        Panel.Left = (int)_position.X;
        Panel.Top = (int)_position.Y;
    }

    /// <inheritdoc />
    public void AddWidget(IWidget widget)
    {
        AddChild(widget);
    }

    /// <inheritdoc />
    public void RemoveWidget(IWidget widget)
    {
        RemoveChild(widget);
    }

    /// <inheritdoc />
    public void Update(GameTime gameTime)
    {
        // First pump = the owning scene is now live, so attach to the global GUI from here on.
        EnsureRegistered();

        if (!_isScreenSpace)
        {
            var camera = CoreEssentials.Camera.Camera.MainCamera;
            if (camera != null)
            {
                Vector2 screenPosition = camera.WorldToScreen(_position);
                Panel.Left = (int)screenPosition.X;
                Panel.Top = (int)screenPosition.Y;
                return;
            }
        }

        Panel.Left = (int)_position.X;
        Panel.Top = (int)_position.Y;
    }

    /// <inheritdoc />
    public void CleanUp()
    {
        ClearChildren();
        if (_isRegistered)
        {
            _manager.RemoveWidget(this);
            _isRegistered = false;
        }
    }
}
