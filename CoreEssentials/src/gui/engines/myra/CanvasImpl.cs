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

    /// <inheritdoc />
    public bool IsScreenSpace => _isScreenSpace;

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
        _manager.AddWidget(this);
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
        if (!_isScreenSpace)
        {
            var camera = CoreEssentials.Cameras.Camera.MainCamera;
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
        _manager.RemoveWidget(this);
    }
}
