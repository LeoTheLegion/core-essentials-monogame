using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;
using CoreEssentials.Cameras;

namespace CoreEssentials.GUI;

/// <summary>
/// Represents a UI canvas that can contain and manage widgets in either screen space or world space.
/// </summary>
public class Canvas
{
    /// <summary>
    /// Gets or sets the position of the canvas.
    /// </summary>
    private Vector2 Position { get; set; }

    /// <summary>
    /// The root panel that contains all widgets added to this canvas.
    /// </summary>
    private Panel _rootPanel;

    /// <summary>
    /// Indicates whether the canvas is in screen space (true) or world space (false).
    /// In screen space, the canvas is positioned relative to the screen coordinates.
    /// In world space, the canvas is positioned relative to the game world coordinates.
    /// </summary>
    private bool _isScreenSpace = true;

    /// <summary>
    /// Initializes a new instance of the Canvas class with specified space type.
    /// </summary>
    /// <param name="isScreenSpace">If true, canvas will be in screen space; if false, in world space.</param>
    public Canvas(bool isScreenSpace)
    {
        _rootPanel = new Panel();

        Position = Vector2.Zero;

        GUIManager.AddWidget(_rootPanel);

        this._isScreenSpace = isScreenSpace;
    }

    /// <summary>
    /// Initializes a new instance of the Canvas class in screen space by default.
    /// </summary>
    public Canvas() : this(true)
    {
    }

    /// <summary>
    /// Sets the position of the canvas in screen coordinates.
    /// </summary>
    /// <param name="position">The position to set.</param>
    public void SetPosition(Vector2 position)
    {
        Position = position;

        // Update the panel position immediately
        _rootPanel.Left = (int)position.X;
        _rootPanel.Top = (int)position.Y;
    }

    /// <summary>
    /// Adds a widget to the canvas.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    public void AddWidget(Widget widget)
    {
        _rootPanel.Widgets.Add(widget);
    }

    /// <summary>
    /// Removes a widget from the canvas.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    public void RemoveWidget(Widget widget)
    {
        _rootPanel.Widgets.Remove(widget);
    }

    /// <summary>
    /// Removes all widgets from the canvas and unregisters it from the GUIManager.
    /// </summary>
    public void CleanUp()
    {
        _rootPanel.Widgets.Clear();
        GUIManager.RemoveWidget(_rootPanel);
    }

    /// <summary>
    /// Updates the canvas position and state.
    /// </summary>
    /// <param name="gameTime">The game timing information.</param>
    public void Update(GameTime gameTime)
    {
        // If the canvas is in world space, update its position based on the game world coordinates
        if (!_isScreenSpace)
        {
            var camera = Camera.MainCamera;
            if (camera != null)
            {
                // Convert world position to screen position
                Vector2 screenPosition = camera.WorldToScreen(Position);
                _rootPanel.Left = (int)screenPosition.X;
                _rootPanel.Top = (int)screenPosition.Y;
                return;
            }
        }

        // In screen space, the position is already set directly
        // We can also update the panel position here if needed
        _rootPanel.Left = (int)Position.X;
        _rootPanel.Top = (int)Position.Y;

    }
}
