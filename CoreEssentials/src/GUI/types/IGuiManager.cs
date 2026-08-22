using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GUI.Types;

/// <summary>
/// Main interface for managing the GUI system lifecycle, widget hierarchy, and rendering.
/// This is the primary user-facing entry point for integrating with the GUI engine.
/// </summary>
public interface IGuiManager
{
    /// <summary>
    /// Gets the width of the root panel in pixels.
    /// </summary>
    int Width { get; }

    /// <summary>
    /// Gets the height of the root panel in pixels.
    /// </summary>
    int Height { get; }

    /// <summary>
    /// Initializes this GUI manager with the game instance and window dimensions.
    /// Must be called before any other methods.
    /// </summary>
    /// <param name="game">The MonoGame Game instance hosting the GUI.</param>
    /// <param name="width">The width of the rendering area in pixels.</param>
    /// <param name="height">The height of the rendering area in pixels.</param>
    void Init(Game game, int width, int height);

    /// <summary>
    /// Shuts down this GUI manager and releases all resources.
    /// </summary>
    void Shutdown();

    /// <summary>
    /// Adds a widget to the GUI root hierarchy.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    void AddWidget(IWidget widget);

    /// <summary>
    /// Removes a widget from the GUI root hierarchy.
    /// </summary>
    /// <param name="widget">The widget to remove.</param>
    void RemoveWidget(IWidget widget);

    /// <summary>
    /// Renders all active GUI elements using the current GameTime.
    /// </summary>
    /// <param name="gameTime">Provides timing values for rendering.</param>
    void Draw(GameTime gameTime);

    /// <summary>
    /// Gets a value indicating whether any widget in the GUI currently has focus.
    /// </summary>
    bool IsAnyWidgetFocused();

    /// <summary>
    /// Gets a value indicating whether the specified widget has keyboard focus.
    /// </summary>
    /// <param name="widget">The widget to check.</param>
    bool IsWidgetFocused(IWidget? widget);

    /// <summary>
    /// Sets an optional custom desktop/root container for advanced use cases.
    /// Most users will not need this method.
    /// </summary>
    void SetDesktop(object desktop);

    /// <summary>
    /// Gets the root panel of the GUI hierarchy, if available.
    /// Returns null before Init is called or after Shutdown.
    /// </summary>
    IPanel? GetRootPanel();
}
