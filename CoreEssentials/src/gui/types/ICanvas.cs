namespace CoreEssentials.GUI.Types;

/// <summary>
/// Interface for UI canvas elements that manage widgets in screen space or world space.
/// </summary>
public interface ICanvas : IPanel
{
    /// <summary>
    /// Gets a value indicating whether this canvas is in screen space (true) or world space (false).
    /// </summary>
    bool IsScreenSpace { get; }

    /// <summary>
    /// Sets the position of this canvas.
    /// </summary>
    /// <param name="position">The new position in screen or world coordinates.</param>
    void SetPosition(Microsoft.Xna.Framework.Vector2 position);

    /// <summary>
    /// Adds a widget to this canvas.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    void AddWidget(IWidget widget);

    /// <summary>
    /// Removes a widget from this canvas.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    void RemoveWidget(IWidget widget);

    /// <summary>
    /// Cleans up this canvas by removing all widgets and releasing resources.
    /// </summary>
    void CleanUp();

    /// <summary>
    /// Updates this canvas state and positioning for world space rendering if applicable.
    /// </summary>
    /// <param name="gameTime">Provides timing values.</param>
    void Update(Microsoft.Xna.Framework.GameTime gameTime);
}
