using Microsoft.Xna.Framework;
using Myra.Graphics2D.Brushes;
using Myra.Graphics2D.UI;
using System.Collections.Generic;
using CoreEssentials.Inputs;
using MonoGame.Extended.Input.InputListeners;
using CoreEssentials.GUI;

namespace CoreEssentials.Debugging
{
    public class Console
    {
        private List<string> _lines;

        private VerticalStackPanel _panel;


        public Console() { 
            _lines = new List<string>();

            Input.Keyboard.KeyPressed += ToggleGUI;
        }

        ~Console() {
            Input.Keyboard.KeyPressed -= ToggleGUI;
        }

        private void ToggleGUI(object sender, KeyboardEventArgs e)
        {
            if(e.Key == Microsoft.Xna.Framework.Input.Keys.C)
                this._panel.Visible = !this._panel.Visible;
        }

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
