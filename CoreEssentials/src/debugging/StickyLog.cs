using CoreEssentials.GUI;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Input.InputListeners;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System.Collections.Generic;

namespace CoreEssentials.Debugging
{
    public class StickyLog
    {

        private Grid _grid;
        private Dictionary<string, Label> log = new Dictionary<string, Label>();

        public StickyLog() {
            Input.Keyboard.KeyPressed += ToggleGUI;
        }

        ~StickyLog() {
            Input.Keyboard.KeyPressed -= ToggleGUI;

            if (_grid != null)
                GUIManager.RemoveWidget(_grid);
        }
        private void ToggleGUI(object sender, KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.R)
                this._grid.Visible = !this._grid.Visible;
        }

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
