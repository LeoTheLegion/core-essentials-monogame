using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System.Collections.Generic;
using CoreEssentials.Inputs;
using MonoGame.Extended.Input.InputListeners;
using CoreEssentials.GUI;

namespace CoreEssentials.Debugging
{
    /// <summary>
    /// Provides an interactive in-game console for debugging and runtime commands.
    /// The console allows developers to view logs and execute commands during gameplay.
    /// </summary>
    public class Console
    {
        private List<string> _lines;

        private VerticalStackPanel _panel;

        /// <summary>
        /// Initializes a new instance of the Console class.
        /// Sets up key handlers for toggling visibility.
        /// </summary>
        public Console() { 
            _lines = new List<string>();

            Input.Keyboard.KeyPressed += ToggleGUI;
        }

        /// <summary>
        /// Finalizes an instance of the Console class.
        /// Removes key handlers to prevent memory leaks.
        /// </summary>
        ~Console() {
            Input.Keyboard.KeyPressed -= ToggleGUI;
        }

        /// <summary>
        /// Toggles the visibility of the console panel.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The keyboard event arguments.</param>
        private void ToggleGUI(object sender, KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.C)
                this._panel.Visible = !this._panel.Visible;
        }

        /// <summary>
        /// Writes a message to the console log.
        /// </summary>
        /// <param name="line">The message to write.</param>
        public void WriteLine(string line)
        {
            if(line.IndexOf('\n') >= 0)
            {
                var lines = line.Split('\n');

                foreach (var l in lines)
                {
                    _lines.Add(l);

                    if (_panel != null)
                        AddTextWidget(l);
                }
            }
            else
            {
                _lines.Add(line);

                if (_panel != null)
                    AddTextWidget(line);
            }

        }

        /// <summary>
        /// Creates and initializes the console's UI components.
        /// </summary>
        public void LoadGUI()
        {
            _panel = new VerticalStackPanel()
            {
                Spacing = 8,
            };

            Color c = Color.Black;
            c.A = 175;

            _panel.Background = new SolidBrush(c);

            _panel.Visible = false;

            GUIManager.AddWidget(_panel);

            if( _lines.Count > 0 )
            {
                foreach( string line in _lines )
                {
                    AddTextWidget(line) ;
                }
            }
        }

        /// <summary>
        /// Adds a text widget to the console panel.
        /// </summary>
        /// <param name="text">The text to display in the widget.</param>
        private void AddTextWidget(string text)
        {
            var textBlock = new Label();
            textBlock.Text = text;
            _panel.Widgets.Add(textBlock);

            if(_panel.Widgets.Count > 38)
            {
                _panel.Widgets.RemoveAt(0);
            }
        }
    }
}
