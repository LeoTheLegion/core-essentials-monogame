using CoreEssentials.GUI;
using CoreEssentials.GUI.Factory;
using CoreEssentials.GUI.Internal;
using CoreEssentials.GUI.Types;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
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
        private IGrid? _grid;
        
        /// <summary>
        /// Canvas that manages the grid widget.
        /// </summary>
        private ICanvas? _canvas;
        
        /// <summary>
        /// Dictionary mapping log keys to their label UI elements.
        /// </summary>
        private Dictionary<string, ILabel> log = new Dictionary<string, ILabel>();

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

            if (_canvas != null)
                _canvas.CleanUp();
        }        
        /// <summary>
        /// Event handler that toggles the visibility of the log when the R key is pressed.
        /// </summary>
        /// <param name="sender">The source of the event, or <see langword="null"/>.</param>
        /// <param name="e">The keyboard event arguments.</param>
        private void ToggleGUI(object? sender, KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.R && _grid != null)
                this._grid.Visible = !this._grid.Visible;
        }
        
        /// <summary>
        /// Gets or sets a value indicating whether the log is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return _grid != null && _grid.Visible; }
            set { if (_grid != null) _grid.Visible = value; }
        }

        /// <summary>
        /// Creates and initializes the UI grid for displaying log entries.
        /// The static grid structure is loaded from an embedded XML layout resource,
        /// while runtime concerns (position, background brush) are set imperatively.
        /// </summary>
        public void LoadGUI()
        {
            // Initialize the Canvas via factory (returns ICanvas interface)
            _canvas = CanvasFactory.CreateScreenSpace();
            _canvas.SetPosition(new Vector2(10, 10)); // Default position, top-left with small margin
            
            // Load grid from embedded XML layout resource (~3 lines vs ~15 before)
            _grid = GuiSerializer.LoadGridFromXmlEmbedded("CoreEssentials.Content.StickyLogLayout.xml");

            // Set background imperatively (IBrush not expressible in XML)
            Color c = Color.Black;
            c.A = 100;
            _grid.Background = c.AsBrush();

            // Add the grid as a child of the canvas
            _canvas.AddChild(_grid);
        }
        
        /// <summary>
        /// Updates the position of the sticky log on screen.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            _canvas?.Update(gameTime);
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

            var keyLabel = WidgetFactory.CreateLabel(key);
            _grid.SetColumn(keyLabel, 0);
            _grid.SetRow(keyLabel, logCount);
            _grid.AddChild(keyLabel);

            var valueLabel = WidgetFactory.CreateLabel(value);
            _grid.SetColumn(valueLabel, 1);
            _grid.SetRow(valueLabel, logCount);
            _grid.AddChild(valueLabel);


            log[key] = valueLabel;
        }

        /// <summary>
        /// Sets the position of the StickyLog on the screen.
        /// </summary>
        /// <param name="position">The position in screen coordinates.</param>
        public void SetPosition(Vector2 position)
        {
            if (_canvas != null)
            {
                _canvas.SetPosition(position);
            }
        }

        /// <summary>
        /// Removes a specific log entry by key.
        /// </summary>
        /// <param name="key">The key of the log entry to remove.</param>
        public void Remove(string key)
        {
            if (_grid != null && log.ContainsKey(key))
            {
                // Find the row of the entry to remove
                int rowToRemove = -1;
                foreach (var widget in _grid.Widgets)
                {
                    if (widget is ILabel label && label == log[key])
                    {
                        rowToRemove = _grid.GetRow(widget);
                        break;
                    }
                }

                if (rowToRemove >= 0)
                {
                    // Remove the widgets for this entry
                    List<IWidget> widgetsToRemove = new List<IWidget>();
                    foreach (var widget in _grid.Widgets)
                    {
                        if (_grid.GetRow(widget) == rowToRemove)
                        {
                            widgetsToRemove.Add(widget);
                        }
                    }

                    foreach (var widget in widgetsToRemove)
                    {
                        _grid.RemoveChild(widget);
                    }

                    // Shift up the rows for widgets below the removed row
                    foreach (var widget in _grid.Widgets)
                    {
                        int row = _grid.GetRow(widget);
                        if (row > rowToRemove)
                        {
                            _grid.SetRow(widget, row - 1);
                        }
                    }

                    // Remove from dictionary
                    log.Remove(key);
                }
            }
        }

        /// <summary>
        /// Clears all log entries.
        /// </summary>
        public void Clear()
        {
            if (_grid != null)
            {
                _grid.ClearChildren();
                log.Clear();
            }
        }
    }
}
