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
        static Desktop _desktop;

        static Panel Root { get { return (Panel)_desktop.Root; } }

        public static int Width { get { return (int)Root.Width; } }
        public static int Height { get { return (int)Root.Height; } }

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

            if (widget is ComboBox)
            {
                ComboBox comboBox = (ComboBox)widget;

                return comboBox.ListBox.IsMouseInside || comboBox.ListBox.IsTouchInside;
            }

            return widget.IsMouseInside || widget.IsTouchInside || widget.IsKeyboardFocused;
        }

        /// <summary>
        /// Draws all GUI elements.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public static void Draw(GameTime gameTime)
        {
            _desktop.Render();
        }
    }
}
