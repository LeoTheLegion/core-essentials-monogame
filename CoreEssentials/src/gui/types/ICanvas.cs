using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Canvas implementation that manages widgets in either screen space or world space.
/// Unlike the interface types, this is a concrete class since it requires mutable state
/// and constructor behavior for positioning logic.
/// </summary>
public class ICanvas
{
    private readonly IList<IWidget> _children = new List<IWidget>();
    private Vector2 _position;
    private bool _isScreenSpace;

    /// <summary>
    /// Gets or sets the position of this canvas.
    /// In screen space, this is absolute screen coordinates.
    /// In world space, this is a world coordinate converted to screen space during update.
    /// </summary>
    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;
            OnPositionChanged();
        }
    }

    /// <summary>
    /// Gets a value indicating whether this canvas is in screen space (true) or world space (false).
    /// </summary>
    public bool IsScreenSpace => _isScreenSpace;

    /// <summary>
    /// Gets the collection of child widgets attached to this canvas.
    /// </summary>
    public IList<IWidget> Children => _children;

    /// <summary>
    /// Initializes a new instance of the ICanvas class with specified space type and position.
    /// </summary>
    /// <param name="isScreenSpace">If true, canvas operates in screen space; if false, in world space.</param>
    /// <param name="position">The initial position of this canvas.</param>
    public ICanvas(bool isScreenSpace = true, Vector2? position = null)
    {
        _isScreenSpace = isScreenSpace;
        _position = position ?? Vector2.Zero;
    }

    /// <summary>
    /// Gets or sets the width of this canvas in pixels.
    /// </summary>
    public float Width { get; set; }

    /// <summary>
    /// Gets or sets the height of this canvas in pixels.
    /// </summary>
    public float Height { get; set; }

    /// <summary>
    /// Sets the position of this canvas.
    /// </summary>
    /// <param name="position">The new position.</param>
    public void SetPosition(Vector2 position)
    {
        _position = position;
        OnPositionChanged();
    }

    /// <summary>
    /// Adds a widget to this canvas.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    public void AddWidget(IWidget widget)
    {
        _children.Add(widget);
    }

    /// <summary>
    /// Removes a widget from this canvas.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    public void RemoveWidget(IWidget widget)
    {
        _children.Remove(widget);
    }

    /// <summary>
    /// Called when the position of this canvas changes. Override for custom behavior.
    /// </summary>
    protected virtual void OnPositionChanged() { }

    /// <summary>
    /// Cleans up this canvas by removing all widgets and releasing resources.
    /// </summary>
    public virtual void CleanUp()
    {
        _children.Clear();
    }

    /// <summary>
    /// Updates this canvas based on its space type and current state.
    /// In world space, this may convert world coordinates to screen coordinates.
    /// </summary>
    /// <param name="gameTime">Provides timing values for the update.</param>
    public virtual void Update(GameTime gameTime) { }
}
