using Microsoft.Xna.Framework;
using Myra;
using Myra.Graphics2D.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//https://github.com/rds1983/Myra/wiki

namespace CoreEssentials.GUI
{
    /// <summary>
    /// Manages GUI elements and rendering for the application.
    /// Provides a centralized system for creating, maintaining and rendering UI components.
    /// </summary>
    public static class GUIManager
    {
        static Desktop? _desktop;

        static Panel Root
        {
            get
            {
                if (_desktop == null)
                    throw new InvalidOperationException("GUIManager has not been initialized. Call Init first.");

                return (Panel)_desktop.Root;
            }
        }

        /// <summary>
        /// Gets the width of the GUI root panel in pixels.
        /// </summary>
        public static int Width { get { return Root.Width ?? 0; } }

        /// <summary>
        /// Gets the height of the GUI root panel in pixels.
        /// </summary>
        public static int Height { get { return Root.Height ?? 0; } }

        /// <summary>
        /// Initializes the GUI manager with the specified game instance and window dimensions.
        /// </summary>
        /// <param name="game">The game instance that will host the GUI.</param>
        /// <param name="width">The width of the window in pixels.</param>
        /// <param name="height">The height of the window in pixels.</param>
        public static void Init(Game game, int width, int height)
        {
            Panel panel = new Panel();
            panel.Width = width;
            panel.Height = height;
            // Add it to the desktop
            _desktop = new Desktop();
            _desktop.Root = panel;
        }

        /// <summary>
        /// Adds a widget to the GUI desktop.
        /// </summary>
        /// <param name="widget">The widget to add to the GUI.</param>
        public static void AddWidget(Widget widget)
        {
            Root.Widgets.Add(widget);
        }

        /// <summary>
        /// Removes a widget from the GUI desktop.
        /// </summary>
        /// <param name="widget">The widget to remove from the GUI.</param>
        public static void RemoveWidget(Widget widget)
        {
            Root.Widgets.Remove(widget);
        }

        /// <summary>
        /// Determines whether any widget in the GUI currently has focus.
        /// </summary>
        /// <returns><c>true</c> if any widget is focused; otherwise, <c>false</c>.</returns>
        public static bool IsAnyWidgetFocused()
        {
            for (int i = 0; i < Root.Widgets.Count; i++)
            {
                Widget w = Root.Widgets[i];
                if(isWidgetFocused(w))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the specified widget currently has focus.
        /// </summary>
        /// <param name="w">The widget to check.</param>
        /// <returns><c>true</c> if the widget is focused; otherwise, <c>false</c>.</returns>
        public static bool IsWidgetFocused(Widget w)
        {
            return isWidgetFocused(w);
        }

        static bool isWidgetFocused(Widget widget)
        {
            if (widget == null)
                return false;

            if (widget is Container)
            {
                Container container = (Container)widget;
                for (int i = 0; i < container.Widgets.Count; i++)
                {
                    Widget w = container.Widgets[i];
                    if (isWidgetFocused(w))
                        return true;
                }
            }

            if (widget is ContentControl)
            {
                ContentControl cc = (ContentControl)widget;

                if(cc.IsMouseInside || cc.IsTouchInside || cc.IsKeyboardFocused)
                {
                    return true;
                }

                if (isWidgetFocused(cc.Content)){
                    return true;
                }
            }

            if (widget is ComboView)
            {
                ComboView comboView = (ComboView)widget;

                return comboView.ListView.IsMouseInside || comboView.ListView.IsTouchInside;
            }

            return widget.IsMouseInside || widget.IsTouchInside || widget.IsKeyboardFocused;
        }

        /// <summary>
        /// Draws all GUI elements.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Draw(GameTime gameTime)
        {
            if (_desktop == null)
                return;

            _desktop.Render();
        }
    }
}
