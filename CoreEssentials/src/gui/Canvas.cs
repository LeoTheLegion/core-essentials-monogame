
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

namespace CoreEssentials.GUI;

public class Canvas
{
    private Vector2 Position { get; set; }

    private Panel _rootPanel;

    public Canvas()
    {
        _rootPanel = new Panel();
     
        Position = Vector2.Zero;

        GUIManager.AddWidget(_rootPanel);
    }    /// <summary>
    /// Sets the position of the canvas in screen coordinates.
    /// </summary>
    /// <param name="position">The position to set.</param>
    public void SetPosition(Vector2 position)
    {
        Position = position;
        
        // Update the panel position immediately
        _rootPanel.Left = (int)position.X;
        _rootPanel.Top = (int)position.Y;
    }

    public void AddWidget(Widget widget)
    {
        _rootPanel.Widgets.Add(widget);
    }

    public void RemoveWidget(Widget widget)
    {
        _rootPanel.Widgets.Remove(widget);
    }

    public void CleanUp()
    {
        _rootPanel.Widgets.Clear();
        GUIManager.RemoveWidget(_rootPanel);
    }

    public void Update(GameTime gameTime)
    {
        _rootPanel.Left = (int)Position.X;
        _rootPanel.Top = (int)Position.Y;
    }
}
