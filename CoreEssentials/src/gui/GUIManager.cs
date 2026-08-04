using Microsoft.Xna.Framework;
using CoreEssentials.GUI.Internal;
using CoreEssentials.GUI.Types;

namespace CoreEssentials.GUI
{
    /// <summary>
    /// Manages GUI elements and rendering for the application.
    /// Provides a centralized system for creating, maintaining and rendering UI components.
    /// This static class delegates all operations to the active GUI engine backend (default: Myra-based GuiManagerImpl).
    /// </summary>
    public static class GUIManager
    {
        private static IGuiManager Engine => EngineResolver.GetEngine();

        /// <summary>
        /// Gets the width of the GUI root panel in pixels.
        /// </summary>
        public static int Width => Engine.Width;

        /// <summary>
        /// Gets the height of the GUI root panel in pixels.
        /// </summary>
        public static int Height => Engine.Height;

        /// <summary>
        /// Initializes the GUI manager with the specified game instance and window dimensions.
        /// </summary>
        /// <param name="game">The game instance that will host the GUI.</param>
        /// <param name="width">The width of the window in pixels.</param>
        /// <param name="height">The height of the window in pixels.</param>
        public static void Init(Game game, int width, int height) => Engine.Init(game, width, height);

        /// <summary>
        /// Adds a widget to the GUI root hierarchy.
        /// </summary>
        /// <param name="widget">The widget to add (must be created via WidgetFactory or wrapper).</param>
        public static void AddWidget(IWidget widget) => Engine.AddWidget(widget);

        /// <summary>
        /// Removes a widget from the GUI root hierarchy.
        /// </summary>
        /// <param name="widget">The widget to remove.</param>
        public static void RemoveWidget(IWidget widget) => Engine.RemoveWidget(widget);

        /// <summary>
        /// Determines whether any widget in the GUI currently has focus.
        /// </summary>
        /// <returns><c>true</c> if any widget is focused; otherwise, <c>false</c>.</returns>
        public static bool IsAnyWidgetFocused() => Engine.IsAnyWidgetFocused();

        /// <summary>
        /// Determines whether the specified widget currently has focus.
        /// </summary>
        /// <param name="widget">The widget to check.</param>
        /// <returns><c>true</c> if the widget is focused; otherwise, <c>false</c>.</returns>
        public static bool IsWidgetFocused(IWidget? widget) => Engine.IsWidgetFocused(widget);

        /// <summary>
        /// Draws all GUI elements.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Draw(GameTime gameTime) => Engine.Draw(gameTime);
    }
}
