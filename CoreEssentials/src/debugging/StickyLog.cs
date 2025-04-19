using CoreEssentials.GUI;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System.Collections.Generic;

namespace CoreEssentials.Debugging
{
    /// <summary>
    /// Provides an on-screen log display for debugging purposes.
    /// The StickyLog creates a semi-transparent overlay that can display persistent key-value pairs,
    /// useful for showing debug information like FPS, entity counts, or other game state values.
    /// </summary>
    public class StickyLog
    {
        /// <summary>
        /// UI grid that contains all log entries.
        /// </summary>
        private Grid _grid;
        
        /// <summary>
        /// Dictionary mapping log keys to their label UI elements.
        /// </summary>
        private Dictionary<string, Label> log = new Dictionary<string, Label>();

        /// <summary>
        /// Initializes a new instance of the StickyLog class and registers the toggle key handler.
        /// </summary>
        public StickyLog() {
            Input.Keyboard.KeyPressed += ToggleGUI;
        }

        /// <summary>
        /// Finalizer that ensures proper cleanup of UI elements and event handlers.
        /// </summary>
        ~StickyLog() {
            Input.Keyboard.KeyPressed -= ToggleGUI;

            if (_grid != null)
                GUIManager.RemoveWidget(_grid);
        }
        
        /// <summary>
        /// Event handler that toggles the visibility of the log when the R key is pressed.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The keyboard event arguments.</param>
        private void ToggleGUI(object sender, KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.R)
                this._grid.Visible = !this._grid.Visible;
        }

        /// <summary>
        /// Creates and initializes the UI grid for displaying log entries.
        /// </summary>
        public void LoadGUI()
        {
            _grid = new Grid
            {
                RowSpacing = 8,
                ColumnSpacing = 8,
            };

            Color c = Color.Black;
            c.A = 100;

            _grid.Background = new SolidBrush(c);
            _grid.Width = 200;
            _grid.Height = 100;

            this._grid.Visible = true;

            GUIManager.AddWidget(_grid);
        }

        /// <summary>
        /// Logs a key-value pair to the on-screen display.
        /// If the key already exists, its value is updated. Otherwise, a new entry is created.
        /// </summary>
        /// <param name="key">The identifier for this log entry.</param>
        /// <param name="value">The value to display.</param>
        public void Log(string key, string value)
        {
            if (log.ContainsKey(key))
            {
                log[key].Text = value;
            }
            else
            {
                CreateNewLabel(key, value);
            }
        }

        /// <summary>
        /// Creates a new key-value pair of labels in the UI grid.
        /// </summary>
        /// <param name="key">The identifier text to display.</param>
        /// <param name="value">The value text to display.</param>
        private void CreateNewLabel(string key, string value)
        {
            if (_grid == null) return;

            int logCount = log.Count;

            _grid.ColumnsProportions.Add(new Proportion(ProportionType.Auto));
            _grid.RowsProportions.Add(new Proportion(ProportionType.Auto));

            var keyLabel = new Label
            {
                Text = key
            };
            Grid.SetColumn(keyLabel, 0);
            Grid.SetRow(keyLabel, logCount);
            _grid.Widgets.Add(keyLabel);

            var valueLabel = new Label
            {
                Text = value
            };
            Grid.SetColumn(valueLabel, 1);
            Grid.SetRow(valueLabel, logCount);
            _grid.Widgets.Add(valueLabel);


            log[key] = valueLabel;
        }
    }
}
